using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeXSubcat;

namespace inventio.Repositories.Dashboards.DowntimeXSubCat
{
    public interface IDowntimeXSubcatRepository
    {
        Task<IEnumerable<DTOReactDropdown<int>>> GetLines(DTODowntimeFilters filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetCategories(DTODowntimeFilters filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetShift(DTODowntimeFilters filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetSubCategory(DTODowntimeFilters filters);
        Task<DTODowntimeDashboard> GetDowntimeDashBoard(DTODowntimeFilters filters);
    }
}