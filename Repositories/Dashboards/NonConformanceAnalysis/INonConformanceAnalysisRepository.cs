
using inventio.Models.DTO.NonConformance;
using Inventio.Models;

namespace inventio.Repositories.Dashboards.NonConformanceAnalysis
{
  public interface INonConformanceAnalysisRepository
  {
    Task<IEnumerable<Line>> GetLines(NonConformanceFilter filters);
    Task<DTONonConformanceDashboard> GetDashboard(NonConformanceFilter filters);
  }
}
