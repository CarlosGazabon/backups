using Microsoft.EntityFrameworkCore;
using inventio.Models.DTO.Role;
using Inventio.Data;
using Inventio.Models;
using Microsoft.AspNetCore.Identity;

namespace inventio.Repositories.Roles
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDBContext _context;

        public RoleRepository(RoleManager<Role> roleManager, ApplicationDBContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }


        private static RoleResponseDto TransformRoleToRoleDto(Role role)
        {
            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name
            };
        }

        private static IEnumerable<RoleResponseDto> TransformRoleArrayToRoleDto(IEnumerable<Role> roles)
        {
            List<RoleResponseDto> roleList = new();

            foreach (var role in roles)
            {
                RoleResponseDto aux = new()
                {
                    Id = role.Id,
                    Name = role.Name
                };

                roleList.Add(aux);

            }

            return roleList;
        }

        public async Task<RoleResponseDto> GetRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id) ?? throw new Exception("Role does not exists");

            return TransformRoleToRoleDto(role);

        }

        public async Task<IEnumerable<RoleResponseDto>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.Where(w => w.IsDelete != true).ToListAsync();

            return TransformRoleArrayToRoleDto(roles);
        }

        public async Task<RoleResponseDto> AddRole(AddRoleRequest request)
        {
            var existsRole = await _context.Roles.Where(w => w.Name == request.Name).AnyAsync();

            if (existsRole)
            {
                throw new Exception("Role already exists");
            }

            var role = new Role
            {
                Name = request.Name
            };

            var result = await _roleManager.CreateAsync(role);

            var menuIds = await _context.Menu.Select(m => m.Id).ToArrayAsync();
            foreach (var id in menuIds)
            {
                var menuRightExists = await _context.MenuRights.AnyAsync(mr => mr.MenuId == id && mr.RoleId == role.Id);

                if (!menuRightExists)
                {
                    var newMenuRight = new MenuRights
                    {
                        MenuId = id,
                        RoleId = role.Id
                    };

                    _context.MenuRights.Add(newMenuRight);
                    await _context.SaveChangesAsync();
                }
            }

            if (result.Succeeded)
            {
                return TransformRoleToRoleDto(role!);
            }

            throw new Exception("Failed to register user");
        }

        public async Task<RoleResponseDto> UpdateRole(string id, AddRoleRequest request)
        {
            var role = await _roleManager.FindByIdAsync(id) ?? throw new Exception("Role does not exist");

            role.Name = request.Name;

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return TransformRoleToRoleDto(role);
            }

            throw new Exception("Failed to update role");
        }

        public async Task<RoleResponseDto> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id) ?? throw new Exception("Role does not exist");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return TransformRoleToRoleDto(role);
            }

            throw new Exception("Failed to delete role");
        }

        public async Task<RoleResponseDto> SoftDeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id) ?? throw new Exception("Role does not exist");
            role.IsDelete = true;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to delete role");
            }

            return TransformRoleToRoleDto(role);
        }

        public async Task<IEnumerable<RoleAccess>> GetRoleAccessByRoleName(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role != null)
            {
                return await _context.RoleAccess
                    .Where(ra => ra.RoleId == role.Id)
                    .ToListAsync();
            }
            else
            {

                return new List<RoleAccess>();
            }
        }

        public async Task<IEnumerable<RoleAccess>> CreateOrUpdateRoleAccessList(IEnumerable<RoleAccess> roleAccessList)
        {
            var createdOrUpdatedRoleAccessList = new List<RoleAccess>();

            foreach (var roleAccess in roleAccessList)
            {
                var existingRoleAccess = await _context.RoleAccess
                    .FirstOrDefaultAsync(ra => ra.MenuId == roleAccess.MenuId && ra.RoleId == roleAccess.RoleId);

                if (existingRoleAccess == null)
                {
                    // If the RoleAccess record does not exist, create a new record.
                    var newRoleAccess = new RoleAccess
                    {
                        RoleId = roleAccess.RoleId,
                        MenuId = roleAccess.MenuId,
                        Active = roleAccess.Active
                    };

                    _context.RoleAccess.Add(newRoleAccess);
                    await _context.SaveChangesAsync();

                    createdOrUpdatedRoleAccessList.Add(newRoleAccess);
                }
                else
                {
                    // If the RoleAccess record already exists, update its properties.
                    existingRoleAccess.Active = roleAccess.Active;
                    await _context.SaveChangesAsync();

                    createdOrUpdatedRoleAccessList.Add(existingRoleAccess);
                }
            }

            return createdOrUpdatedRoleAccessList;
        }

        public async Task<IEnumerable<RoleRights>> GetRoleRightsByRoleName(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role != null)
            {
                return await _context.RoleRights
                    .Where(ra => ra.RoleId == role.Id)
                    .ToListAsync();
            }
            else
            {

                return new List<RoleRights>();
            }
        }

        public async Task<IEnumerable<RoleRights>> GetRoleRightsByRoleId(string roleId)
        {
            return await _context.RoleRights
                .Where(rr => rr.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoleRights>> UpdateOrCreateRoleRights(IEnumerable<RoleRights> roleRightsList)
        {
            var updatedRoleRightsList = new List<RoleRights>();

            foreach (var roleRights in roleRightsList)
            {
                // Check if a record with the same RoleId and MenuId exists
                var existingRoleRights = await _context.RoleRights.FirstOrDefaultAsync(rr =>
                    rr.RoleId == roleRights.RoleId && rr.MenuId == roleRights.MenuId);

                if (existingRoleRights != null)
                {
                    // If the record exists, update its properties
                    existingRoleRights.AllowView = roleRights.AllowView;
                    existingRoleRights.AllowInsert = roleRights.AllowInsert;
                    existingRoleRights.AllowEdit = roleRights.AllowEdit;
                    existingRoleRights.AllowDelete = roleRights.AllowDelete;

                    // Save changes to the existing record
                    await _context.SaveChangesAsync();

                    updatedRoleRightsList.Add(existingRoleRights);
                }
                else
                {
                    // If the record does not exist, create a new record
                    var newRoleRights = new RoleRights
                    {
                        RoleId = roleRights.RoleId,
                        MenuId = roleRights.MenuId,
                        MenuName = roleRights.MenuName,
                        AllowView = roleRights.AllowView,
                        AllowInsert = roleRights.AllowInsert,
                        AllowEdit = roleRights.AllowEdit,
                        AllowDelete = roleRights.AllowDelete
                    };

                    // Add the new record to the context and save changes
                    _context.RoleRights.Add(newRoleRights);
                    await _context.SaveChangesAsync();

                    updatedRoleRightsList.Add(newRoleRights);
                }
            }

            return updatedRoleRightsList;
        }

    }
}