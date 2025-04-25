using inventio.Models.DTO.Rights;
using Inventio.Data;
using Inventio.Models;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Rights
{
    public class MenuRightsRepository : IMenuRightsRepository
    {

        private readonly ApplicationDBContext _context;

        public MenuRightsRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DTODropdown>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new DTODropdown
                {
                    Value = r.Id,
                    Label = r.Name
                })
                .ToListAsync();

            return roles;
        }

        public async Task<IEnumerable<DTODropdown>> GetMenus()
        {
            var rootMenus = await _context.Menu
                 .Where(m => m.ParentId == null)
                 .Select(m => new DTODropdown
                 {
                     Value = m.Id,
                     Label = m.Label
                 })
                 .ToListAsync();

            return rootMenus;
        }

        public async Task<IEnumerable<DTODropdown>> GetSubMenus(string menuId)
        {

            var subMenus = await _context.Menu
                .Where(m => m.ParentId == menuId)
                .Select(m => new DTODropdown
                {
                    Value = m.Id,
                    Label = m.Label
                })
                .ToListAsync();

            return subMenus;
        }

        public async Task<IEnumerable<DTOMenuRights>> GetRights(string roleId)
        {

            var menuRights = await _context.Menu
                .Join(
                    _context.MenuRights,
                    m => m.Id,
                    mr => mr.MenuId,
                    (m, mr) => new { m, mr }
                )
                .GroupJoin(
                    _context.Menu,
                    mmr => mmr.m.ParentId,
                    m2 => m2.Id,
                    (mmr, m2) => new { mmr, m2 }
                )
                .SelectMany(
                    x => x.m2.DefaultIfEmpty(),
                    (x, m2) => new { x.mmr, m2 }
                )
                .Where(joined => joined.mmr.mr.RoleId == roleId)
                .Select(joined => new DTOMenuRights
                {
                    Id = joined.mmr.m.Id,
                    Label = joined.mmr.m.Label,
                    ParentLabel = joined.m2 != null ? joined.m2.Label : null,
                    CanView = joined.mmr.mr.CanView,
                    CanAdd = joined.mmr.mr.CanAdd,
                    CanModify = joined.mmr.mr.CanModify,
                    CanDelete = joined.mmr.mr.CanDelete
                })
                .ToListAsync();

            return menuRights;
        }


        public async Task<DTOMenuRights> AddRights(DTORightsRequest request)
        {

            bool rightsExist = await _context.MenuRights
                   .AnyAsync(mr => mr.MenuId == request.MenuId && mr.RoleId == request.RoleId);

            if (rightsExist)
            {
                throw new InvalidOperationException("MenuRights already exists for this Menu and Role.");
            }

            var menuRight = new MenuRights
            {
                MenuId = request.MenuId,
                RoleId = request.RoleId,
                CanView = request.CanView,
                CanAdd = request.CanAdd,
                CanDelete = request.CanDelete,
                CanModify = request.CanModify
            };

            _context.MenuRights.Add(menuRight);

            await _context.SaveChangesAsync();

            // Get Right Added

            var menuRightAdded = await _context.Menu
                .Join(
                    _context.MenuRights,
                    m => m.Id,
                    mr => mr.MenuId,
                    (m, mr) => new { m, mr }
                )
                .GroupJoin(
                    _context.Menu,
                    mmr => mmr.m.ParentId,
                    m2 => m2.Id,
                    (mmr, m2) => new { mmr, m2 }
                )
                .SelectMany(
                    x => x.m2.DefaultIfEmpty(),
                    (x, m2) => new { x.mmr, m2 }
                )
                .Where(joined => joined.mmr.mr.RoleId == menuRight.RoleId && joined.mmr.mr.MenuId == menuRight.MenuId)
                .Select(joined => new DTOMenuRights
                {
                    Id = joined.mmr.m.Id,
                    ParentLabel = joined.m2 != null ? joined.m2.Label : null,
                    CanView = joined.mmr.mr.CanView,
                    CanAdd = joined.mmr.mr.CanAdd,
                    CanModify = joined.mmr.mr.CanModify,
                    CanDelete = joined.mmr.mr.CanDelete
                })
                .FirstOrDefaultAsync();

            return menuRightAdded;
        }

        public async Task<List<DTORightsRequest>> UpdateRights(List<DTORightsRequest> requests)
        {
            foreach (var request in requests)
            {
                var existingRights = await _context.MenuRights
                    .FirstOrDefaultAsync(mr => mr.MenuId == request.MenuId && mr.RoleId == request.RoleId) ?? throw new InvalidOperationException($"MenuRights do not exist for MenuId {request.MenuId} and RoleId {request.RoleId}.");

                existingRights.CanView = request.CanView;
                existingRights.CanAdd = request.CanAdd;
                existingRights.CanDelete = request.CanDelete;
                existingRights.CanModify = request.CanModify;

                _context.MenuRights.Update(existingRights);
                await _context.SaveChangesAsync();
            }


            return requests;
        }

        public async Task<bool> ExistsMenuRight(DTOExistsRightRequest request)
        {
            bool rightsExist = await _context.MenuRights
                   .AnyAsync(mr => mr.MenuId == request.MenuId && mr.RoleId == request.RoleId);

            return rightsExist;
        }
    }
}