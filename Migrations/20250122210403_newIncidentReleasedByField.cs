using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class newIncidentReleasedByField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReleasedById",
                table: "QualityIncident",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_ReleasedById",
                table: "QualityIncident",
                column: "ReleasedById");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_ReleasedById",
                table: "QualityIncident",
                column: "ReleasedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_ReleasedById",
                table: "QualityIncident");

            migrationBuilder.DropIndex(
                name: "IX_QualityIncident_ReleasedById",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "ReleasedById",
                table: "QualityIncident");
        }
    }
}
