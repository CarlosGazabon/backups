using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public UtilController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // GET: api/Util
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<string> GetUtil()
        {
            return "OK UtilController";
        }

        public struct RolesMappingHelperRequest
        {
            public string RoleId { get; set; }
        }

        // POST: api/Util/RolesMappingHelper
        [HttpPost("roles-mapping-helper")]
        [AllowAnonymous]
        public async Task<IActionResult> PostRolesMappingHelper(RolesMappingHelperRequest requestInfo)
        // 
        {
            var menuIds = await _context.Menu.Select(m => m.Id).ToArrayAsync();
            foreach (var id in menuIds)
            {
                var menuRightExists = await _context.MenuRights
                    .AnyAsync(mr => mr.MenuId == id && mr.RoleId == requestInfo.RoleId);

                if (!menuRightExists)
                {
                    var newMenuRight = new MenuRights
                    {
                        MenuId = id,
                        RoleId = requestInfo.RoleId
                    };

                    _context.MenuRights.Add(newMenuRight);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok("OK2");
        }

    }
}
