using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class adjustQualityIncidentCommentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReleaseComments",
                table: "QualityIncident",
                type: "nvarchar(270)",
                maxLength: 270,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PotencialRootCause",
                table: "QualityIncident",
                type: "nvarchar(270)",
                maxLength: 270,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "QualityIncident",
                type: "nvarchar(270)",
                maxLength: 270,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionComments",
                table: "QualityIncident",
                type: "nvarchar(270)",
                maxLength: 270,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReleaseComments",
                table: "QualityIncident",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(270)",
                oldMaxLength: 270,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PotencialRootCause",
                table: "QualityIncident",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(270)",
                oldMaxLength: 270);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "QualityIncident",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(270)",
                oldMaxLength: 270,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionComments",
                table: "QualityIncident",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(270)",
                oldMaxLength: 270);
        }
    }
}
