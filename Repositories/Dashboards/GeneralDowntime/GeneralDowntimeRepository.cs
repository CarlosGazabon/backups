using inventio.Repositories.Dashboards.GeneralDowntime;
using Inventio.Data;
using Inventio.Models.DTO.GeneralDowntime;
using Inventio.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.GeneralDowntime
{
  public class GeneralDowntimeRepository : IGeneralDowntimeRepository
  {
    private readonly ApplicationDBContext _context;

    public GeneralDowntimeRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    /* Filters */
    public async Task<IEnumerable<string>> GetLine(GeneralDowntimeFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var query = await _context.VwDowntime
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .Select(s => s.Line)
      .Distinct()
      .ToListAsync();

      return query;
    }

    public async Task<IEnumerable<string>> GetCategories(GeneralDowntimeFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var query = await _context.VwDowntime
      .Where(w => w.Date >= startDate && w.Date <= endDate
        && filters.Lines!.Contains(w.Line))
      .Select(s => s.Category)
      .Distinct()
      .ToListAsync();

      return query;
    }

    public async Task<IEnumerable<string>> GetShifts(GeneralDowntimeFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var query = await _context.VwDowntime
      .Where(w => w.Date >= startDate && w.Date <= endDate
        && filters.Lines!.Contains(w.Line)
        && filters.Categories!.Contains(w.Category))
      .Select(s => s.Shift)
      .Distinct()
      .ToListAsync();

      return query;
    }
    /* End Filters */

    /* Charts */
    // public List<DTOGeneralColumnStack> DataColumnStack(void data) { }
    /* End Charts */

    public async Task<DTOGeneralDashboard> GetDashboard(GeneralDowntimeFilter filters, bool allFilters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      IQueryable<VwDowntime> query = _context.VwDowntime
      .Where(w => w.Date >= startDate && w.Date <= endDate);

      if (!allFilters)
      {
        query = query.Where(w => w.Date >= startDate && w.Date <= endDate
        && filters.Lines!.Contains(w.Line)
        && filters.Shifts!.Contains(w.Shift)
        && filters.Categories!.Contains(w.Category));
      }

      var data = await query
      .Select(s => new { s.Line, s.Category, s.Minutes, s.ExtraMinutes })
      .ToListAsync();

      List<DTOGeneralColumn> column = data
      .GroupBy(g => g.Category)
      .Select(g => new DTOGeneralColumn
      {
        Minutes = g.Sum(s => s.Minutes) + g.Sum(s => s.ExtraMinutes),
        Category = g.Key
      }).ToList();

      List<DTOGeneralColumnStack> columStack = data
      .GroupBy(g => new { g.Line, g.Category })
      .Select(g => new DTOGeneralColumnStack
      {
        Line = g.Key.Line,
        Category = g.Key.Category,
        Minutes = g.Sum(s => s.Minutes) + g.Sum(s => s.ExtraMinutes)
      })
      .ToList();

      DTOGeneralDashboard generalDashboard = new()
      {
        GeneralColumnStack = columStack,
        GeneralColumn = column
      };

      return generalDashboard;
    }

  }
}