using inventio.Models.DTO.Menu;
using Inventio.Data;
using Inventio.Models;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.MenuRepo
{
    public class MenuNotFoundException : Exception
    {
        public MenuNotFoundException() : base("The Menu did not exists") { }
    }

    public class MenuRepository : IMenuRepository
    {

        private readonly ApplicationDBContext _context;

        public MenuRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DTOSideBarMenu>> GetSideBarMenu(string roleId)
        {
            var sideBarMenu = await _context.Menu
                .Join(
                    _context.MenuRights,
                    m => m.Id,
                    mr => mr.MenuId,
                    (m, mr) => new { m, mr }
                )
                .GroupJoin(
                    _context.Icons,
                    mmr => mmr.m.IconId,
                    i => i.Id,
                    (mmr, i) => new { mmr, i }
                )
                .SelectMany(
                    x => x.i.DefaultIfEmpty(),
                    (x, i) => new { x.mmr, i }
                )
                .Where(joined => joined.mmr.mr.RoleId == roleId)
                .Select(joined => new DTOSideBarMenu
                {
                    Id = joined.mmr.m.Id,
                    Label = joined.mmr.m.Label,
                    Route = joined.mmr.m.Route,
                    Parent_id = joined.mmr.m.ParentId,
                    Icon = joined.i != null ? joined.i.Name : null,
                    Sort = joined.mmr.m.Sort,
                    Can_view = joined.mmr.mr.CanView,
                    Can_add = joined.mmr.mr.CanAdd,
                    Can_delete = joined.mmr.mr.CanDelete,
                    Can_modify = joined.mmr.mr.CanModify
                })
                .ToListAsync();

            return sideBarMenu;
        }

        public async Task<IEnumerable<Icons>> GetIcons()
        {
            var icons = await _context.Icons.ToListAsync();

            return icons;
        }


        // MENU METHODS
        public async Task<IEnumerable<DTORootMenu>> GetRootMenu()
        {
            var rootMenu = await _context.Menu
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.Sort)
                .Select(m => new
                {
                    Menu = m,
                    Icon = _context.Icons.FirstOrDefault(i => i.Id == m.IconId)
                })
                .ToListAsync();

            return rootMenu.Select(result => new DTORootMenu
            {
                Id = result.Menu.Id,
                Label = result.Menu.Label,
                Route = result.Menu.Route,
                Parent_id = result.Menu.ParentId,
                IconId = result.Icon?.Id,
                IconName = result.Icon?.Name ?? "",
                Sort = result.Menu.Sort
            }).ToList();
        }

        public async Task<DTORootMenu> AddRootMenu(DTORootMenuRequest request)
        {
            var newMenu = new Menu
            {
                Id = request.Id,
                Label = request.Label,
                Route = request.Route,
                ParentId = null,
                IconId = request.IconId,
                Sort = request.Sort
            };

            _context.Menu.Add(newMenu);
            await _context.SaveChangesAsync();

            var icon = await _context.Icons.Where(w => w.Id == request.IconId).FirstOrDefaultAsync();
            var roles = await _context.Roles.Where(w => w.IsDelete != true).ToListAsync();

            foreach (var role in roles)
            {
                var menuRight = new MenuRights
                {
                    MenuId = newMenu.Id,
                    RoleId = role.Id,
                    CanView = false,
                    CanAdd = false,
                    CanDelete = false,
                    CanModify = false
                };

                _context.MenuRights.Add(menuRight);
            }

            await _context.SaveChangesAsync();

            var newRootMenu = new DTORootMenu
            {
                Id = newMenu.Id,
                Label = newMenu.Label,
                Route = newMenu.Route,
                Parent_id = newMenu.ParentId,
                IconId = icon?.Id,
                IconName = icon?.Name ?? "",
                Sort = newMenu.Sort
            };

            return newRootMenu;
        }

        public async Task<DTORootMenu> UpdateRootMenu(DTORootMenuRequest request)
        {
            var menuToUpdate = await _context.Menu.FindAsync(request.Id);

            if (menuToUpdate == null)
            {
                throw new MenuNotFoundException();
            }

            menuToUpdate.Label = request.Label;
            menuToUpdate.Route = request.Route;
            menuToUpdate.IconId = request.IconId;

            await _context.SaveChangesAsync();

            var updatedIcon = await _context.Icons.FirstOrDefaultAsync(i => i.Id == request.IconId);

            var updatedRootMenu = new DTORootMenu
            {
                Id = menuToUpdate.Id,
                Label = menuToUpdate.Label,
                Route = menuToUpdate.Route,
                Parent_id = menuToUpdate.ParentId,
                IconId = updatedIcon?.Id,
                IconName = updatedIcon?.Name ?? "",
                Sort = menuToUpdate.Sort
            };

            return updatedRootMenu;
        }

        public async Task<DTORootMenu> DeleteRootMenu(string menuId)
        {
            var menuToDelete = await _context.Menu.FindAsync(menuId);

            if (menuToDelete == null)
            {
                throw new MenuNotFoundException();
            }

            // Check and delete related records in MenuRights
            var relatedMenuRights = await _context.MenuRights
                .Where(mr => mr.MenuId == menuId || _context.Menu.Any(m => m.ParentId == menuId && m.Id == mr.MenuId))
                .ToListAsync();

            if (relatedMenuRights.Any())
            {
                _context.MenuRights.RemoveRange(relatedMenuRights);
                await _context.SaveChangesAsync();
            }

            // Get menu children
            var childMenus = await _context.Menu
                .Where(m => m.ParentId == menuId)
                .ToListAsync();

            // Check and delete related records in MenuRights for children
            foreach (var childMenu in childMenus)
            {
                var childMenuRights = await _context.MenuRights
                    .Where(mr => mr.MenuId == childMenu.Id)
                    .ToListAsync();

                if (childMenuRights.Any())
                {
                    _context.MenuRights.RemoveRange(childMenuRights);
                    await _context.SaveChangesAsync();
                }
            }

            // Delete children
            _context.Menu.RemoveRange(childMenus);
            await _context.SaveChangesAsync();

            // Delete parent menu
            _context.Menu.Remove(menuToDelete);
            await _context.SaveChangesAsync();


            var deletedRootMenu = new DTORootMenu
            {
                Id = menuToDelete.Id,
                Label = menuToDelete.Label,
                Route = menuToDelete.Route,
                Parent_id = menuToDelete.ParentId,
                Sort = menuToDelete.Sort,
                IconId = null,
                IconName = null,
            };

            return deletedRootMenu;
        }


        // SUBMENU METHODS

        public async Task<IEnumerable<DTOSubMenu>> GetSubMenu()
        {
            var query = from m1 in _context.Menu
                        join m2 in _context.Menu on m1.ParentId equals m2.Id
                        join i in _context.Icons on m2.IconId equals i.Id into iconGroup
                        from icon in iconGroup.DefaultIfEmpty()
                        where m1.ParentId != null
                        select new DTOSubMenu
                        {
                            Id = m1.Id,
                            Label = m1.Label,
                            Route = string.IsNullOrEmpty(m1.Route) ? "" : m1.Route,
                            Parent_id = string.IsNullOrEmpty(m1.ParentId) ? "" : m1.ParentId,
                            ParentLabel = m2.Label,
                            IconId = m2.IconId,
                            IconName = icon.Name,
                            Sort = m1.Sort
                        };

            var result = await query.ToListAsync();
            return result;
        }

        public async Task<DTOSubMenu> AddSubMenu(DTOSubMenuRequest request)
        {
            var newMenu = new Menu
            {
                Id = request.Id,
                Label = request.Label,
                Route = request.Route,
                ParentId = request.Parent_id,
                IconId = request?.IconId ?? null,
                Sort = request?.Sort ?? 0
            };

            _context.Menu.Add(newMenu);
            await _context.SaveChangesAsync();

            var roles = await _context.Roles.Where(w => w.IsDelete != true).ToListAsync();

            foreach (var role in roles)
            {
                var menuRight = new MenuRights
                {
                    MenuId = newMenu.Id,
                    RoleId = role.Id,
                    CanView = false,
                    CanAdd = false,
                    CanDelete = false,
                    CanModify = false
                };

                _context.MenuRights.Add(menuRight);
            }

            await _context.SaveChangesAsync();

            var query = from m1 in _context.Menu
                        join m2 in _context.Menu on m1.ParentId equals m2.Id
                        join i in _context.Icons on m2.IconId equals i.Id into iconGroup
                        from icon in iconGroup.DefaultIfEmpty()
                        where m1.Id == newMenu.Id
                        select new DTOSubMenu
                        {
                            Id = m1.Id,
                            Label = m1.Label,
                            Route = string.IsNullOrEmpty(m1.Route) ? "" : m1.Route,
                            Parent_id = string.IsNullOrEmpty(m1.ParentId) ? "" : m1.ParentId,
                            ParentLabel = m2.Label,
                            IconId = m2.IconId,
                            IconName = icon.Name,
                            Sort = m1.Sort
                        };

            var newSubMenuAdded = await query.FirstOrDefaultAsync();

            return newSubMenuAdded!;
        }

        public async Task<DTOSubMenu> UpdateSubMenu(DTOSubMenuRequest request)
        {
            var menuToUpdate = await _context.Menu.FindAsync(request.Id);

            if (menuToUpdate == null)
            {
                throw new MenuNotFoundException();
            }

            menuToUpdate.Label = request.Label;
            menuToUpdate.Route = request.Route;
            menuToUpdate.IconId = request.IconId;

            await _context.SaveChangesAsync();

            var updatedIcon = await _context.Icons.FirstOrDefaultAsync(i => i.Id == request.IconId);

            var updatedSubMenu = new DTOSubMenu
            {
                Id = menuToUpdate.Id,
                Label = menuToUpdate.Label,
                Route = menuToUpdate.Route,
                Parent_id = string.IsNullOrEmpty(menuToUpdate.ParentId) ? "" : menuToUpdate.ParentId,
                ParentLabel = _context.Menu.FirstOrDefault(parent => parent.Id == menuToUpdate.ParentId)?.Label,
                IconId = updatedIcon?.Id,
                IconName = updatedIcon?.Name ?? "",
                Sort = menuToUpdate.Sort
            };

            return updatedSubMenu;
        }

        public async Task<DTOSubMenu> DeleteSubMenu(string menuId)
        {
            var menuToDelete = await _context.Menu.FindAsync(menuId);

            if (menuToDelete == null)
            {
                throw new MenuNotFoundException();
            }


            var menuRightsToDelete = await _context.MenuRights
                .Where(mr => mr.MenuId == menuId)
                .ToListAsync();

            if (menuRightsToDelete.Any())
            {
                _context.MenuRights.RemoveRange(menuRightsToDelete);
                await _context.SaveChangesAsync();
            }

            _context.Menu.Remove(menuToDelete);
            await _context.SaveChangesAsync();

            var deletedSubMenu = new DTOSubMenu
            {
                Id = menuToDelete.Id,
                Label = menuToDelete.Label,
                Route = string.IsNullOrEmpty(menuToDelete.Route) ? "" : menuToDelete.Route,
                ParentLabel = _context.Menu.FirstOrDefault(parent => parent.Id == menuToDelete.ParentId)?.Label,
                Sort = menuToDelete.Sort,
                IconId = null,
                IconName = null,
            };

            return deletedSubMenu;
        }

    }
}