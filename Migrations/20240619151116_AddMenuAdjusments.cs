using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuAdjusments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MenuRights_Menu_id",
                table: "MenuRights",
                column: "Menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRights_Role_id",
                table: "MenuRights",
                column: "Role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_IconId",
                table: "Menu",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Parent_id",
                table: "Menu",
                column: "Parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Menu_Icons_IconId",
                table: "Menu",
                column: "IconId",
                principalTable: "Icons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Menu_Menu_Parent_id",
                table: "Menu",
                column: "Parent_id",
                principalTable: "Menu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuRights_AspNetRoles_Role_id",
                table: "MenuRights",
                column: "Role_id",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuRights_Menu_Menu_id",
                table: "MenuRights",
                column: "Menu_id",
                principalTable: "Menu",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menu_Icons_IconId",
                table: "Menu");

            migrationBuilder.DropForeignKey(
                name: "FK_Menu_Menu_Parent_id",
                table: "Menu");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuRights_AspNetRoles_Role_id",
                table: "MenuRights");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuRights_Menu_Menu_id",
                table: "MenuRights");

            migrationBuilder.DropIndex(
                name: "IX_MenuRights_Menu_id",
                table: "MenuRights");

            migrationBuilder.DropIndex(
                name: "IX_MenuRights_Role_id",
                table: "MenuRights");

            migrationBuilder.DropIndex(
                name: "IX_Menu_IconId",
                table: "Menu");

            migrationBuilder.DropIndex(
                name: "IX_Menu_Parent_id",
                table: "Menu");
        }
    }
}
