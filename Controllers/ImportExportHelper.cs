using ClosedXML.Excel;
using Hangfire.Dashboard;
using Inventio.Data;
using Inventio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Inventio.Controllers
{

    public class ImportExportHelper(ApplicationDBContext context)
    {
        public readonly ApplicationDBContext _context = context;

        public async Task<DataTable> GetLinesDataTable()
        {
            var lines = await _context.Line.ToListAsync();

            DataTable dt = new("Lines");
            dt.Columns.AddRange(new DataColumn[] {
                new("Id"),
                new("Name"),
                new("Inactive")
            });

            foreach (var line in lines)
            {
                dt.Rows.Add(
                    line.Id,
                    line.Name,
                    line.Inactive ? "Yes" : "No"
                );
            }
            return dt;
        }

        public async Task<DataTable> GetProductsDataTable()
        {
            var products = await _context.Product.ToListAsync();

            DataTable dt = new("Products");
            dt.Columns.AddRange(new DataColumn[] {
                new("Id"),
                new("Name"),
                new("Price"),
                new("Inactive")
            });

            foreach (var product in products)
            {
                dt.Rows.Add(
                    product.Id,
                    product.Flavour,
                    product.Sku,
                    product.Inactive ? "Yes" : "No"
                );
            }
            return dt;
        }

        public async Task<DataTable> GetShiftstDataTable()
        {
            var shifts = await _context.Shift.ToListAsync();

            DataTable dt = new("Shifts");
            dt.Columns.AddRange(new DataColumn[] {
                new("Id"),
                new("Name"),
                new("Inactive")
            });

            foreach (var shift in shifts)
            {
                dt.Rows.Add(
                    shift.Id,
                    shift.Name,
                    shift.Inactive ? "Yes" : "No"
                );
            }
            return dt;
        }

        public Line MapExcelRowToLine(IXLRow row)
        {
            // 01 - Name
            // 02 - Flow
            // 03 - Inactive
            // 04 - Color
            // 05 - Utilization
            // 06 - Efficiency
            // 07 - BottleScrap
            // 08 - CanScrap
            // 09 - PreformScrap
            // 10 - Scrap
            // 11 - PouchScrap

            var line = new Line();

            line.Name = row.Cell(1).Value.ToString();
            line.Flow = int.Parse(row.Cell(2).Value.ToString());
            line.Inactive = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower());
            line.Color = row.Cell(4).Value.ToString();
            line.Utilization = Decimal.Parse(row.Cell(5).Value.ToString());
            line.Efficiency = Decimal.Parse(row.Cell(6).Value.ToString());
            line.BottleScrap = ConvertToBoolean(row.Cell(7).Value.ToString().ToLower());
            line.CanScrap = ConvertToBoolean(row.Cell(8).Value.ToString().ToLower());
            line.PreformScrap = ConvertToBoolean(row.Cell(9).Value.ToString().ToLower());
            line.Scrap = Decimal.Parse(row.Cell(10).Value.ToString());
            line.PouchScrap = ConvertToBoolean(row.Cell(11).Value.ToString().ToLower());

            return line;
        }

        public bool ConvertToBoolean(string value)
        {
            if (value == null)
            {
                return false;
            }

            if (value == "true" || value == "1")
            {
                return true;
            }

            if (value == "false" || value == "0")
            {
                return false;
            }

            return false;
            // return value.ToLower() == "true";
        }

        public Product MapExcelRowToProduct(IXLRow row)
        {
            // PRODUCTS                
            // 01 Sku
            // 02 NetContent
            // 03 Packing
            // 04 Flavour
            // 05 Inactive
            // 06 UnitsPerPackage
            // 07 StandardSpeed
            // 08 Line

            var lines = _context.Line.ToList();

            var lineName = row.Cell(8).Value.ToString();
            var line = lines.First(l => l.Name == lineName);

            var product = new Product();

            product.Sku = row.Cell(1).Value.ToString();
            product.NetContent = row.Cell(2).Value.ToString();
            product.Packing = row.Cell(3).Value.ToString();
            product.Flavour = row.Cell(4).Value.ToString();
            product.Inactive = ConvertToBoolean(row.Cell(5).Value.ToString().ToLower());
            product.UnitsPerPackage = int.Parse(row.Cell(6).Value.ToString());
            product.StandardSpeed = int.Parse(row.Cell(7).Value.ToString());
            product.LineID = line.Id;

            return product;
        }

        public Shift MapExcelRowToShift(IXLRow row)
        {
            // SHIFTS
            // 01 Name
            // 02 WeekDays1
            // 03 WeekDays2
            // 04 Schedule
            // 05 ScheduleStarts
            // 06 ScheduleEnds
            // 07 Inactive

            var shift = new Shift();

            shift.Name = row.Cell(1).Value.ToString();
            shift.WeekDays1 = row.Cell(2).Value.ToString();
            shift.WeekDays2 = row.Cell(3).Value.ToString();
            shift.Schedule = row.Cell(4).Value.ToString();
            shift.ScheduleStarts = row.Cell(5).Value.ToString();
            shift.ScheduleEnds = row.Cell(6).Value.ToString();
            shift.Inactive = ConvertToBoolean(row.Cell(7).Value.ToString().ToLower());

            return shift;

        }

        public FeatureFlags MapExcelRowToFeatureFlags(IXLRow row)
        {
            // 01 Code
            // 02 Name
            // 03 Description
            // 04 Inactive

            var featureFlags = new FeatureFlags();

            featureFlags.Code = row.Cell(1).Value.ToString().ToLower();
            featureFlags.Name = row.Cell(2).Value.ToString();
            featureFlags.Description = row.Cell(3).Value.ToString();
            featureFlags.Inactive = ConvertToBoolean(row.Cell(4).Value.ToString().ToLower());

            return featureFlags;

        }

        public Hour MapExcelRowToHour(IXLRow row)
        {
            // 01 Time
            // 02 TimeTypeID
            // 03 Sort

            var hour = new Hour
            {
                Time = row.Cell(1).Value.ToString(),
                TimeTypeID = int.Parse(row.Cell(2).Value.ToString()),
                Sort = int.Parse(row.Cell(3).Value.ToString())
            };

            return hour;
        }

        public Icons MapExcelRowToIcon(IXLRow row)
        {
            // 01 Name
            // 02 Disabled

            var icons = new Icons
            {
                Name = row.Cell(1).Value.ToString(),
                Disabled = ConvertToBoolean(row.Cell(2).Value.ToString().ToLower())
            };

            return icons;
        }


        // public struct MenuImportDTO()
        // {
        //     public required string Label { get; set; }
        //     public required string Route { get; set; }
        //     public required string Icon { get; set; }
        //     public int Sort { get; set; }

        // }

        public Menu MapExcelRowToSubMenu(IXLRow row)
        {
            // 01 - Label
            // 02 - Route
            // 03 - Sort
            // 04 - Parent            

            var parentName = row.Cell(4).Value.ToString();
            var parent = _context.Menu.Where(m => m.Label == parentName).FirstOrDefault();

            var menu = new Menu
            {
                Label = row.Cell(1).Value.ToString(),
                Route = row.Cell(2).Value.ToString(),
                Sort = int.Parse(row.Cell(3).Value.ToString()),
                Parent = parent
            };

            return menu;
        }

        public Menu MapExcelRowToMenu(IXLRow row)
        {
            // 01 - Label
            // 02 - Route
            // 03 - Icon
            // 04 - Sort

            var iconName = row.Cell(3).Value.ToString();
            var icon = _context.Icons.Where(i => i.Name == iconName).FirstOrDefault();

            var menu = new Menu
            {
                Label = row.Cell(1).Value.ToString(),
                Route = row.Cell(2).Value.ToString(),
                Icon = icon,
                Sort = int.Parse(row.Cell(4).Value.ToString())
            };

            return menu;
        }

        // MenuRights
        public MenuRights MapExcelRowToMenuRights(IXLRow row)
        {
            // 01 - Menu
            // 02 - Rol
            // 03 - Can_view
            // 04 - Can_add
            // 05 - Can_delete
            // 06 - Can_modify

            var MenuName = row.Cell(1).Value.ToString();
            var RoleName = row.Cell(2).Value.ToString();
            var menu = _context.Menu.Where(m => m.Label == MenuName).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Name == RoleName).FirstOrDefault();

            var menuRight = new MenuRights
            {
                Menu = menu,
                Role = role,
                CanView = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower()),
                CanAdd = ConvertToBoolean(row.Cell(4).Value.ToString().ToLower()),
                CanDelete = ConvertToBoolean(row.Cell(5).Value.ToString().ToLower()),
                CanModify = ConvertToBoolean(row.Cell(6).Value.ToString().ToLower())
            };

            return menuRight;
        }

        public Role MapExcelRowToRole(IXLRow row)
        {
            // 01 Name

            var roles = new Role
            {
                Name = row.Cell(1).Value.ToString(),
            };

            return roles;
        }

        public struct UserImportDTO
        {
            public string Name { get; set; }
            public string LastName { get; set; }

            public string UserName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Password { get; set; }
            public string Rol { get; set; }

        }

        public UserImportDTO MapExcelRowToUser(IXLRow row)
        {
            // 01 Name
            // 02 LastName
            // 03 UserName
            // 04 Email
            // 05 PhoneNumber
            // 06 Password
            // 07 Rol

            var users = new UserImportDTO
            {
                Name = row.Cell(1).Value.ToString(),
                LastName = row.Cell(2).Value.ToString(),
                UserName = row.Cell(3).Value.ToString(),
                Email = row.Cell(4).Value.ToString(),
                PhoneNumber = row.Cell(5).Value.ToString(),
                Password = row.Cell(6).Value.ToString(),
                Rol = row.Cell(7).Value.ToString()
            };

            return users;
        }

        public QualityIncidentReason MapExcelRowToQualityIncidentReason(IXLRow row)
        {
            // 01 Name
            // 02 Disabled

            var reason = new QualityIncidentReason
            {
                Description = row.Cell(1).Value.ToString(),
                Disable = ConvertToBoolean(row.Cell(2).Value.ToString().ToLower())
            };

            return reason;
        }

        public Supervisor MapExcelRowToSupervisor(IXLRow row)
        {
            // 01 Name
            // 02 Description
            // 03 Inactive

            var reason = new Supervisor
            {
                Name = row.Cell(1).Value.ToString(),
                Description = row.Cell(2).Value.ToString(),
                Inactive = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower())
            };

            return reason;
        }

        public DowntimeCategory MapExcelRowToDowntimeCategory(IXLRow row)
        {
            // 01 Name
            // 02 Inactive
            // 03 IsChangeOver

            var downtimeCategory = new DowntimeCategory
            {
                Name = row.Cell(1).Value.ToString(),
                Inactive = ConvertToBoolean(row.Cell(2).Value.ToString().ToLower()),
                IsChangeOver = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower())
            };

            return downtimeCategory;
        }
        public DowntimeSubCategory1 MapExcelRowToDowntimeSubCategory1(IXLRow row)
        {
            // 01 Name
            // 02 DowntimeCategory
            // 03 Inactive

            var downtimeCategories = _context.DowntimeCategory.ToList();
            string parentCategoryName = row.Cell(2).Value.ToString();

            var downtimeCategory = downtimeCategories.First(c => c.Name == parentCategoryName);

            var downtimeSubCategory1 = new DowntimeSubCategory1
            {
                Name = row.Cell(1).Value.ToString(),
                DowntimeCategoryID = downtimeCategory.Id,
                Inactive = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower()),

            };

            return downtimeSubCategory1;
        }
        public DowntimeSubCategory2 MapExcelRowToDowntimeSubCategory2(IXLRow row)
        {
            // 01 Name
            // 02 DowntimeSubCategory1
            // 03 Inactive

            var downtimeSubCategory1s = _context.DowntimeSubCategory1.ToList();
            var downtimeSubCategory1Name = row.Cell(2).Value.ToString();

            var downtimeSubCategory1 = downtimeSubCategory1s.First(c => c.Name == downtimeSubCategory1Name);

            var downtimeSubCategory2 = new DowntimeSubCategory2
            {
                Name = row.Cell(1).Value.ToString(),
                DowntimeSubCategory1ID = downtimeSubCategory1.Id,
                Inactive = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower()),
            };

            return downtimeSubCategory2;
        }

        public DowntimeCode MapExcelRowToDowntimeCode(IXLRow row)
        {
            // 01 Code
            // 02 Failure
            // 03 Inactive
            // 04 ObjectiveMinutes
            // 05 DowntimeCategory
            // 06 DowntimeSubCategory1
            // 07 DowntimeSubCategory2

            string categoryName = row.Cell(5).Value.ToString();
            string subCategoryName1 = row.Cell(6).Value.ToString();
            string subCategoryName2 = row.Cell(7).Value.ToString();

            var downtimeCategories = _context.DowntimeCategory.ToList();

            var downtimeCategory = downtimeCategories.First(c => c.Name == categoryName);
            var downtimeSubCategory1s = _context.DowntimeSubCategory1.Where(s => s.DowntimeCategoryID == downtimeCategory.Id).ToList();
            var downtimeSubCategory1 = downtimeSubCategory1s.First(c => c.Name == subCategoryName1);

            var downtimeSubCategory2s = _context.DowntimeSubCategory2.Where(s => s.DowntimeSubCategory1ID == downtimeSubCategory1.Id).ToList();
            var downtimeSubCategory2 = downtimeSubCategory2s.First(c => c.Name == subCategoryName2);

            var downtimeCode = new DowntimeCode
            {
                Code = row.Cell(1).Value.ToString(),
                Failure = row.Cell(2).Value.ToString(),
                Inactive = ConvertToBoolean(row.Cell(3).Value.ToString().ToLower()),
                ObjectiveMinutes = int.Parse(row.Cell(4).Value.ToString()),
                DowntimeCategoryID = downtimeCategory.Id,
                DowntimeSubCategory1ID = downtimeSubCategory1.Id,
                DowntimeSubCategory2ID = downtimeSubCategory2.Id
            };

            return downtimeCode;

        }


        public IEnumerable<IXLRow> GetRowsBySheetName(XLWorkbook workbook, string sheetName)
        {
            var worksheetLines = workbook.Worksheet(sheetName);

            if (worksheetLines == null)
            {
                // return BadRequest("La hoja 'Lines' no se encontró en el archivo.");
                return [];
            }

            var rows = worksheetLines.RowsUsed().Skip(1); // Skip header row
            return rows;
        }


    }
}
