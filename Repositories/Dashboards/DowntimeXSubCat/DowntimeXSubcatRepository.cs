using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeXSubcat;
using Inventio.Data;
using Inventio.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.DowntimeXSubCat
{
    public class DowntimeXSubcatRepository : IDowntimeXSubcatRepository
    {

        private readonly ApplicationDBContext _context;

        public DowntimeXSubcatRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        // private functions

        private List<DTOMinutesXSubcategory> GetDowntimeMinutesXSubCategory(DTODowntimeFilters filters, List<VwDowntimeXSubCat> baseList)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = baseList
              .GroupBy(g => new { g.SubCategory2Id, g.SubCategory2 })
              .Select(s => new DTOMinutesXSubcategory
              {
                  SubCategory = s.Key.SubCategory2,
                  Minutes = s.Sum(obj => obj.Minutes) + s.Sum(obj => obj.ExtraMinutes),
                  Hours = s.Sum(obj => obj.Hours)
              })
              .ToList();


            return result;
        }


        private List<DTOMinutesXCode> GetDowntimeMinutesXCode(DTODowntimeFilters filters, List<VwDowntimeXSubCat> baseList)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

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
                  Minutes = s.Sum(obj => obj.Minutes) + s.Sum(obj => obj.ExtraMinutes),
              })
              .ToList();


            return result;
        }

        // public functions
        public async Task<IEnumerable<DTOReactDropdown<int>>> GetLines(DTODowntimeFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeXSubCat
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Line,
                    Value = s.LineId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetCategories(DTODowntimeFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeXSubCat
                .Where(w => filters.Lines.Contains(w.LineId) && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Category,
                    Value = s.CategoryId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetShift(DTODowntimeFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeXSubCat
                .Where(w => filters.Lines.Contains(w.LineId)
                            && filters.Categories.Contains(w.CategoryId)
                            && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Shift,
                    Value = s.ShiftId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetSubCategory(DTODowntimeFilters filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeXSubCat
                .Where(w => filters.Lines.Contains(w.LineId)
                            && filters.Categories.Contains(w.CategoryId)
                            && filters.Shifts.Contains(w.ShiftId)
                            && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.SubCategory2,
                    Value = s.SubCategory2Id
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<DTODowntimeDashboard> GetDowntimeDashBoard(DTODowntimeFilters filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<VwDowntimeXSubCat> baseList = await _context.VwDowntimeXSubCat
              .Where(w => filters.Lines.Contains(w.LineId)
              && filters.SubCategories.Contains(w.SubCategory2Id)
              && filters.Shifts.Contains(w.ShiftId) && w.Date >= startDate && w.Date <= endDate).ToListAsync();

            List<DTOMinutesXSubcategory> minutesXSubcategories = GetDowntimeMinutesXSubCategory(filters, baseList);
            List<DTOMinutesXCode> minutesXCodes = GetDowntimeMinutesXCode(filters, baseList);

            DTODowntimeDashboard downtimeDashboard = new DTODowntimeDashboard
            {
                MinutesXSubCategory = minutesXSubcategories,
                MinutesXCode = minutesXCodes
            };

            return downtimeDashboard;
        }
    }
}