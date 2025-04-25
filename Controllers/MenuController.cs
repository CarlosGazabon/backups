using Microsoft.AspNetCore.Mvc;
using Inventio.Data;
using inventio.Repositories.MenuRepo;
using inventio.Models.DTO.Menu;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMenuRepository _menuRepository;

        public MenuController(ApplicationDBContext context, IMenuRepository menuRepository)
        {
            _context = context;
            _menuRepository = menuRepository;
        }

        [HttpGet("sidebar")]
        public async Task<IActionResult> GetSideBarMenu([FromQuery] string roleId)
        {
            if (_context.Menu == null || _context.MenuRights == null || _context.Icons == null)
            {
                return NotFound();
            }

            try
            {
                var sideBarMenu = await _menuRepository.GetSideBarMenu(roleId);
                return Ok(sideBarMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("icons")]
        public async Task<IActionResult> GetIcons()
        {
            try
            {
                var icons = await _menuRepository.GetIcons();
                return Ok(icons);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("rootmenu")]
        public async Task<IActionResult> GetRootMenu()
        {
            try
            {
                var rootMenu = await _menuRepository.GetRootMenu();
                return Ok(rootMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("rootmenu")]
        public async Task<IActionResult> AddRootMenu([FromBody] DTORootMenuRequest request)
        {
            try
            {
                var newRootMenu = await _menuRepository.AddRootMenu(request);
                return Ok(newRootMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("rootmenu")]
        public async Task<IActionResult> UpdateRootMenu([FromBody] DTORootMenuRequest request)
        {
            try
            {
                var updatedRootMenu = await _menuRepository.UpdateRootMenu(request);
                return Ok(updatedRootMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("rootmenu/{menuId}")]
        public async Task<IActionResult> DeleteRootMenu(string menuId)
        {
            try
            {
                var deletedRootMenu = await _menuRepository.DeleteRootMenu(menuId);
                return Ok(deletedRootMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // SUBMENU Methods:

        [HttpGet("submenu")]
        public async Task<IActionResult> GetSubMenu()
        {
            try
            {
                var subMenu = await _menuRepository.GetSubMenu();
                return Ok(subMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submenu")]
        public async Task<IActionResult> AddSubMenu([FromBody] DTOSubMenuRequest request)
        {
            try
            {
                var newSubMenu = await _menuRepository.AddSubMenu(request);
                return Ok(newSubMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("submenu")]
        public async Task<IActionResult> UpdateSubMenu([FromBody] DTOSubMenuRequest request)
        {
            try
            {
                var updatedSubMenu = await _menuRepository.UpdateSubMenu(request);
                return Ok(updatedSubMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("submenu/{menuId}")]
        public async Task<IActionResult> DeleteSubMenu(string menuId)
        {
            try
            {
                var deletedSubMenu = await _menuRepository.DeleteSubMenu(menuId);
                return Ok(deletedSubMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}