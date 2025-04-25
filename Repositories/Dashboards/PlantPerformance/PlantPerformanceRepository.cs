using Inventio.Data;
using Inventio.Models.Views;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using inventio.Models.DTO.VwPlantPerformance;
using inventio.Repositories.Dashboards.Scrap;
using inventio.Repositories.Dashboards.ChangeOver;
using inventio.Repositories.EntityFramework;
using inventio.Models.DTO.VwUtilization;
using inventio.Models.DTO.ProductivitySummary;
using System.Configuration;
using Inventio.Repositories.Formulas;
using inventio.Models.DTO.Efficiency;
using inventio.Models.DTO.ProductionSummary;
using Inventio.Utils.Formulas;


namespace inventio.Repositories.Dashboards.PlantPerformance
{
    public class PlantPerformanceRepository : IPlantPerformanceRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IScrapRepository _scrapRepository;
        //private readonly IEFRepository<DTOGetEfficiency> _PCLrepository;
        private readonly IConfiguration _configuration;
        private readonly IGeneralEfficiencyRepository _generalEfficiencyRepository;
        private readonly EfficiencyFormula _efficiencyFormula;
        private readonly UtilizationFormula _utilizationFormula;

        public PlantPerformanceRepository(ApplicationDBContext context, IScrapRepository scrapRepository, IConfiguration configuration, IGeneralEfficiencyRepository formulaEfficiency, EfficiencyFormula efficiencyFormula, UtilizationFormula utilizationFormula)
        {
            _context = context;
            _scrapRepository = scrapRepository;
            //_PCLrepository = PCLrepository;
            _configuration = configuration;
            _generalEfficiencyRepository = formulaEfficiency;
            _efficiencyFormula = efficiencyFormula;
            _utilizationFormula = utilizationFormula;
        }

        private async Task<GembaCases> GetGembaCases(GembaFilter filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Total Cases By Line

            var resultCasesByLine = await _context.ProductionSummary
            .Where(obj => obj.Date >= startDate && obj.Date <= endDate)
            .GroupBy(x => x.Line)
            .Select(g => new { Line = g.Key, Production = g.Sum(x => x.Production) }).ToListAsync();


            if (resultCasesByLine.Count == 0)
            {

                GembaCases abortG = new();
                return abortG;
            }


            int sumTotal = 0;
            List<TotalCasesByLine> tempList = new();

            for (int i = 0; i < resultCasesByLine.Count; i++)
            {

                decimal production = resultCasesByLine[i].Production ?? 0;
                sumTotal += Convert.ToInt32(production);
            }

            for (int i = 0; i < resultCasesByLine.Count; i++)
            {

                decimal production = resultCasesByLine[i].Production ?? 0;
                decimal auxSumTotal = sumTotal == 0 ? 1 : sumTotal;
                TotalCasesByLine aux = new()
                {
                    Line = resultCasesByLine[i].Line,
                    Production = Convert.ToInt32(production),
                    Percent = Math.Round((production * 100) / auxSumTotal, 1)
                };
                tempList.Add(aux);
            }

            TotalCasesByLineFormatted resultCasesByLineFormatted = new()
            {
                Data = tempList,
                SumTotal = sumTotal
            };

            GembaCases resultGembaCases = new()
            {
                TotalCasesByLine = resultCasesByLineFormatted
            };

            return resultGembaCases;

        }

        private async Task<GembaEfficiency> GetGembaEfficiency(GembaFilter filters)
        {
            // Efficiency by Line

            string categories = filters.EfficiencyCategories.Count > 0 ? string.Join(",", filters.EfficiencyCategories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var dataEfficiency = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
            {
                Categories = filters.EfficiencyCategories,
                StartDate = filters.StartDate,
                EndDate = filters.EndDate
            });


            var data = dataEfficiency.GroupBy(g => g.Line)
            .Select(g => new
            {
                Line = g.Key,
                Production = g.Sum(ps => ps.Production),
                MaxProduction = g.Sum(ps => ps.MaxProduction)
            })
            .ToList();

            /* var data = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            )
            .GroupBy(g => g.Line)
            .Select(g => new
            {
                Line = g.Key,
                Production = g.Sum(ps => ps.Production),
                MaxProduction = g.Sum(ps => ps.MaxProduction)
            })
            .ToList(); */

            decimal? Production = data.Sum(s => s.Production);
            decimal? MaxProduction = data.Sum(s => s.MaxProduction);

            decimal? totalEff = (Production ?? 0) / (MaxProduction == 0 ? 1 : MaxProduction);

            List<EfficiencyByLine> tempList = new();

            foreach (var item in data)
            {
                tempList.Add(new EfficiencyByLine
                {
                    Line = item.Line,
                    Efficiency = item.Production / item.MaxProduction,
                    //Efficiency = _efficiencyFormula.CalculateEfficiency(item.Production, item.MaxProduction)
                });
            }

            EfficiencyByLineFormatted resultByLineFormatted = new()
            {
                Table = tempList,
                TotalEfficiency = totalEff.GetValueOrDefault(0)
            };

            GembaEfficiency resultFormatted = new()
            {
                EfficiencyByLine = resultByLineFormatted
            };

            return resultFormatted;
        }

        private GembaDowntime GetGembaDowntime(GembaFilter filters, List<VwDowntime> downtimeData)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = downtimeData
            .GroupBy(d => d.Category)
            .Select(g => new
            {
                Category = g.Key,
                Hours = g.Sum(d => d.Hours),
                Minutes = g.Sum(d => d.Minutes)
            })
            .ToList();


            if (result.Count == 0)
            {
                GembaDowntime abort = new();
                return abort;
            }

            decimal totalHours = 0;
            decimal totalMinutes = 0;
            List<Downtime> tempList = new();

            for (int i = 0; i < result.Count; i++)
            {

                decimal hour = Math.Round(result[i].Hours.GetValueOrDefault(0));
                decimal minute = Math.Round(result[i].Minutes);

                totalHours += Math.Round(hour, 0);
                totalMinutes += Math.Round(minute, 0);

                Downtime tempDowntime = new()
                {
                    DownTimeCategory = result[i].Category,
                    Hours = hour,
                    Minutes = minute
                };
                tempList.Add(tempDowntime);
            }

            GembaDowntime resultGembaDowntime = new()
            {
                Data = tempList,
                TotalHours = totalHours,
                TotalMinutes = totalMinutes
            };

            return resultGembaDowntime;

        }
        private async Task<DTOUtilization> GetGembaUtilization(GembaFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();

            // Calculate the total number of days in the selected period
            TimeSpan dateDiff = endDate - startDate;
            int days = dateDiff.Days + 1;

            // Get the distinct line IDs from the filtered productivity data
            var lineIdsInData = productivityWithDowntime
                .Select(p => p.LineId)
                .Distinct()
                .ToList();

            // Get the flow values for the lines present in the filtered data
            var totalFlows = await _context.Line
                .Where(l => lineIdsInData.Contains(l.Id)) // Filter by the line IDs from the productivity data
                .SumAsync(l => l.Flow);

            // Group the data by date and line name, and calculate the production hours and excluded category hours
            var groupedData = productivityWithDowntime
                .GroupBy(p => new { p.Date, p.Name }) // Group by date and line name
                .Select(group => new
                {
                    group.Key.Date,
                    group.Key.Name,

                    // Calculate production hours without repeating HourStart
                    ProductionHrs = group
                        .Select(p => p.HourStart)
                        .Distinct()
                        .Count(),

                    // Calculate the excluded category hours, filtered by category and converted to hours
                    ExcludedCategoryHrs = group
                        .Where(p => p.DowntimeCategoryId.HasValue && // Ensure DowntimeCategoryId is not null
                                    filters.UtilizationCategories.Contains(p.DowntimeCategoryId.Value)) // Use the non-nullable value with .Value
                        .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m) // Convert minutes to hours
                })
                .ToList();

            // Calculate the total hours and flows for the filtered lines
            decimal totalProductionHrs = groupedData.Sum(g => g.ProductionHrs);
            decimal totalExcludedCategoryHrs = groupedData.Sum(g => g.ExcludedCategoryHrs);

            // Calculate the total time period in hours (flows * 24 hours per day * number of days)
            int timePeriod = totalFlows * 24 * days;

            // Calculate utilization percentage
            decimal utilizationPercentage = _utilizationFormula.ShortUtilizationFormula(
                totalProductionHrs,
                totalExcludedCategoryHrs,
                timePeriod) * 100;

            // Create the result DTO
            DTOUtilization resultUtilization = new()
            {
                Percentage = Math.Round(utilizationPercentage, 1),
            };

            return resultUtilization;
        }

        private Task<decimal> GetChangeOverMetrics(GembaFilter filters, List<VwDowntime> downtimeData)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            string strChangeOver = _configuration["Tenant:ChangeOverString"] ?? throw new SettingsPropertyNotFoundException();

            var query = downtimeData.Where(w => w.Category == strChangeOver);

            var data = query
                .GroupBy(g => new
                {
                    Date = g.Date,
                    Category = g.Category
                })
                .Select(s => new
                {
                    Date = s.Key.Date,
                    Minutes = s.Sum(su => su.Minutes)
                })
                .ToList();

            decimal minutes = 0;
            foreach (var row in data)
            {
                minutes += row.Minutes;
            }

            return Task.FromResult(Math.Round(minutes / 60, 2));
        }


        public async Task<GembaObj> GetSummaryGemba(GembaFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var downtimeDataSet = await _context.VwDowntime
            .Where(d => d.Date >= startDate && d.Date <= endDate)
            .ToListAsync();

            var summaryEfficiency = await GetGembaEfficiency(filters);
            var summaryCases = await GetGembaCases(filters);
            var summaryDowntime = GetGembaDowntime(filters, downtimeDataSet);
            var summaryUtilization = await GetGembaUtilization(filters);
            decimal changeOverMetrics = await GetChangeOverMetrics(filters, downtimeDataSet);

            ScrapFilter scrapFilter = new()
            {
                EndDate = filters.EndDate,
                StartDate = filters.StartDate,
            };

            var TotalScrap = await _scrapRepository.GetTotalPlantScrap(scrapFilter);

            var scrapTable = await _scrapRepository.GetScrapTableAllLines(scrapFilter);

            /* ChangeOverFilter changeOverFilter = new()
            {
                EndDate = filters.EndDate,
                StartDate = filters.StartDate,
            }; */

            //var changeOverMetrics = await _changeOverRepository.GetChangeOverAllMetrics(changeOverFilter);

            GembaObj resultGemba = new()
            {
                Cases = summaryCases,
                Efficiency = summaryEfficiency,
                Downtime = summaryDowntime,
                Utilization = summaryUtilization,
                TotalScrapPercentage = TotalScrap,
                ScrapTable = scrapTable,
                ChangeOverMetrics = changeOverMetrics
            };

            return resultGemba;
        }
    }
}