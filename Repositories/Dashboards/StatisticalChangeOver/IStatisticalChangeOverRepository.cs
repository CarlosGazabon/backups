
using inventio.Models.DTO;
using inventio.Models.DTO.StatisticalChangeOver;

namespace inventio.Repositories.Dashboards.StatisticalChangeOver
{
    public interface IStatisticalChangeOverRepository
    {
        Task<List<DTOReactDropdown<string>>> GetLines(DTOStatisticalChangeOverFilter filters);
        Task<List<DTOReactDropdown<string>>> GetShifts(DTOStatisticalChangeOverFilter filters);
        Task<List<DTOReactDropdown<string>>> GetSupervisors(DTOStatisticalChangeOverFilter filters);
        Task<List<DTOReactDropdown<string>>> GetSubCategories(DTOStatisticalChangeOverFilter filters);
        Task<List<DTOReactDropdown<string>>> GetCodes(DTOStatisticalChangeOverFilter filters);
        Task<DTOStatisticalChangeOverDashboard> GetStatisticalChangeOverDashboard(DTOStatisticalChangeOverFilter filters);
    }
}