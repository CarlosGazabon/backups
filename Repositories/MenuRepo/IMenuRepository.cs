using inventio.Models.DTO.Menu;
using Inventio.Models;

namespace inventio.Repositories.MenuRepo
{
    public interface IMenuRepository
    {
        Task<IEnumerable<DTOSideBarMenu>> GetSideBarMenu(string roleId);
        Task<IEnumerable<Icons>> GetIcons();

        // Menu Methods
        Task<IEnumerable<DTORootMenu>> GetRootMenu();
        Task<DTORootMenu> AddRootMenu(DTORootMenuRequest request);
        Task<DTORootMenu> UpdateRootMenu(DTORootMenuRequest request);
        Task<DTORootMenu> DeleteRootMenu(string menuId);

        // SubMenu Methods
        Task<IEnumerable<DTOSubMenu>> GetSubMenu();
        Task<DTOSubMenu> AddSubMenu(DTOSubMenuRequest request);
        Task<DTOSubMenu> UpdateSubMenu(DTOSubMenuRequest request);
        Task<DTOSubMenu> DeleteSubMenu(string menuId);
    }
}