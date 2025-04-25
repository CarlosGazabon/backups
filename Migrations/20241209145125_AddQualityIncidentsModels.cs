using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityIncidentsModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityIncidentReason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Disable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityIncidentReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityIncident",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncidentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ActionComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PotencialRootCause = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Released = table.Column<bool>(type: "bit", nullable: false),
                    ReleasedForConsumption = table.Column<int>(type: "int", nullable: false),
                    ReleasedForDonation = table.Column<int>(type: "int", nullable: false),
                    ReleasedForDestruction = table.Column<int>(type: "int", nullable: false),
                    ReleasedForOther = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    QualityIncidentReasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityIncident", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityIncident_Line_LineId",
                        column: x => x.LineId,
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_QualityIncident_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_QualityIncident_QualityIncidentReason_QualityIncidentReasonId",
                        column: x => x.QualityIncidentReasonId,
                        principalTable: "QualityIncidentReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_QualityIncident_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_LineId",
                table: "QualityIncident",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_ProductId",
                table: "QualityIncident",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_QualityIncidentReasonId",
                table: "QualityIncident",
                column: "QualityIncidentReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_ShiftId",
                table: "QualityIncident",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityIncident");

            migrationBuilder.DropTable(
                name: "QualityIncidentReason");
        }
    }
}
