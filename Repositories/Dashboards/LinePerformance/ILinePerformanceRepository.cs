using inventio.Models.DTO;
using inventio.Models.DTO.LinePerformance;

namespace inventio.Repositories.Dashboards.LinePerformance
{
    public interface ILinePerformanceRepository
    {
        Task<List<DTOReactDropdown<string>>> GetLines(LinePerformanceFilters filters);
        Task<DTOLinePerformanceDashboard> LinePerformanceDashboard(LinePerformanceFilters filters);
        Task<DTOLineObjectives?> GetLineObjectives(LinePerformanceFilters filters);
    }
}