using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class addNewQualityIncidentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "QualityIncident",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfIncident",
                table: "QualityIncident",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfRelease",
                table: "QualityIncident",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "QualityIncident",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReleaseComments",
                table: "QualityIncident",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "QualityIncident",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_CreatedById",
                table: "QualityIncident",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIncident_UpdatedById",
                table: "QualityIncident",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_CreatedById",
                table: "QualityIncident",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_UpdatedById",
                table: "QualityIncident",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_CreatedById",
                table: "QualityIncident");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_UpdatedById",
                table: "QualityIncident");

            migrationBuilder.DropIndex(
                name: "IX_QualityIncident_CreatedById",
                table: "QualityIncident");

            migrationBuilder.DropIndex(
                name: "IX_QualityIncident_UpdatedById",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "DateOfIncident",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "DateOfRelease",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "ReleaseComments",
                table: "QualityIncident");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "QualityIncident");
        }
    }
}
