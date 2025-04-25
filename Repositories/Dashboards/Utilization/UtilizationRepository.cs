

using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using inventio.Models.DTO.VwUtilization;
using inventio.Utils;
using System.Data;
using inventio.Models.DTO;
using Inventio.Utils.Formulas;


namespace inventio.Repositories.Dashboards.Utilization
{
    public class UtilizationRepository : IUtilizationRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly UtilizationFormula _utilizationFormula;

        public UtilizationRepository(ApplicationDBContext context, UtilizationFormula utilizationFormula)
        {
            _context = context;
            _utilizationFormula = utilizationFormula;
        }


        public async Task<List<DTOReactDropdown<int>>> GetUtilizationLines(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwUtilization
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .Select(s => new DTOReactDropdown<int>
            {
                Value = s.LineId,
                Label = s.Name
            })
            .Distinct()
            .ToListAsync();

            return result;
        }

        public async Task<List<DTOReactDropdown<int>>> GetUtilizationCategories(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines based on the provided names
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id) // Select the ID of the line
                .ToListAsync();

            // Retrieve the IDs of productivity entries that match the dates and line IDs
            var productivityIds = await _context.Productivity
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineID)) // Filter by line IDs
                .Select(p => p.Id) // Select the ID of productivity
                .ToListAsync();

            // Retrieve downtime reasons associated with the filtered productivity IDs
            var downtimeReasons = await _context.DowntimeReason
                .Where(dr => productivityIds.Contains(dr.ProductivityID)) // Filter by productivity IDs
                .ToListAsync();

            // Retrieve downtime categories associated with the downtime reasons
            var result = await _context.DowntimeCategory
                .Where(dc => downtimeReasons.Select(dr => dr.DowntimeCategoryId).Contains(dc.Id)) // Filter by downtime category IDs
                .Select(dc => new DTOReactDropdown<int>
                {
                    Value = dc.Id, // Category ID
                    Label = dc.Name // Category name
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<DTOUtilization> GetPlantUtilization(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the line IDs corresponding to the line names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            // Calculate the total number of days in the selected period
            TimeSpan dateDiff = endDate - startDate;
            int days = dateDiff.Days + 1;

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
                        .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName))
                        .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m) // Convert minutes to hours
                })
                .ToList();

            // Calculate the total hours and flows for all lines
            decimal totalProductionHrs = groupedData.Sum(g => g.ProductionHrs);
            decimal totalExcludedCategoryHrs = groupedData.Sum(g => g.ExcludedCategoryHrs);

            // Retrieve the flow values for the filtered lines
            var totalFlows = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .SumAsync(s => s.Flow);

            // Calculate the total time period in hours (flows * 24 hours per day * number of days)
            int timePeriod = totalFlows * 24 * days;

            // Calculate utilization percentage
            decimal utilizationPercentage = _utilizationFormula.ShortUtilizationFormula(
                totalProductionHrs,
                totalExcludedCategoryHrs,
                timePeriod) * 100;

            DTOUtilization resultUtilization = new()
            {
                Percentage = Math.Round(utilizationPercentage, 1),
            };

            return resultUtilization;
        }

        public async Task<List<DTOUtilizationPerLine>> GetUtilizationPerLine(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines corresponding to the names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            // Group the data by date and line name, and calculate the production hours and excluded category hours
            var groupedData = productivityWithDowntime
                .GroupBy(p => new { p.Name }) // Group by date and line name
                .Select(group => new
                {
                    Line = group.Key.Name,

                    // Calculate production hours without repeating HourStart
                    ProductionHrs = group
                    .GroupBy(p => p.Date) // Group by date
                    .Select(dateGroup => dateGroup
                        .Select(p => p.HourStart)
                        .Distinct()
                        .Count()) // Count distinct HourStart values per date
                    .Sum(), // Sum all distinct counts across dates

                    // Calculate the excluded category hours, filtered by category and converted to hours
                    ExcludedCategoryHrs = group
                        .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName))
                        .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m) // Convert minutes to hours
                })
                .ToList();

            List<DTOUtilizationPerLine> utilizationList = new();

            foreach (var data in groupedData)
            {
                decimal productionHrs = data.ProductionHrs;
                decimal excludedCategoryHrs = data.ExcludedCategoryHrs;

                // Define the time period (e.g., 1 day in hours)
                int days = (endDate - startDate).Days + 1;
                int timePeriod = 24 * days;

                // Calculate the utilization using the formula, including excluded hours
                decimal utilization = _utilizationFormula.ShortUtilizationFormula(
                    productionHrs,
                    excludedCategoryHrs,
                    timePeriod) * 100;

                DTOUtilizationPerLine aux = new()
                {
                    Line = data.Line,
                    Utilization = Math.Round(utilization, 1),
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }
        public async Task<List<DTOUtilizationTable>> GetUtilizationTable(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<string> flows = new List<string>();

            foreach (var row in filters.Lines)
            {
                flows.Add(row);
                flows.Add(row + " (2)");
            }

            TimeSpan dateDiff = endDate - startDate;
            int days = dateDiff.Days + 1;

            var intermediateResult = await _context.VwProductivityWithDowntime
                .Where(w => w.Date >= startDate && w.Date <= endDate && flows.Contains(w.Name))
                .ToListAsync();

            var result = intermediateResult
                .GroupBy(g => g.Name)
                .Select(s => new
                {
                    Line = s.Key,

                    // Calculate the total production hours by summing up unique HourStart counts for each day
                    ProductionHrs = s
                    .GroupBy(p => p.Date) // Group by date
                    .Select(dateGroup => dateGroup
                        .Select(p => p.HourStart)
                        .Distinct()
                        .Count()) // Count distinct HourStart values per date
                    .Sum(),

                    ExcludedCategoryHrs = s
                        .Where(u => !string.IsNullOrEmpty(u.DowntimeCategoryName) && filters.Categories.Contains(u.DowntimeCategoryName))
                        .Sum(u => u.Minutes.GetValueOrDefault(0m) / 60m),
                })
                .ToList();

            List<DTOUtilizationTable> utilizationList = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal? productionHrs = result[i].ProductionHrs;
                decimal? excludedHrs = result[i].ExcludedCategoryHrs;
                int timePeriod = 1 * 24 * days;

                decimal? utilization = _utilizationFormula.ShortUtilizationFormula(
                    productionHrs.GetValueOrDefault(1),
                    excludedHrs.GetValueOrDefault(0),
                    timePeriod) * 100;

                DTOUtilizationTable aux = new()
                {
                    Line = result[i].Line,
                    Utilization = Math.Round(utilization ?? 0, 1),
                    ChangeHrs = Math.Round(result[i].ExcludedCategoryHrs, 2),
                    NetHrs = result[i].ProductionHrs - Math.Round(result[i].ExcludedCategoryHrs, 2),
                    TotalHrs = result[i].ProductionHrs
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }

        public async Task<List<DTOUtilizationPerTime>> GetUtilizationPerDay(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines corresponding to the names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            // Group the data by date and line name, and calculate the production hours and excluded category hours (minutes)
            var groupedData = productivityWithDowntime
            .GroupBy(p => new { p.Name, p.Date, })
            .Select(group => new
            {
                group.Key.Date,
                group.Key.Name,

                // Cuenta los registros, pero solo los distintos por HourStart
                ProductionHrs = group
                    .Select(p => p.HourStart)
                    .Distinct()
                    .Count(),

                // ExcludedCategoryHrs suma los minutos, filtrando por categoría y convirtiéndolos en horas
                ExcludedCategoryHrs = group
                    .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName)) // Filtro por categoría
                    .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m)
            })
            .OrderBy(o => o.Date)
            .ThenBy(o => o.Name)
            .ToList();


            List<DTOUtilizationPerTime> utilizationList = new();

            foreach (var data in groupedData)
            {
                // Define the time period (e.g., 1 day in hours)
                int timePeriod = 1 * 24 * 1;

                // Calculate the utilization using the formula
                decimal? utilization = _utilizationFormula.ShortUtilizationFormula(
                    data.ProductionHrs,
                    data.ExcludedCategoryHrs,
                    timePeriod) * 100;

                DTOUtilizationPerTime aux = new()
                {
                    Time = data.Date.ToString("MM/dd/yy"),
                    Line = data.Name.ToString(),
                    Utilization = Math.Round(utilization ?? 0, 1)
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }
        public async Task<List<DTOUtilizationPerTime>> GetUtilizationPerWeek(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines corresponding to the names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            Charts charts = new();

            // Group the data by line, week, and year, and calculate production hours and excluded category hours
            var groupedData = productivityWithDowntime
                .GroupBy(p => new
                {
                    Line = p.Name,
                    Week = charts.GetIso8601WeekOfYear(p.Date), // Get week of the year for each date
                    p.Date.Year
                })
                .Select(group => new
                {
                    group.Key.Week,
                    group.Key.Year,
                    group.Key.Line,

                    // Calculate production hours without repeating HourStart
                    ProductionHrs = group
                    .GroupBy(p => p.Date.Date)
                    .Select(dayGroup => dayGroup
                        .Select(p => p.HourStart)
                        .Distinct()
                        .Count())
                    .Sum(),


                    // Calculate the excluded category hours, filtering by downtime category and converting to hours
                    ExcludedCategoryHrs = group
                    .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName)) // Filtro por categoría
                    .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m)

                })
                .OrderBy(o => o.Year)
                .ThenBy(o => o.Week)
                .ToList();

            List<DTOUtilizationPerTime> utilizationList = new();

            foreach (var data in groupedData)
            {
                // Define the time period (1 week in hours)
                int timePeriod = 1 * 24 * 7;

                // Calculate utilization using the formula
                decimal? utilization = _utilizationFormula.ShortUtilizationFormula(
                    data.ProductionHrs,
                    data.ExcludedCategoryHrs,
                    timePeriod) * 100;

                DTOUtilizationPerTime aux = new()
                {
                    Time = data.Week.ToString() + "/" + data.Year.ToString(),
                    Line = data.Line,
                    Utilization = Math.Round(utilization ?? 0, 1)
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }

        public async Task<List<DTOUtilizationPerTime>> GetUtilizationPerMonth(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines corresponding to the names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            Charts charts = new();

            // Group the data by line, month, and year, and calculate production hours and excluded category hours
            var groupedData = productivityWithDowntime
                .GroupBy(p => new
                {
                    Line = p.Name,
                    Month = p.Date.Month,
                    p.Date.Year
                })
                .Select(group => new
                {
                    group.Key.Month,
                    group.Key.Year,
                    group.Key.Line,

                    // Calculate production hours without repeating HourStart per day, then sum for the month
                    ProductionHrs = group
                        .GroupBy(p => p.Date.Date) // Group by each day
                        .Select(dayGroup => dayGroup
                            .Select(p => p.HourStart) // Count production hours per day without duplicates
                            .Distinct()
                            .Count())
                        .Sum(), // Sum the hours for each day to get the total for the month

                    // Calculate the excluded category hours, filtering by downtime category and converting to hours
                    ExcludedCategoryHrs = group
                        .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName)) // Filter by category
                        .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m) // Convert minutes to hours
                })
                .OrderBy(o => o.Year)
                .ThenBy(o => o.Month)
                .ToList();

            List<DTOUtilizationPerTime> utilizationList = new();

            foreach (var data in groupedData)
            {
                // Calculate the total time period for the month (days in the month * 24 hours)
                int timePeriod = 24 * DateTime.DaysInMonth(data.Year, data.Month);

                // Calculate utilization using the formula
                decimal? utilization = _utilizationFormula.ShortUtilizationFormula(
                    data.ProductionHrs,
                    data.ExcludedCategoryHrs,
                    timePeriod) * 100;

                DTOUtilizationPerTime aux = new()
                {
                    Time = $"{data.Year}/{data.Month}",
                    Line = data.Line,
                    Utilization = Math.Round(utilization ?? 0, 1)
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }

        public async Task<List<DTOUtilizationPerTime>> GetUtilizationPerQuarter(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines corresponding to the names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            Charts charts = new();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            // Group the data by line, year, and quarter, and calculate production hours and excluded category hours
            var groupedData = productivityWithDowntime
                .GroupBy(p => new
                {
                    Line = p.Name,
                    Quarter = (p.Date.Month - 1) / 3 + 1,
                    p.Date.Year
                })
                .Select(group => new
                {
                    group.Key.Quarter,
                    group.Key.Year,
                    group.Key.Line,

                    // Calculate production hours without repeating HourStart per day, then sum for the quarter
                    ProductionHrs = group
                        .GroupBy(p => p.Date.Date)
                        .Select(dayGroup => dayGroup
                            .Select(p => p.HourStart)
                            .Distinct()
                            .Count())
                        .Sum(),

                    // Calculate the excluded category hours, filtering by downtime category and converting to hours
                    ExcludedCategoryHrs = group
                        .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName))
                        .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m)
                })
                .OrderBy(o => o.Year)
                .ThenBy(o => o.Quarter)
                .ToList();

            List<DTOUtilizationPerTime> utilizationList = new();

            foreach (var data in groupedData)
            {
                // Obtener el rango de fechas para el trimestre
                (DateTime quarterStart, DateTime quarterEnd) = GetQuarterDateRange(data.Year, data.Quarter);

                // Calcular el número real de días en el trimestre
                int totalDays = (int)(quarterEnd - quarterStart).TotalDays + 1;
                int timePeriod = totalDays * 24;

                // Calcular la utilización usando la fórmula
                decimal? utilization = _utilizationFormula.ShortUtilizationFormula(
                    data.ProductionHrs,
                    data.ExcludedCategoryHrs,
                    timePeriod) * 100;

                DTOUtilizationPerTime aux = new()
                {
                    Time = $"Q{data.Quarter}/{data.Year}",
                    Line = data.Line,
                    Utilization = Math.Round(utilization ?? 0, 1)
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }

        private (DateTime Start, DateTime End) GetQuarterDateRange(int year, int quarter)
        {
            return quarter switch
            {
                1 => (new DateTime(year, 1, 1), new DateTime(year, 3, 31)),
                2 => (new DateTime(year, 4, 1), new DateTime(year, 6, 30)),
                3 => (new DateTime(year, 7, 1), new DateTime(year, 9, 30)),
                4 => (new DateTime(year, 10, 1), new DateTime(year, 12, 31)),
                _ => throw new ArgumentException("Quarter must be between 1 and 4.")
            };
        }

        public async Task<List<DTOUtilizationPerTime>> GetUtilizationPerYear(UtilizationFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            // Retrieve the IDs of the lines corresponding to the names in the filters
            var lineIds = await _context.Line
                .Where(w => filters.Lines.Contains(w.Name))
                .Select(l => l.Id)
                .ToListAsync();

            // Retrieve the productivity entries matching the dates and line IDs
            var productivityWithDowntime = await _context.VwProductivityWithDowntime
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => lineIds.Contains(p.LineId))
                .ToListAsync();

            // Group the data by line and year, and calculate production hours and excluded category hours
            var groupedData = productivityWithDowntime
                .GroupBy(p => new
                {
                    Line = p.Name,
                    p.Date.Year
                })
                .Select(group => new
                {
                    group.Key.Year,
                    group.Key.Line,

                    // Calculate production hours without repeating HourStart per day, then sum for the year
                    ProductionHrs = group
                        .GroupBy(p => p.Date.Date) // Group by each day
                        .Select(dayGroup => dayGroup
                            .Select(p => p.HourStart) // Count production hours per day without duplicates
                            .Distinct()
                            .Count())
                        .Sum(), // Sum the hours for each day to get the total for the year

                    // Calculate the excluded category hours, filtering by downtime category and converting to hours
                    ExcludedCategoryHrs = group
                        .Where(p => !string.IsNullOrEmpty(p.DowntimeCategoryName) && filters.Categories.Contains(p.DowntimeCategoryName)) // Filter by category
                        .Sum(p => p.Minutes.GetValueOrDefault(0m) / 60m) // Convert minutes to hours
                })
                .OrderBy(o => o.Year)
                .ToList();

            static int GetDaysInYear(int year)
            {
                return DateTime.IsLeapYear(year) ? 366 : 365;
            }

            List<DTOUtilizationPerTime> utilizationList = new();

            foreach (var data in groupedData)
            {
                // Calculate the total time period for the year (days in the year * 24 hours)
                int timePeriod = 24 * GetDaysInYear(data.Year);

                // Calculate utilization using the formula
                decimal? utilization = _utilizationFormula.ShortUtilizationFormula(
                    data.ProductionHrs,
                    data.ExcludedCategoryHrs,
                    timePeriod) * 100;

                DTOUtilizationPerTime aux = new()
                {
                    Time = data.Year.ToString(),
                    Line = data.Line,
                    Utilization = Math.Round(utilization ?? 0, 1)
                };

                utilizationList.Add(aux);
            }

            return utilizationList;
        }

    }
}