using inventio.Models.DTO.Role;
using inventio.Repositories.Roles;
using Inventio.Models;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {

        private readonly IRoleRepository _repository;

        public RoleController(
            IRoleRepository repository
        )
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetUsers()
        {
            var result = await _repository.GetRolesAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetUser(string id)
        {
            var result = await _repository.GetRole(id);

            return Ok(result);
        }


        [HttpPost("create")]
        public async Task<ActionResult<RoleResponseDto>> CreateRole(
            [FromBody] AddRoleRequest request
        )
        {
            var result = await _repository.AddRole(request);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoleResponseDto>> UpdateRole(string id, [FromBody] AddRoleRequest request)
        {
            var result = await _repository.UpdateRole(id, request);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<RoleResponseDto>> DeleteRole(string id)
        {
            var result = await _repository.SoftDeleteRole(id);
            return Ok(result);
        }


        [HttpGet("role-access/{roleName}")]
        public async Task<ActionResult<IEnumerable<RoleAccess>>> GetRoleAccessByRoleName(string roleName)
        {
            var roleAccessList = await _repository.GetRoleAccessByRoleName(roleName);
            return Ok(roleAccessList);
        }

        [HttpPost("role-access/update-or-create")]
        public async Task<ActionResult<IEnumerable<RoleAccess>>> CreateOrUpdateRoleAccessList(IEnumerable<RoleAccess> roleAccessList)
        {
            var result = await _repository.CreateOrUpdateRoleAccessList(roleAccessList);
            return Ok(result);
        }

        [HttpGet("role-rights/{roleName}")]
        public async Task<ActionResult<IEnumerable<RoleRights>>> GetRoleRightsByRoleName(string roleName)
        {
            var roleAccessList = await _repository.GetRoleRightsByRoleName(roleName);
            return Ok(roleAccessList);
        }

        [HttpGet("role-rights/role-id/{roleId}")]
        public async Task<ActionResult<IEnumerable<RoleRights>>> GetRoleRightsByRoleId(string roleId)
        {
            var roleRightsList = await _repository.GetRoleRightsByRoleId(roleId);
            return Ok(roleRightsList);
        }

        [HttpPost("role-rights/update-or-create")]
        public async Task<ActionResult<IEnumerable<RoleRights>>> UpdateOrCreateRoleRights([FromBody] IEnumerable<RoleRights> roleRightsList)
        {
            if (roleRightsList == null || !roleRightsList.Any())
            {
                return BadRequest("Invalid RoleRights data");
            }

            try
            {
                var result = await _repository.UpdateOrCreateRoleRights(roleRightsList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update or create RoleRights", error = ex.Message });
            }
        }
    }
}