using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class addIncidentBatchNumberAndExpiryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "QualityIncident",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpiryCode",
                table: "QualityIncident",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "ExpiryCode",
                table: "QualityIncident");
        }
    }
}
