using inventio.Models.DTO.DowntimeSku;
using Inventio.Data;
using iText.Forms.Xfdf;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.DowntimeSku
{
    public class DowntimeSkuRepository : IDowntimeSkuRepository
    {
        private readonly ApplicationDBContext _context;

        public DowntimeSkuRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        //* --------------------------------- Filters -------------------------------- */
        public async Task<List<DTODowntimeSkuGetFilters>> GetFilters(DTODowntimeSkuDatesFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var filter = await _context.VwDowntimePerSku
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .GroupBy(g => new
            {
                g.Line,
                g.LineID,
                g.Category,
                g.CategoryID,
                g.Shift,
                g.ShiftID,
                g.Packing,
                g.SKU,
                g.SKUID
            })
            .Select(s => new DTODowntimeSkuGetFilters
            {
                Line = s.Key.Line,
                LineID = s.Key.LineID,
                Category = s.Key.Category,
                CategoryID = s.Key.CategoryID,
                Shift = s.Key.Shift,
                ShiftID = s.Key.ShiftID,
                Packing = s.Key.Packing,
                SKU = s.Key.SKU,
                SkuID = s.Key.SKUID
            })
            .Distinct()
            .ToListAsync();

            return filter;
        }

        //* ----------------------------- Method Privates ---------------------------- */
        public DTODowntimeSkuTotal GetTotalDowntime(List<Models.Views.VwDowntimePerSku> data, DowntimeSkuFilter filters)
        {
            var minutes = data
                .Where(w => filters.Shifts.Contains(w.ShiftID))
                .Sum(s => s.Minutes);

            var extraMinutes = data
                .Where(w => filters.Shifts.Contains(w.ShiftID))
                .Sum(s => s.ExtraMinutes);

            DTODowntimeSkuTotal totalDowntime = new()
            {
                Minutes = minutes + extraMinutes,
                Hours = Math.Round((minutes + extraMinutes) / 60, 2)
            };

            return totalDowntime;
        }

        public List<DTODowntimeSkuTable> GetDowntimePerSkuTable(List<Models.Views.VwDowntimePerSku> data, DowntimeSkuFilter filters)
        {
            var downtimePerSkuTable = data
                .Where(w => filters.Lines.Contains(w.LineID)
                        && filters.Categories.Contains(w.CategoryID)
                        && filters.Shifts.Contains(w.ShiftID)
                        && filters.Packing.Contains(w.Packing)
                        && filters.Sku.Contains(w.SKUID))
                .GroupBy(g => new
                {
                    g.Line,
                    g.SKU
                })
                .Select(s => new DTODowntimeSkuTable
                {
                    Minutes = s.Sum(s => s.Minutes) + s.Sum(s => s.ExtraMinutes),
                    Sku = s.Key.SKU,
                    Line = s.Key.Line
                })
                .ToList();

            return downtimePerSkuTable;
        }

        public List<DTODowntimeSkuPerMinutes> GetDowntimePerSkuMinutes(List<Models.Views.VwDowntimePerSku> data, DowntimeSkuFilter filters)
        {
            var downtimeSkuMinutes = data
                .Where(w => filters.Lines.Contains(w.LineID)
                        && filters.Categories.Contains(w.CategoryID)
                        && filters.Shifts.Contains(w.ShiftID)
                        && filters.Packing.Contains(w.Packing)
                        && filters.Sku.Contains(w.SKUID))
                .GroupBy(g => new
                {
                    g.Category,
                    g.SKU,
                    g.Flavor,
                    g.NetContent,
                    g.Packing
                })
                .Select(s => new DTODowntimeSkuPerMinutes
                {
                    Minutes = s.Sum(d => d.Minutes) + s.Sum(d => d.ExtraMinutes),
                    Category = s.Key.Category,
                    Sku = s.Key.SKU,
                    Flavour = s.Key.Flavor,
                    NetContent = s.Key.NetContent,
                    Packing = s.Key.Packing
                })
                .ToList();

            return downtimeSkuMinutes;
        }

        public List<DTODowntimeSkuCodesPerMinutes> GetDowntimePerSkuCodesMinutes(List<Models.Views.VwDowntimePerSku> data, DowntimeSkuFilter filters)
        {
            var downtimePerSkuCodesMinutes = data
                .Where(w => filters.Lines.Contains(w.LineID)
                        && filters.Categories.Contains(w.CategoryID)
                        && filters.Shifts.Contains(w.ShiftID)
                        && filters.Packing.Contains(w.Packing)
                        && filters.Sku.Contains(w.SKUID))
                .GroupBy(d => new
                {
                    d.Code,
                    d.SubCategory2,
                    d.Failure,
                })
                .Select(group => new DTODowntimeSkuCodesPerMinutes
                {
                    Minutes = group.Sum(d => d.Minutes) + group.Sum(d => d.ExtraMinutes),
                    Code = group.Key.Code,
                    SubCategory2 = group.Key.SubCategory2,
                    Failure = group.Key.Failure
                })
                .ToList();

            return downtimePerSkuCodesMinutes;
        }

        //* -------------------------------- Dashboard ------------------------------- */
        public async Task<DTODowntimeSkuDashboard> GetDowntimeSkuDashboard(DowntimeSkuFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = await _context.VwDowntimePerSku
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .ToListAsync();

            DTODowntimeSkuTotal totalDowntime = GetTotalDowntime(data, filters);
            List<DTODowntimeSkuTable> downtimeSkuTables = GetDowntimePerSkuTable(data, filters);
            List<DTODowntimeSkuPerMinutes> downtimeSkuMinutes = GetDowntimePerSkuMinutes(data, filters);
            List<DTODowntimeSkuCodesPerMinutes> downtimePerSkuCodesMinutes = GetDowntimePerSkuCodesMinutes(data, filters);

            DTODowntimeSkuDashboard downtimeSkuDashboard = new()
            {
                TotalDowntime = totalDowntime,
                DowntimeSkuTables = downtimeSkuTables,
                DowntimeSkuMinutes = downtimeSkuMinutes,
                DowntimePerSkuCodesMinutes = downtimePerSkuCodesMinutes
            };

            return downtimeSkuDashboard;
        }
    }
}