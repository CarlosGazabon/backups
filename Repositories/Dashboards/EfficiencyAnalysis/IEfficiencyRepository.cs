using System.Data;
using inventio.Models.StoredProcedure;
using inventio.Models.DTO;
using inventio.Models.DTO.Efficiency;

namespace inventio.Repositories.Dashboards.EfficiencyAnalysis
{
  public interface IEfficiencyRepository
  {
    Task<IEnumerable<DTOReactDropdown<int>>> GetLines(EfficiencyFilter filters);
    Task<DTOEfficiencyDashboard> GetDashboard(EfficiencyFilter filters);
    Task<DataTable> ExcelEfficiencyPerLine(EfficiencyFilter filters);
    Task<DataTable> ExcelEfficiencyPerTimePeriod(EfficiencyFilter filters);
    Task<List<SPGetDataEfficiency>> Test(EfficiencyFilter filters);
  }
}