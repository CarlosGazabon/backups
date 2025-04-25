using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeOver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Line = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supervisor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subcategory2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Failure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Objective = table.Column<int>(type: "int", nullable: true),
                    Minutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Hour_Sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeOver", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    IsChangeOver = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hour",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeTypeID = table.Column<int>(type: "int", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hour", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Icons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Disabled = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Icons__3214EC071FEBF8E9", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Line",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flow = table.Column<int>(type: "int", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Utilization = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Efficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Scrap = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BottleScrap = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "(CONVERT([bit],(0)))"),
                    CanScrap = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "(CONVERT([bit],(0)))"),
                    PreformScrap = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "(CONVERT([bit],(0)))"),
                    PouchScrap = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "(CONVERT([bit],(0)))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Line", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Parent_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Menu__3213E83F75A31B61", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Menu_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Can_view = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    Can_add = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    Can_delete = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    Can_modify = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MenuRigh__3213E83FB1DA2D1C", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MenuId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MenuId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowView = table.Column<bool>(type: "bit", nullable: true),
                    AllowInsert = table.Column<bool>(type: "bit", nullable: true),
                    AllowEdit = table.Column<bool>(type: "bit", nullable: true),
                    AllowDelete = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekDays1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekDays2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduleStarts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduleEnds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supervisor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supervisor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeSubCategory1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    DowntimeCategoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeSubCategory1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DowntimeSubCategory1_DowntimeCategory_DowntimeCategoryID",
                        column: x => x.DowntimeCategoryID,
                        principalTable: "DowntimeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NetContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Packing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flavour = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    UnitsPerPackage = table.Column<int>(type: "int", nullable: false),
                    StandardSpeed = table.Column<int>(type: "int", nullable: false),
                    LineID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Line_LineID",
                        column: x => x.LineID,
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productivity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    Production = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingProduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingMinutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DowntimeMinutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HourStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HourEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flow = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScrapUnits = table.Column<int>(type: "int", nullable: false),
                    CanScrap = table.Column<int>(type: "int", nullable: false),
                    PreformScrap = table.Column<int>(type: "int", nullable: false),
                    BottleScrap = table.Column<int>(type: "int", nullable: false),
                    PouchScrap = table.Column<int>(type: "int", nullable: false),
                    Sku2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductID2 = table.Column<int>(type: "int", nullable: true),
                    Production2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ScrapUnits2 = table.Column<int>(type: "int", nullable: false),
                    CanScrap2 = table.Column<int>(type: "int", nullable: false),
                    PreformScrap2 = table.Column<int>(type: "int", nullable: false),
                    BottleScrap2 = table.Column<int>(type: "int", nullable: false),
                    PouchScrap2 = table.Column<int>(type: "int", nullable: false),
                    StandardSpeed = table.Column<int>(type: "int", nullable: false),
                    SupervisorID = table.Column<int>(type: "int", nullable: false),
                    LineID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: false),
                    HourID = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productivity_Hour_HourID",
                        column: x => x.HourID,
                        principalTable: "Hour",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productivity_Line_LineID",
                        column: x => x.LineID,
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productivity_Shift_ShiftID",
                        column: x => x.ShiftID,
                        principalTable: "Shift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productivity_Supervisor_SupervisorID",
                        column: x => x.SupervisorID,
                        principalTable: "Supervisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeSubCategory2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    DowntimeSubCategory1ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeSubCategory2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DowntimeSubCategory2_DowntimeSubCategory1_DowntimeSubCategory1ID",
                        column: x => x.DowntimeSubCategory1ID,
                        principalTable: "DowntimeSubCategory1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductivityFlow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowIndex = table.Column<int>(type: "int", nullable: false),
                    ProductivityID = table.Column<int>(type: "int", nullable: false),
                    Minutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    Production = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ScrapUnits = table.Column<int>(type: "int", nullable: false),
                    CanScrap = table.Column<int>(type: "int", nullable: false),
                    PreformScrap = table.Column<int>(type: "int", nullable: false),
                    BottleScrap = table.Column<int>(type: "int", nullable: false),
                    PouchScrap = table.Column<int>(type: "int", nullable: false),
                    Empty = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductivityFlow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductivityFlow_Product_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductivityFlow_Productivity_ProductivityID",
                        column: x => x.ProductivityID,
                        principalTable: "Productivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DowntimeCategoryID = table.Column<int>(type: "int", nullable: false),
                    DowntimeSubCategory1ID = table.Column<int>(type: "int", nullable: false),
                    DowntimeSubCategory2ID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Failure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    ObjectiveMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DowntimeCode_DowntimeCategory_DowntimeCategoryID",
                        column: x => x.DowntimeCategoryID,
                        principalTable: "DowntimeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DowntimeCode_DowntimeSubCategory1_DowntimeSubCategory1ID",
                        column: x => x.DowntimeSubCategory1ID,
                        principalTable: "DowntimeSubCategory1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DowntimeCode_DowntimeSubCategory2_DowntimeSubCategory2ID",
                        column: x => x.DowntimeSubCategory2ID,
                        principalTable: "DowntimeSubCategory2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeReason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowIndex = table.Column<int>(type: "int", nullable: false),
                    ProductivityID = table.Column<int>(type: "int", nullable: false),
                    Minutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DowntimeCategoryId = table.Column<int>(type: "int", nullable: false),
                    DowntimeSubCategory1Id = table.Column<int>(type: "int", nullable: false),
                    DowntimeSubCategory2Id = table.Column<int>(type: "int", nullable: false),
                    DowntimeCodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DowntimeReason_DowntimeCategory_DowntimeCategoryId",
                        column: x => x.DowntimeCategoryId,
                        principalTable: "DowntimeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DowntimeReason_DowntimeCode_DowntimeCodeId",
                        column: x => x.DowntimeCodeId,
                        principalTable: "DowntimeCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DowntimeReason_DowntimeSubCategory1_DowntimeSubCategory1Id",
                        column: x => x.DowntimeSubCategory1Id,
                        principalTable: "DowntimeSubCategory1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DowntimeReason_DowntimeSubCategory2_DowntimeSubCategory2Id",
                        column: x => x.DowntimeSubCategory2Id,
                        principalTable: "DowntimeSubCategory2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DowntimeReason_Productivity_ProductivityID",
                        column: x => x.ProductivityID,
                        principalTable: "Productivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeCode_DowntimeCategoryID",
                table: "DowntimeCode",
                column: "DowntimeCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeCode_DowntimeSubCategory1ID",
                table: "DowntimeCode",
                column: "DowntimeSubCategory1ID");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeCode_DowntimeSubCategory2ID",
                table: "DowntimeCode",
                column: "DowntimeSubCategory2ID");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeReason_DowntimeCategoryId",
                table: "DowntimeReason",
                column: "DowntimeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeReason_DowntimeCodeId",
                table: "DowntimeReason",
                column: "DowntimeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeReason_DowntimeSubCategory1Id",
                table: "DowntimeReason",
                column: "DowntimeSubCategory1Id");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeReason_DowntimeSubCategory2Id",
                table: "DowntimeReason",
                column: "DowntimeSubCategory2Id");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeReason_ProductivityID",
                table: "DowntimeReason",
                column: "ProductivityID");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeSubCategory1_DowntimeCategoryID",
                table: "DowntimeSubCategory1",
                column: "DowntimeCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeSubCategory2_DowntimeSubCategory1ID",
                table: "DowntimeSubCategory2",
                column: "DowntimeSubCategory1ID");

            migrationBuilder.CreateIndex(
                name: "IX_Product_LineID",
                table: "Product",
                column: "LineID");

            migrationBuilder.CreateIndex(
                name: "IX_Productivity_HourID",
                table: "Productivity",
                column: "HourID");

            migrationBuilder.CreateIndex(
                name: "IX_Productivity_LineID",
                table: "Productivity",
                column: "LineID");

            migrationBuilder.CreateIndex(
                name: "IX_Productivity_ShiftID",
                table: "Productivity",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "IX_Productivity_SupervisorID",
                table: "Productivity",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityFlow_ProductID",
                table: "ProductivityFlow",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityFlow_ProductivityID",
                table: "ProductivityFlow",
                column: "ProductivityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChangeOver");

            migrationBuilder.DropTable(
                name: "DowntimeReason");

            migrationBuilder.DropTable(
                name: "Icons");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "MenuRights");

            migrationBuilder.DropTable(
                name: "ProductivityFlow");

            migrationBuilder.DropTable(
                name: "RoleAccess");

            migrationBuilder.DropTable(
                name: "RoleRights");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DowntimeCode");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Productivity");

            migrationBuilder.DropTable(
                name: "DowntimeSubCategory2");

            migrationBuilder.DropTable(
                name: "Hour");

            migrationBuilder.DropTable(
                name: "Line");

            migrationBuilder.DropTable(
                name: "Shift");

            migrationBuilder.DropTable(
                name: "Supervisor");

            migrationBuilder.DropTable(
                name: "DowntimeSubCategory1");

            migrationBuilder.DropTable(
                name: "DowntimeCategory");
        }
    }
}
