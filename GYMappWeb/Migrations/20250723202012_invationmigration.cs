using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYMappWeb.Migrations
{
    /// <inheritdoc />
    public partial class invationmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles_ID",
                table: "tbl_Users");

            migrationBuilder.AddColumn<int>(
                name: "invitationUsed",
                table: "tbl_UserMemberShip",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "invitationCount",
                table: "tbl_MembershipTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invitationUsed",
                table: "tbl_UserMemberShip");

            migrationBuilder.DropColumn(
                name: "invitationCount",
                table: "tbl_MembershipTypes");

            migrationBuilder.AddColumn<int>(
                name: "Roles_ID",
                table: "tbl_Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
