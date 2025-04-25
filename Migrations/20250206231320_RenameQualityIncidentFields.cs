using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class RenameQualityIncidentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_CreatedById",
                table: "QualityIncident");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_UpdatedById",
                table: "QualityIncident");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "QualityIncident",
                newName: "AuditUpdatedById");

            migrationBuilder.RenameColumn(
                name: "ExpiryCode",
                table: "QualityIncident",
                newName: "ExpirationCode");

            migrationBuilder.RenameColumn(
                name: "DateUpdated",
                table: "QualityIncident",
                newName: "AuditDateUpdated");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "QualityIncident",
                newName: "AuditDateCreated");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "QualityIncident",
                newName: "AuditCreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_QualityIncident_UpdatedById",
                table: "QualityIncident",
                newName: "IX_QualityIncident_AuditUpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_QualityIncident_CreatedById",
                table: "QualityIncident",
                newName: "IX_QualityIncident_AuditCreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_AuditCreatedById",
                table: "QualityIncident",
                column: "AuditCreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_AuditUpdatedById",
                table: "QualityIncident",
                column: "AuditUpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_AuditCreatedById",
                table: "QualityIncident");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityIncident_AspNetUsers_AuditUpdatedById",
                table: "QualityIncident");

            migrationBuilder.RenameColumn(
                name: "ExpirationCode",
                table: "QualityIncident",
                newName: "ExpiryCode");

            migrationBuilder.RenameColumn(
                name: "AuditUpdatedById",
                table: "QualityIncident",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "AuditDateUpdated",
                table: "QualityIncident",
                newName: "DateUpdated");

            migrationBuilder.RenameColumn(
                name: "AuditDateCreated",
                table: "QualityIncident",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "AuditCreatedById",
                table: "QualityIncident",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_QualityIncident_AuditUpdatedById",
                table: "QualityIncident",
                newName: "IX_QualityIncident_UpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_QualityIncident_AuditCreatedById",
                table: "QualityIncident",
                newName: "IX_QualityIncident_CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_CreatedById",
                table: "QualityIncident",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityIncident_AspNetUsers_UpdatedById",
                table: "QualityIncident",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
