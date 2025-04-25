using System.Data;
using inventio.Models.DTO.Cases;
using Inventio.Models.DTO.Cases;

namespace inventio.Repositories.Dashboards.CasesAnalysis
{
  public interface ICasesAnalysisRepository
  {
    Task<IEnumerable<string>> GetLines(CasesFilter filters);
    Task<DTOCasesDashboard> GetDashboard(CasesFilter filters);
    Task<DataTable> ExcelCasesPerLine(CasesFilter filters);
  }
}