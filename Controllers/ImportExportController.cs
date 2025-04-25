using ClosedXML.Excel;
using Inventio.Data;
using Inventio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Inventio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportExportController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public readonly IPasswordHasher<User> _passwordHasher;

        public ImportExportController(ApplicationDBContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }



        [AllowAnonymous]
        [HttpGet("export")]
        public async Task<IActionResult> ExportInventioDataToExcel()
        {
            try
            {
                var helper = new ImportExportHelper(_context);
                DataTable dtLines = await helper.GetLinesDataTable();
                DataTable dtProducts = await helper.GetProductsDataTable();
                DataTable dtShifts = await helper.GetShiftstDataTable();


                using XLWorkbook wb = new();
                wb.Worksheets.Add(dtLines);
                wb.Worksheets.Add(dtProducts);
                wb.Worksheets.Add(dtShifts);
                using MemoryStream stream = new();
                wb.SaveAs(stream);
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "Lines.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }



        [AllowAnonymous]
        [HttpPost("import")]
        public async Task<IActionResult> ImportInventioDataFromExcel(IFormFile file)
        {
            var helper = new ImportExportHelper(_context);

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("El archivo está vacío o no se ha seleccionado ningún archivo.");
                }

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("El archivo debe ser un archivo Excel (.xlsx).");
                }

                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);



                // LINES
                var rows = helper.GetRowsBySheetName(workbook, "Lines");

                foreach (var row in rows)
                {
                    var line = helper.MapExcelRowToLine(row);
                    _context.Line.Add(line);
                }

                await _context.SaveChangesAsync();

                // PRODUCTS            
                var rowsProducts = helper.GetRowsBySheetName(workbook, "Products");

                foreach (var row in rowsProducts)
                {
                    var product = helper.MapExcelRowToProduct(row);

                    _context.Product.Add(product);
                }

                await _context.SaveChangesAsync();

                // SHIFTS
                var rowsShifts = helper.GetRowsBySheetName(workbook, "Shifts");

                foreach (var row in rowsShifts)
                {
                    var shift = helper.MapExcelRowToShift(row);

                    _context.Shift.Add(shift);
                }

                await _context.SaveChangesAsync();

                // FEATURE FLAGS
                var rowsFeatureFlags = helper.GetRowsBySheetName(workbook, "FeatureFlags");

                foreach (var row in rowsFeatureFlags)
                {
                    var featureFlags = helper.MapExcelRowToFeatureFlags(row);

                    _context.FeatureFlags.Add(featureFlags);
                }

                // HOURS
                var rowsHours = helper.GetRowsBySheetName(workbook, "Hours");

                foreach (var row in rowsHours)
                {
                    var hours = helper.MapExcelRowToHour(row);

                    _context.Hour.Add(hours);

                }

                // QualityIncidentReason
                var rowsQualityIncidentReason = helper.GetRowsBySheetName(workbook, "QualityIncidentReason");

                foreach (var row in rowsQualityIncidentReason)
                {
                    var qualityIncidentReason = helper.MapExcelRowToQualityIncidentReason(row);

                    _context.QualityIncidentReason.Add(qualityIncidentReason);

                }

                // Supervisor
                var rowsSupervisor = helper.GetRowsBySheetName(workbook, "Supervisor");

                foreach (var row in rowsSupervisor)
                {
                    var supervisor = helper.MapExcelRowToSupervisor(row);

                    _context.Supervisor.Add(supervisor);

                }

                // DowntimeCategory
                var rowsDowntimeCategory = helper.GetRowsBySheetName(workbook, "DowntimeCategory");

                foreach (var row in rowsDowntimeCategory)
                {
                    var downtimeCategory = helper.MapExcelRowToDowntimeCategory(row);

                    _context.DowntimeCategory.Add(downtimeCategory);

                }
                await _context.SaveChangesAsync();

                // DowntimeSubCategory1
                var rowsDowntimeSubCategory1 = helper.GetRowsBySheetName(workbook, "DowntimeSubCategory1");

                foreach (var row in rowsDowntimeSubCategory1)
                {
                    var downtimeSubCategory1 = helper.MapExcelRowToDowntimeSubCategory1(row);

                    _context.DowntimeSubCategory1.Add(downtimeSubCategory1);

                }
                await _context.SaveChangesAsync();

                // DowntimeSubCategory2
                var rowsDowntimeSubCategory2 = helper.GetRowsBySheetName(workbook, "DowntimeSubCategory2");

                foreach (var row in rowsDowntimeSubCategory2)
                {
                    var downtimeSubCategory2 = helper.MapExcelRowToDowntimeSubCategory2(row);

                    _context.DowntimeSubCategory2.Add(downtimeSubCategory2);

                }
                await _context.SaveChangesAsync();

                // DowntimeCode
                var rowsDowntimeCode = helper.GetRowsBySheetName(workbook, "DowntimeCode");

                foreach (var row in rowsDowntimeCode)
                {
                    var downtimeCode = helper.MapExcelRowToDowntimeCode(row);

                    _context.DowntimeCode.Add(downtimeCode);

                }
                // await _context.CommitTransactionAsync();
                await _context.SaveChangesAsync();


                // import stats
                var importStats = new
                {
                    Lines = rows.Count(),
                    Products = rowsProducts.Count(),
                    Shifts = rowsShifts.Count(),
                    FeatureFlags = rowsFeatureFlags.Count(),
                    Hours = rowsHours.Count(),
                    // Icons = rowsIcons.Count(),
                    QualityIncidentReason = rowsQualityIncidentReason.Count(),
                    Supervisor = rowsSupervisor.Count(),
                    DowntimeCategory = rowsDowntimeCategory.Count(),
                    DowntimeSubCategory1 = rowsDowntimeSubCategory1.Count(),
                    DowntimeSubCategory2 = rowsDowntimeSubCategory2.Count(),
                    DowntimeCode = rowsDowntimeCode.Count()
                };

                // Done processing excel reading
                var successMessage = "Líneas+Products+Shifts+Feature-Flags+Hour+Icons+QualityIncidentReason+Supervisor+DowntimeCategory+DowntimeSubCategory1+DowntimeSubCategory2+DowntimeCode importadas correctamente.";

                return Ok(new { message = successMessage, stats = importStats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("import-users-data")]
        public async Task<IActionResult> ImportInventioUsersDataFromExcel(IFormFile file)
        {
            var helper = new ImportExportHelper(_context);

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("El archivo está vacío o no se ha seleccionado ningún archivo.");
                }

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("El archivo debe ser un archivo Excel (.xlsx).");
                }

                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                // ROLES
                var rowsRoles = helper.GetRowsBySheetName(workbook, "Roles");

                foreach (var row in rowsRoles)
                {
                    var rol = helper.MapExcelRowToRole(row);

                    IdentityRole applicationRole = new Role();
                    Guid guidRole = Guid.NewGuid();
                    applicationRole.Id = guidRole.ToString();
                    applicationRole.Name = rol.Name;
                    applicationRole.NormalizedName = rol.Name?.ToUpper();

                    _context.Roles.Add((Role)applicationRole);
                }

                await _context.SaveChangesAsync();

                // USERS
                var rowsUsers = helper.GetRowsBySheetName(workbook, "Users");

                foreach (var row in rowsUsers)
                {
                    var user = helper.MapExcelRowToUser(row);

                    IdentityUser applicationUser = new User();
                    Guid guid = Guid.NewGuid();
                    applicationUser.Id = guid.ToString();
                    applicationUser.UserName = user.UserName;
                    applicationUser.NormalizedUserName = user.Name?.ToUpper();
                    applicationUser.Email = user.Email;
                    applicationUser.NormalizedEmail = user.Email?.ToUpper();
                    applicationUser.PhoneNumber = user.PhoneNumber;

                    await _context.Users.AddAsync((User)applicationUser);

                    var hashedPassword = _passwordHasher.HashPassword((User)applicationUser, user.Password) as string;
                    applicationUser.SecurityStamp = Guid.NewGuid().ToString();
                    applicationUser.PasswordHash = hashedPassword;

                    // add role to user
                    var userForRole = await _context.Users.FindAsync(applicationUser.Id);
                    var role = _context.Roles.Where(r => r.Name == user.Rol).FirstOrDefault();


                    if (userForRole != null && role != null)
                    {
                        var userRole = new IdentityUserRole<string>
                        {
                            UserId = userForRole.Id,
                            RoleId = role.Id
                        };
                        _context.UserRoles.Add(userRole);
                    }
                }

                await _context.SaveChangesAsync();

                // ICONS
                var rowsIcons = helper.GetRowsBySheetName(workbook, "Icons");

                foreach (var row in rowsIcons)
                {
                    var icon = helper.MapExcelRowToIcon(row);

                    _context.Icons.Add(icon);
                }

                _context.SaveChanges();

                // ROOT MENUs
                var rowsMenu = helper.GetRowsBySheetName(workbook, "Menu");

                foreach (var row in rowsMenu)
                {
                    var menu = helper.MapExcelRowToMenu(row);
                    Guid guid = Guid.NewGuid();
                    menu.Id = guid.ToString();

                    _context.Menu.Add(menu);
                }

                _context.SaveChanges();

                // SUB MENUs
                var rowsSubMenu = helper.GetRowsBySheetName(workbook, "SubMenu");

                foreach (var row in rowsSubMenu)
                {
                    var subMenu = helper.MapExcelRowToSubMenu(row);
                    Guid guid = Guid.NewGuid();
                    subMenu.Id = guid.ToString();

                    _context.Menu.Add(subMenu);
                }
                _context.SaveChanges();

                // MenuRights
                var rowsMenuRights = helper.GetRowsBySheetName(workbook, "MenuRights");

                foreach (var row in rowsMenuRights)
                {
                    var menuRight = helper.MapExcelRowToMenuRights(row);

                    _context.MenuRights.Add(menuRight);
                }
                _context.SaveChanges();


                // @TODO: Use transactions
                // await _context.CommitTransactionAsync();

                // DONE PROCESSING EXCEL READING
                var successMessage = "User,Roles & Menu stuff, importadas correctamente.";

                var importStats = new { };

                return Ok(new { message = successMessage, stats = importStats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

    }
}
