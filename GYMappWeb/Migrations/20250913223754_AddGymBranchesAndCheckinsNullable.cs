using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYMappWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddGymBranchesAndCheckinsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GymBranch_ID",
                table: "tbl_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GymBranch_ID",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GymBranches",
                columns: table => new
                {
                    GymBranch_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GymName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymBranches", x => x.GymBranch_ID);
                });

            migrationBuilder.CreateTable(
                name: "Checkins",
                columns: table => new
                {
                    Checkin_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckinDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    GymBranch_ID = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkins", x => x.Checkin_ID);
                    table.ForeignKey(
                        name: "FK_Checkins_GymBranches",
                        column: x => x.GymBranch_ID,
                        principalTable: "GymBranches",
                        principalColumn: "GymBranch_ID");
                    table.ForeignKey(
                        name: "FK_Checkins_tbl_Users",
                        column: x => x.User_ID,
                        principalTable: "tbl_Users",
                        principalColumn: "User_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Users_GymBranch_ID",
                table: "tbl_Users",
                column: "GymBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GymBranch_ID",
                table: "AspNetUsers",
                column: "GymBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Checkins_GymBranch_ID",
                table: "Checkins",
                column: "GymBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Checkins_User_ID",
                table: "Checkins",
                column: "User_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_GymBranches",
                table: "AspNetUsers",
                column: "GymBranch_ID",
                principalTable: "GymBranches",
                principalColumn: "GymBranch_ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Users_GymBranches",
                table: "tbl_Users",
                column: "GymBranch_ID",
                principalTable: "GymBranches",
                principalColumn: "GymBranch_ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_GymBranches",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Users_GymBranches",
                table: "tbl_Users");

            migrationBuilder.DropTable(
                name: "Checkins");

            migrationBuilder.DropTable(
                name: "GymBranches");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Users_GymBranch_ID",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GymBranch_ID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GymBranch_ID",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "GymBranch_ID",
                table: "AspNetUsers");
        }
    }
}
