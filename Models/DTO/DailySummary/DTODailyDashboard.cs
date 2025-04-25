using inventio.Models.DTO.ProductionSummary;
using Inventio.Models.DTO.GeneralDowntime;

namespace Inventio.Models.DTO.DailySummary
{
  public class DTODailyDashboard
  {
    public List<DTODailyTableShift> TableShift { get; set; } = default!;
    public List<DTODailyTablePackage> TablePackage { get; set; } = default!;
    public List<DTODailyTableCategory> TableCategory { get; set; } = default!;
    public List<DTODailyTableWarehouse> TableWarehouse { get; set; } = default!;
    public List<EfficiencyByLine> TableEfficiency { get; set; } = default!;
    public decimal? TotalProduction { get; set; }
    public decimal? TotalDowntime { get; set; }
    public DTOGeneralDashboard TableGeneralDowntime { get; set; } = default!;
  }

}