using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeTrend;

namespace inventio.Repositories.Dashboards.TrendAnalysisRepository
{
    public interface ITrendAnalysisRepository
    {
        Task<IEnumerable<DTOReactDropdown<int>>> GetLinesAsync(DTOTrendFilter filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetCategoriesAsync(DTOTrendFilter filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetSubCategoryAsync(DTOTrendFilter filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetCodesAsync(DTOTrendFilter filters);
        Task<DTODowntimeTrendDashboard> GetTrendDashBoard(DTOTrendFilter filters);
    }
}