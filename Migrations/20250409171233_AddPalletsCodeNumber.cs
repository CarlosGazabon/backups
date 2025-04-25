using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class AddPalletsCodeNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PalletsCodeNumber",
                table: "QualityIncident",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PalletsCodeNumber",
                table: "QualityIncident");
        }
    }
}
