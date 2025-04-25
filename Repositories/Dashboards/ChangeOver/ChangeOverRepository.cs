using inventio.Models.DTO;
using inventio.Models.DTO.ChangeOver;
using Inventio.Data;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.ChangeOver
{
    public class ChangeOverRepository : IChangeOverRepository
    {
        private readonly ApplicationDBContext _context;
        public ChangeOverRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetChangeOverLines(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ChangeOver
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .Select(w => w.Line)
            .Distinct()
            .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<string>> GetChangeOverShifts(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ChangeOver
            .Where(w => filters.Lines!.Contains(w.Line)
             && w.Date >= startDate && w.Date <= endDate)
            .Select(w => w.Shift)
            .Distinct()
            .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DTOReactSelect>> GetChangeOverSupervisors(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ChangeOver
                .Where(w => filters.Lines!.Contains(w.Line)
                    && filters.Shifts!.Contains(w.Shift)
                    && w.Date >= startDate && w.Date <= endDate)
                .Join(
                    _context.Supervisor,
                    changeOver => changeOver.Supervisor,
                    supervisor => supervisor.Description,
                    (changeOver, supervisor) => new { Supervisor = supervisor }
                )
                .Select(s => new DTOReactSelect
                {
                    Label = s.Supervisor.Name,
                    Value = s.Supervisor.Description
                })
                .Distinct()
                .ToListAsync();


            return result;
        }

        public async Task<IEnumerable<string>> GetChangeOverSubCategory2(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ChangeOver
            .Where(w => filters.Lines!.Contains(w.Line)
                && filters.Shifts!.Contains(w.Shift)
                && filters.Supervisors!.Contains(w.Supervisor)
                && w.Date >= startDate && w.Date <= endDate)
            .Select(w => w.Subcategory2)
            .Distinct()
            .ToListAsync();

            return result;
        }

        public async Task<DTOChangeOverMetrics> GetChangeOverAllMetrics(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var minutes = await _context.ChangeOver
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .SumAsync(s => s.Minutes);

            var quantity = await _context.ChangeOver
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .CountAsync();

            DTOChangeOverMetrics result = new()
            {
                TotalHours = Math.Round(minutes / 60, 0),
                TotalMinutes = Math.Round(minutes, 0),
                TotalQuantity = quantity
            };

            return result;
        }

        public async Task<DTOChangeOverMetrics> GetChangeOverMetrics(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var minutes = await _context.ChangeOver
            .Where(w => filters.Lines!.Contains(w.Line)
                && filters.Shifts!.Contains(w.Shift)
                && filters.Supervisors!.Contains(w.Supervisor)
                && filters.SubCategory2!.Contains(w.Subcategory2)
                && w.Date >= startDate && w.Date <= endDate)
            .SumAsync(s => s.Minutes);

            var quantity = await _context.ChangeOver
            .Where(w => filters.Lines!.Contains(w.Line)
                && filters.Shifts!.Contains(w.Shift)
                && filters.Supervisors!.Contains(w.Supervisor)
                && filters.SubCategory2!.Contains(w.Subcategory2)
                && w.Date >= startDate && w.Date <= endDate)
            .CountAsync();

            DTOChangeOverMetrics result = new()
            {
                TotalHours = Math.Round(minutes / 60, 0),
                TotalMinutes = Math.Round(minutes, 0),
                TotalQuantity = quantity
            };

            return result;
        }

        public async Task<List<DTOChangeOverTable>> GetChangeOverTable(ChangeOverFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ChangeOver
            .Where(w => filters.Lines!.Contains(w.Line)
                && filters.Shifts!.Contains(w.Shift)
                && filters.Supervisors!.Contains(w.Supervisor)
                && filters.SubCategory2!.Contains(w.Subcategory2)
                && w.Date >= startDate && w.Date <= endDate)
                .GroupBy(g => new { g.Subcategory2, g.Line })
                .Select(group => new DTOChangeOverTable
                {
                    SubCategory2 = group.Key.Subcategory2,
                    Line = group.Key.Line,
                    Minutes = group.Sum(s => s.Minutes),
                    Quantity = group.Count()
                })
                .OrderBy(o => o.SubCategory2)
                .ThenBy(t => t.Line)
                .ToListAsync();

            return result;
        }

        public async Task<DTOChangeOverDashboard> GetScrapDashboard(ChangeOverFilter filters)
        {
            DTOChangeOverMetrics changeOverMetrics = await GetChangeOverMetrics(filters);
            List<DTOChangeOverTable> changeOverTables = await GetChangeOverTable(filters);

            DTOChangeOverDashboard changeOverDashboard = new()
            {
                ChangeOverMetrics = changeOverMetrics,
                ChangeOverTable = changeOverTables
            };

            return changeOverDashboard;
        }
    }
}