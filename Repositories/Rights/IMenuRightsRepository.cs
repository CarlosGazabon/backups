
using inventio.Models.DTO.Rights;

namespace inventio.Repositories.Rights
{
    public interface IMenuRightsRepository
    {
        Task<IEnumerable<DTODropdown>> GetRoles();
        Task<IEnumerable<DTODropdown>> GetMenus();
        Task<IEnumerable<DTODropdown>> GetSubMenus(string menuId);
        Task<IEnumerable<DTOMenuRights>> GetRights(string roleId);
        Task<DTOMenuRights> AddRights(DTORightsRequest request);
        Task<List<DTORightsRequest>> UpdateRights(List<DTORightsRequest> request);
        Task<bool> ExistsMenuRight(DTOExistsRightRequest request);
    }
}