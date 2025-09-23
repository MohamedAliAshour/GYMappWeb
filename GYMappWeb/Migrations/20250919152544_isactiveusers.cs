using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYMappWeb.Migrations
{
    /// <inheritdoc />
    public partial class isactiveusers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GymBranch_ID",
                table: "tbl_UserMemberShip",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GymBranch_ID",
                table: "tbl_Offers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GymBranch_ID",
                table: "tbl_MembershipTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GymBranch_ID",
                table: "tbl_MemberShipFreeze",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GymBranches",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserMemberShip_GymBranch_ID",
                table: "tbl_UserMemberShip",
                column: "GymBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Offers_GymBranch_ID",
                table: "tbl_Offers",
                column: "GymBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MembershipTypes_GymBranch_ID",
                table: "tbl_MembershipTypes",
                column: "GymBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MemberShipFreeze_GymBranch_ID",
                table: "tbl_MemberShipFreeze",
                column: "GymBranch_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_MemberShipFreeze_GymBranches",
                table: "tbl_MemberShipFreeze",
                column: "GymBranch_ID",
                principalTable: "GymBranches",
                principalColumn: "GymBranch_ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_MembershipTypes_GymBranches",
                table: "tbl_MembershipTypes",
                column: "GymBranch_ID",
                principalTable: "GymBranches",
                principalColumn: "GymBranch_ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Offers_GymBranches",
                table: "tbl_Offers",
                column: "GymBranch_ID",
                principalTable: "GymBranches",
                principalColumn: "GymBranch_ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_UserMemberShip_GymBranches",
                table: "tbl_UserMemberShip",
                column: "GymBranch_ID",
                principalTable: "GymBranches",
                principalColumn: "GymBranch_ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_MemberShipFreeze_GymBranches",
                table: "tbl_MemberShipFreeze");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_MembershipTypes_GymBranches",
                table: "tbl_MembershipTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Offers_GymBranches",
                table: "tbl_Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_UserMemberShip_GymBranches",
                table: "tbl_UserMemberShip");

            migrationBuilder.DropIndex(
                name: "IX_tbl_UserMemberShip_GymBranch_ID",
                table: "tbl_UserMemberShip");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Offers_GymBranch_ID",
                table: "tbl_Offers");

            migrationBuilder.DropIndex(
                name: "IX_tbl_MembershipTypes_GymBranch_ID",
                table: "tbl_MembershipTypes");

            migrationBuilder.DropIndex(
                name: "IX_tbl_MemberShipFreeze_GymBranch_ID",
                table: "tbl_MemberShipFreeze");

            migrationBuilder.DropColumn(
                name: "GymBranch_ID",
                table: "tbl_UserMemberShip");

            migrationBuilder.DropColumn(
                name: "GymBranch_ID",
                table: "tbl_Offers");

            migrationBuilder.DropColumn(
                name: "GymBranch_ID",
                table: "tbl_MembershipTypes");

            migrationBuilder.DropColumn(
                name: "GymBranch_ID",
                table: "tbl_MemberShipFreeze");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GymBranches");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
