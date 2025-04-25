using inventio.Models.DTO.Rights;
using inventio.Repositories.Rights;
using Inventio.Data;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/menu-rights")]
    public class MenuRightsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMenuRightsRepository _menuRightsRepository;

        public MenuRightsController(ApplicationDBContext context, IMenuRightsRepository menuRightsRepository)
        {
            _context = context;
            _menuRightsRepository = menuRightsRepository;
        }

        [HttpGet("roles")]
        public async Task<IEnumerable<DTODropdown>> GetRoles()
        {
            return await _menuRightsRepository.GetRoles();
        }

        [HttpGet("menus")]
        public async Task<IEnumerable<DTODropdown>> GetMenus()
        {
            return await _menuRightsRepository.GetMenus();
        }

        [HttpGet("submenus/{menuId}")]
        public async Task<IEnumerable<DTODropdown>> GetSubMenus(string menuId)
        {
            return await _menuRightsRepository.GetSubMenus(menuId);
        }

        [HttpPost("rights/exists")]
        public async Task<IActionResult> RightExists(DTOExistsRightRequest request)
        {
            bool result = await _menuRightsRepository.ExistsMenuRight(request);

            return Ok(result);
        }

        [HttpGet("rights/{roleId}")]
        public async Task<IEnumerable<DTOMenuRights>> GetRights(string roleId)
        {
            return await _menuRightsRepository.GetRights(roleId);
        }

        [HttpPost("add-rights")]
        public async Task<IActionResult> AddRights(DTORightsRequest request)
        {
            try
            {
                var menuRightAdded = await _menuRightsRepository.AddRights(request);
                return Ok(menuRightAdded);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("update-rights")]
        public async Task<IActionResult> UpdateRights(List<DTORightsRequest> requests)
        {
            try
            {
                var menuRightUpdated = await _menuRightsRepository.UpdateRights(requests);
                if (menuRightUpdated == null)
                {
                    return NotFound("MenuRights do not exist for this Menu and Role.");
                }
                return Ok(menuRightUpdated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}