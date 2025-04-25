using inventio.Models.DTO.ProductionSummary;
using inventio.Repositories.Dashboards.GeneralDowntime;
using Inventio.Data;
using Inventio.Models.DTO.DailySummary;
using Inventio.Models.DTO.GeneralDowntime;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.DailySummary
{
  public class DailySummaryRepository : IDailySummaryRepository
  {
    private readonly ApplicationDBContext _context;
    private readonly IGeneralDowntimeRepository _generalDowntimeRepository;
    public DailySummaryRepository(ApplicationDBContext context, IGeneralDowntimeRepository generalDowntime)
    {
      _context = context;
      _generalDowntimeRepository = generalDowntime;
    }

    public async Task<List<EfficiencyByLine>> GetEfficiency(DailySummaryFilter filter)
    {
      DateTime Date = DateTime.Parse(filter.Date);

      var data = await _context.ProductionSummary
      .Where(w => w.Date == Date)
      .GroupBy(g => g.Line)
      .Select(g => new
      {
        Line = g.Key,
        Production = g.Sum(s => s.Production),
        MaxProduction = g.Sum(s => s.EffDen)
      })
      .ToListAsync();

      List<EfficiencyByLine> tempList = new();

      foreach (var item in data)
      {
        tempList.Add(new EfficiencyByLine
        {
          Line = item.Line,
          Efficiency = (item.Production ?? 0) / (item.MaxProduction ?? 1) * 100
        });
      }

      return tempList;
    }

    public async Task<DTODailyDashboard> GetDashboard(DailySummaryFilter filter)
    {
      DateTime Date = DateTime.Parse(filter.Date);

      var data = await _context.VwDailySummary
      .Where(w => w.Date == Date)
      .ToListAsync();

      var totalProduction = data.Sum(s => s.Production);

      var tableShift = data
      .GroupBy(g => new { g.Line, g.Shift })
      .Select(g => new DTODailyTableShift
      {
        Line = g.Key.Line,
        Shift = g.Key.Shift,
        Production = g.Sum(s => s.Production)
      })
      .ToList();

      var tablePackage = data
      .GroupBy(g => new { g.Line, g.Packing, g.Sku, g.Flavor })
      .Select(g => new DTODailyTablePackage
      {
        Line = g.Key.Line,
        Sku = g.Key.Sku,
        Flavor = g.Key.Flavor,
        Package = g.Key.Packing,
        Production = g.Sum(s => s.Production)
      })
      .ToList();

      var dataDowntime = await _context.VwDowntime
      .Where(w => w.Date == Date)
      .ToListAsync();

      var totalDowntime = dataDowntime.Sum(s => s.Minutes) / 60;

      var categorie1 = dataDowntime
      .Where(w => w.Category != "CHANGE OVER" && w.Category != "CAMBIO")
      .GroupBy(g => g.Category)
      .Select(g => new DTODailyTableCategory
      {
        Category = g.Key,
        Hours = g.Sum(s => s.Minutes) / 60
      })
      .ToList();

      var categorie2 = dataDowntime
      .Where(w => w.Category == "CHANGE OVER" || w.Category == "CAMBIO")
      .GroupBy(g => g.DowntimeSubCategory2)
      .Select(g => new DTODailyTableCategory
      {
        Category = g.Key,
        Hours = g.Sum(s => s.Minutes) / 60
      })
      .ToList();

      //TODO: Dont show in frontend
      List<DTODailyTableCategory> tableCategory = categorie1.Concat(categorie2).ToList();

      //TODO: Dont show in frontend
      var tableWarehouse = dataDowntime
      .Where(w => w.Category == "WAREHOUSE" || w.Category == "ALMACEN")
      .GroupBy(g => new { g.Line, g.Category })
      .Select(g => new DTODailyTableWarehouse
      {
        Line = g.Key.Line,
        Category = g.Key.Category,
        Hours = g.Sum(s => s.Minutes) / 60
      })
      .ToList();

      List<EfficiencyByLine> tableEfficiency = await GetEfficiency(filter);

      GeneralDowntimeFilter filterGeneralDowntime = new GeneralDowntimeFilter()
      {
        EndDate = filter.Date,
        StartDate = filter.Date
      };

      DTOGeneralDashboard tableDowntime = await _generalDowntimeRepository.GetDashboard(filterGeneralDowntime, true);

      DTODailyDashboard dailyDashboard = new()
      {
        TableShift = tableShift,
        TablePackage = tablePackage,
        TableCategory = tableCategory,
        TableWarehouse = tableWarehouse,
        TableEfficiency = tableEfficiency,
        TotalProduction = totalProduction,
        TotalDowntime = totalDowntime,
        TableGeneralDowntime = tableDowntime
      };

      return dailyDashboard;

    }

  }

}