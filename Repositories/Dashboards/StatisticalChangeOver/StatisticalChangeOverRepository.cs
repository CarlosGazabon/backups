using inventio.Models.DTO;
using inventio.Models.DTO.StatisticalChangeOver;
using Inventio.Data;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.StatisticalChangeOver
{
    public class StatisticalChangeOverRepository : IStatisticalChangeOverRepository
    {

        private readonly ApplicationDBContext _context;
        public StatisticalChangeOverRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<DTOReactDropdown<string>>> GetLines(DTOStatisticalChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<DTOReactDropdown<string>> result = new List<DTOReactDropdown<string>>();
            try
            {
                result = await _context.VwStatisticalChangeOver
                    .Where(w => w.Date >= startDate && w.Date <= endDate)
                    .Select(s => new DTOReactDropdown<string>
                    {
                        Label = s.Line,
                        Value = s.Line
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetLines: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<List<DTOReactDropdown<string>>> GetShifts(DTOStatisticalChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<DTOReactDropdown<string>> result = new List<DTOReactDropdown<string>>();
            try
            {
                result = await _context.VwStatisticalChangeOver
                    .Where(w => filters.Lines!.Contains(w.Line)
                        && w.Date >= startDate && w.Date <= endDate)
                    .Select(s => new DTOReactDropdown<string>
                    {
                        Label = s.Shift,
                        Value = s.Shift
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetShift: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<List<DTOReactDropdown<string>>> GetSupervisors(DTOStatisticalChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwStatisticalChangeOver
                .Where(w => filters.Lines!.Contains(w.Line)
                    && filters.Shifts!.Contains(w.Shift)
                    && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<string>
                {
                    Label = s.SupervisorName,
                    Value = s.Supervisor
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<List<DTOReactDropdown<string>>> GetSubCategories(DTOStatisticalChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);


            List<DTOReactDropdown<string>> result = new List<DTOReactDropdown<string>>();
            try
            {
                result = await _context.VwStatisticalChangeOver
                    .Where(w => filters.Lines!.Contains(w.Line)
                        && filters.Shifts!.Contains(w.Shift)
                        && filters.Supervisors!.Contains(w.Supervisor)
                        && w.Date >= startDate && w.Date <= endDate)
                    .Select(s => new DTOReactDropdown<string>
                    {
                        Label = s.SubCategory2,
                        Value = s.SubCategory2
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetSubCategories: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<List<DTOReactDropdown<string>>> GetCodes(DTOStatisticalChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<DTOReactDropdown<string>> result = new List<DTOReactDropdown<string>>();
            try
            {
                result = await _context.VwStatisticalChangeOver
                    .Where(w => filters.Lines!.Contains(w.Line)
                        && filters.Shifts!.Contains(w.Shift)
                        && filters.Supervisors!.Contains(w.Supervisor)
                        && filters.SubCategories!.Contains(w.SubCategory2)
                        && w.Date >= startDate && w.Date <= endDate)
                    .Select(s => new DTOReactDropdown<string>
                    {
                        Label = s.Code,
                        Value = s.Code
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetCodes: {ex.Message}");
                throw;
            }

            return result;
        }


        // Function to calculate the population standard deviation
        private static decimal CalculatePopulationStdDev(IEnumerable<decimal> values)
        {
            double mean = (double)values.Average();
            double sumOfSquares = values.Sum(value => Math.Pow((double)value - mean, 2));
            double populationStdDev = Math.Sqrt(sumOfSquares / values.Count());
            return (decimal)populationStdDev;
        }

        private List<DTOTableChangeOver> GetTableChangeOver(DTOStatisticalChangeOverFilter filters, List<Inventio.Models.ChangeOver> data)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<DTOTableChangeOver> result = new();
            try
            {
                result = data
                    .Join(
                        _context.DowntimeCode,
                        co => co.Code,
                        dc => dc.Code,
                        (co, dc) => new { co, dc }
                    )
                    .GroupBy(joined => new { joined.co.Code, joined.dc.Failure, joined.dc.ObjectiveMinutes })
                    .Select(group => new DTOTableChangeOver
                    {
                        Code = group.Key.Code,
                        Failure = group.Key.Failure,
                        Count = group.Count(),
                        Objective = group.Key.ObjectiveMinutes,
                        Min = group.Min(min => min.co.Minutes),
                        Max = group.Max(max => max.co.Minutes),
                        Avg = group.Average(avg => avg.co.Minutes),
                        STDPopulation = CalculatePopulationStdDev(group.Select(co => co.co.Minutes)),
                        Total = group.Sum(s => s.co.Minutes)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetTableChangeOver: {ex.Message}");
                throw;
            }

            return result;
        }


        private List<DTOHistogram> GetHistogram(DTOStatisticalChangeOverFilter filters, List<Inventio.Models.ChangeOver> data)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<DTOHistogram> histogram = new();
            try
            {
                histogram = data
                    .Select(g => new DTOHistogram
                    {
                        Id = g.Id,
                        Minutes = g.Minutes
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetHistogram: {ex.Message}");
                throw;
            }

            return histogram;
        }


        public async Task<DTOStatisticalChangeOverDashboard> GetStatisticalChangeOverDashboard(DTOStatisticalChangeOverFilter filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<Inventio.Models.ChangeOver> dataSet = await _context.ChangeOver
                                .Where(w => filters.Lines!.Contains(w.Line)
                                    && filters.Shifts!.Contains(w.Shift)
                                    && filters.Supervisors!.Contains(w.Supervisor)
                                    && filters.SubCategories!.Contains(w.Subcategory2)
                                    && filters.Codes!.Contains(w.Code)
                                    && w.Date >= startDate && w.Date <= endDate).ToListAsync();

            List<DTOTableChangeOver> tableChangeOver = GetTableChangeOver(filters, dataSet);
            List<DTOHistogram> histogram = GetHistogram(filters, dataSet);

            DTOStatisticalChangeOverDashboard dashboard = new()
            {
                TableChangeOver = tableChangeOver,
                Histogram = histogram
            };

            return dashboard;
        }
    }
}