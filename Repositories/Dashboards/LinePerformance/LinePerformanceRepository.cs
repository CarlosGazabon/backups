using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeXSubcat;
using inventio.Models.DTO.Efficiency;
using inventio.Models.DTO.LinePerformance;
using inventio.Models.DTO.ProductivitySummary;
using inventio.Models.DTO.VwUtilization;
using Inventio.Data;
using Inventio.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.LinePerformance
{
    public class LinePerformanceRepository : ILinePerformanceRepository
    {
        private readonly ApplicationDBContext _context;

        public LinePerformanceRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        private DTOProductionSet GetProductionPerSku(List<ProductivitySummary> baseList)
        {

            var result = baseList
                .GroupBy(g => new
                {
                    g.SKU,
                })
                .Select(g => new DTOProductionPerSku
                {
                    Sku = g.Key.SKU,
                    Production = g.Sum(s => s.Production)
                })
            .ToList();

            decimal totalProduction = result.Sum(s => s.Production);

            DTOProductionSet productionSet = new()
            {
                ProductionPerSku = result,
                TotalProduction = totalProduction
            };

            return productionSet;
        }

        private DTOEffSet GetShiftEff(List<VwGeneralEfficiency> baseList)
        {
            var result = baseList
            .GroupBy(g => new
            {
                g.Shift,
            })
            .Select(g => new
            {
                g.Key.Shift,
                Production = g.Sum(s => s.Production),
                EffDen = g.Sum(s => s.EffDen),
                Efficiency = g.Sum(s => s.Production) / g.Sum(s => s.EffDen),
            })
            .ToList();

            List<DTOEfficiencyPerShift> shiftEffList = new();
            decimal totalProduction = 0;
            decimal totalEffDen = 0;

            for (int i = 0; i < result.Count; i++)
            {
                totalProduction += result[i].Production.GetValueOrDefault(0);
                totalEffDen += result[i].EffDen.GetValueOrDefault(0);

                decimal auxEff = Math.Round(result[i].Efficiency.GetValueOrDefault(0) * 100, 1);
                DTOEfficiencyPerShift aux = new()
                {
                    Shift = result[i].Shift,
                    Efficiency = auxEff
                };

                shiftEffList.Add(aux);
            }

            decimal totalEfficiency = totalProduction / (totalEffDen == 0 ? 1 : totalEffDen) * 100;

            shiftEffList = shiftEffList.OrderBy(item => item.Shift).ToList();
            DTOEffSet effSet = new()
            {
                EfficiencyPerShift = shiftEffList,
                TotalEfficiency = Math.Round(totalEfficiency, 2)
            };

            return effSet;
        }

        private DTODowntimeSet GetDowntimeMinutesXSubCategory(List<VwDowntimeXSubCat> baseList)
        {

            var downtimePerSubcat = baseList
              .GroupBy(g => new { g.SubCategory2Id, g.SubCategory2 })
              .Select(s => new DTOMinutesXSubcategory
              {
                  SubCategory = s.Key.SubCategory2,
                  Minutes = s.Sum(obj => obj.Minutes),
                  Hours = s.Sum(obj => obj.Hours)
              })
              .ToList();

            decimal? totalDowntime = downtimePerSubcat.Sum(s => s.Hours);

            DTODowntimeSet downtimeSet = new()
            {
                MinutesXSubcategories = downtimePerSubcat,
                TotalDowntime = Math.Round(totalDowntime.GetValueOrDefault(0), 2)
            };

            return downtimeSet;
        }


        private List<DTOMinutesXCode> GetDowntimeMinutesXCode(List<VwDowntimeXSubCat> baseList)
        {

            var result = baseList
              .GroupBy(g => new
              {
                  g.SubCategory2Id,
                  g.SubCategory2,
                  g.Code,
                  g.Failure
              })
              .Select(s => new DTOMinutesXCode
              {
                  Code = s.Key.Code,
                  SubCategory = s.Key.SubCategory2,
                  Failure = s.Key.Failure,
                  Minutes = s.Sum(obj => obj.Minutes)
              })
              .ToList();


            return result;
        }

        private DTODowntimeContainer GetDowntime(List<VwDowntimeXSubCat> baseList)
        {
            DTODowntimeSet downtimeSet = GetDowntimeMinutesXSubCategory(baseList);
            List<DTOMinutesXCode> minutesXCodes = GetDowntimeMinutesXCode(baseList);

            DTODowntimePerMinutes downtimePerMinutes = new()
            {
                MinutesXCode = minutesXCodes,
                MinutesXSubCategory = downtimeSet.MinutesXSubcategories
            };

            DTODowntimeContainer downtimeContainer = new()
            {
                DowntimePerMinutes = downtimePerMinutes,
                TotalDowntimeHours = downtimeSet.TotalDowntime,
            };

            return downtimeContainer;
        }

        private DTOScrapSet GetScrapPerLine(List<ProductivitySummary> baseList)
        {

            var result = baseList
                .GroupBy(ps => ps.Line)
                .Select(group => new
                {
                    Line = group.Key,
                    PreformScrap = group.Sum(ps => ps.PreformScrap),
                    BottleScrap = group.Sum(ps => ps.BottleScrap),
                    CanScrap = group.Sum(ps => ps.CanScrap),
                    PouchScrap = group.Sum(ps => ps.PouchScrap),
                    ScrapUnits = group.Sum(ps => ps.ScrapUnits),
                    ScrapDen = group.Sum(ps => ps.ScrapDEN) == 0 ? 1 : group.Sum(ps => ps.ScrapDEN)
                })
                .ToList();

            int totalScrapUnits = 0;
            decimal totalScrapDen = 0;
            List<DTOScrapPerLine> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {

                totalScrapUnits += result[i].ScrapUnits;
                totalScrapDen += result[i].ScrapDen;

                decimal preformScrapDen = result[i].PreformScrap + result[i].ScrapDen;
                decimal preformScrapPercent = result[i].PreformScrap / (preformScrapDen == 0 ? 1 : preformScrapDen);

                decimal bottleScrapDen = result[i].BottleScrap + result[i].ScrapDen;
                decimal bottleScrapPercent = bottleScrapDen == 0 ? 1 : result[i].BottleScrap / (bottleScrapDen == 0 ? 1 : bottleScrapDen);

                decimal canScrapDen = result[i].CanScrap + result[i].ScrapDen;
                decimal canScrapPercent = canScrapDen == 0 ? 1 : result[i].CanScrap / (canScrapDen == 0 ? 1 : canScrapDen);

                decimal pouchScrapDen = result[i].PouchScrap + result[i].ScrapDen;
                decimal pouchScrapPercent = pouchScrapDen == 0 ? 1 : result[i].PouchScrap / (pouchScrapDen == 0 ? 1 : pouchScrapDen);

                DTOScrapPerLine auxPreform = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Preform",
                    ScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                };

                DTOScrapPerLine auxBottle = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Bottle",
                    ScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                };

                DTOScrapPerLine auxCan = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Can",
                    ScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                };

                DTOScrapPerLine auxPouch = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Pouch",
                    ScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                };

                scrapList.Add(auxPreform);
                scrapList.Add(auxBottle);
                scrapList.Add(auxCan);
                scrapList.Add(auxPouch);
            }

            decimal denominator = (totalScrapUnits + totalScrapDen) != 0 ? (totalScrapUnits + totalScrapDen) : 1;
            decimal totalScrapPercent = totalScrapUnits / denominator;

            DTOScrapSet scrapSet = new()
            {
                ScrapPerLine = scrapList,
                TotalScrapPercent = Math.Round(totalScrapPercent * 100, 2)
            };

            return scrapSet;
        }

        public async Task<DTOUtilization> GetPlantUtilization(LinePerformanceFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<string> flowsLine = new()
            {
                filters.Line,
                filters.Line + " (2)"
            };


            TimeSpan dateDiff = endDate - startDate;

            int days = dateDiff.Days + 1;

            var hours = await _context.VwUtilization
                .Where(w => w.Date >= startDate && w.Date <= endDate && flowsLine.Contains(w.Name))
                .SumAsync(s => s.NetHrs);

            var flows = await _context.Line
                .Where(w => w.Name == filters.Line)
                .SumAsync(s => s.Flow);

            decimal utilizationPercentage = hours.GetValueOrDefault(1) / (flows * 24 * days) * 100;

            DTOUtilization resultUtilization = new()
            {
                Percentage = Math.Round(utilizationPercentage, 2),
            };

            return resultUtilization;
        }


        public async Task<DTOLinePerformanceDashboard> LinePerformanceDashboard(LinePerformanceFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            /* -------------------------- Productivity DataSet -------------------------- */
            // Get Production by Sku and Scrap per Line
            List<ProductivitySummary> productivityBaseList = await _context.ProductivitySummary
               .Where(w => w.Date >= startDate && w.Date <= endDate
               && w.Line == filters.Line)
           .ToListAsync();

            // Get Eff per Line
            List<VwGeneralEfficiency> effDataSet = await _context.VwGeneralEfficiency
                .Where(w => w.Date >= startDate && w.Date <= endDate
                && w.Line == filters.Line)
           .ToListAsync();

            // Get Downtime Per Subcategory 
            List<VwDowntimeXSubCat> downtimeBaseList = await _context.VwDowntimeXSubCat
                .Where(w => w.Date >= startDate && w.Date <= endDate
                && w.Line == filters.Line)
            .ToListAsync();

            // Get Total Change Over Per Line
            var totalChangeOver = await _context.ChangeOver
            .Where(w => w.Line == filters.Line
                && w.Date >= startDate && w.Date <= endDate)
            .SumAsync(s => s.Minutes);


            decimal totalChangeOverHrs = totalChangeOver / 60;

            // Get Data needed fot the dashboard 

            DTOProductionSet productionSet = GetProductionPerSku(productivityBaseList);

            DTOEffSet effSet = GetShiftEff(effDataSet);

            DTODowntimeContainer downtime = GetDowntime(downtimeBaseList);

            DTOScrapSet scrapSet = GetScrapPerLine(productivityBaseList);

            DTOUtilization utilization = await GetPlantUtilization(filters);

            DTOSummary summary = new()
            {
                Production = productionSet.TotalProduction,
                Efficiency = effSet.TotalEfficiency,
                Downtime = downtime.TotalDowntimeHours,
                Scrap = scrapSet.TotalScrapPercent,
                Utilization = utilization.Percentage,
                ChangeOver = Math.Round(totalChangeOverHrs, 2)
            };

            DTOLinePerformanceDashboard linePerformanceDashboard = new()
            {
                Summary = summary,
                ProductionPerSku = productionSet.ProductionPerSku,
                EfficiencyPerShift = effSet.EfficiencyPerShift,
                DowntimePerMinutes = downtime.DowntimePerMinutes,
                ScrapPerLine = scrapSet.ScrapPerLine
            };

            return linePerformanceDashboard;
        }

        public async Task<List<DTOReactDropdown<string>>> GetLines(LinePerformanceFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<DTOReactDropdown<string>> result = new List<DTOReactDropdown<string>>();
            try
            {
                result = await _context.ProductivitySummary
                    .Where(w => w.Date >= startDate && w.Date <= endDate)
                    .Select(s => new DTOReactDropdown<string>
                    {
                        Label = s.Line,
                        Value = s.Line
                    })
                    .Distinct()
                    .OrderBy(o => o.Value)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetLines: {ex.Message}");
                throw;
            }

            return result;
        }

        // objectives lines
        public async Task<DTOLineObjectives?> GetLineObjectives(LinePerformanceFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.Line
                .Where(w => w.Name == filters.Line)
                .Select(s => new
                {
                    Utilization = s.Utilization,
                    Efficiency = s.Efficiency,
                    Scrap = s.Scrap
                })
                .FirstOrDefaultAsync();

            if (result != null)
            {
                DTOLineObjectives objectives = new()
                {
                    Efficiency = result.Efficiency * 100,
                    Scrap = result.Scrap * 100,
                    Utilization = result.Utilization * 100
                };

                return objectives;
            }


            return null;
        }
    }
}