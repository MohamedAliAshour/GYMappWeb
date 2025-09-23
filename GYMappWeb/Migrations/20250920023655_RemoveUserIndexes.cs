using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYMappWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_UserCode",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_UserName",
                table: "tbl_Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserCode",
                table: "tbl_Users",
                column: "UserCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserName",
                table: "tbl_Users",
                column: "UserName",
                unique: true);
        }
    }
}
