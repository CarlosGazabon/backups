using inventio.Models.DTO.Role;
using Inventio.Models;

namespace inventio.Repositories.Roles
{
    public interface IRoleRepository
    {
        Task<RoleResponseDto> GetRole(string id);
        Task<IEnumerable<RoleResponseDto>> GetRolesAsync();
        Task<RoleResponseDto> AddRole(AddRoleRequest request);
        Task<RoleResponseDto> UpdateRole(string id, AddRoleRequest request);
        Task<RoleResponseDto> DeleteRole(string id);
        Task<RoleResponseDto> SoftDeleteRole(string id);
        Task<IEnumerable<RoleAccess>> GetRoleAccessByRoleName(string roleName);
        Task<IEnumerable<RoleAccess>> CreateOrUpdateRoleAccessList(IEnumerable<RoleAccess> roleAccessList);
        Task<IEnumerable<RoleRights>> GetRoleRightsByRoleName(string roleName);
        Task<IEnumerable<RoleRights>> GetRoleRightsByRoleId(string roleId);
        Task<IEnumerable<RoleRights>> UpdateOrCreateRoleRights(IEnumerable<RoleRights> roleRightsList);
    }
}