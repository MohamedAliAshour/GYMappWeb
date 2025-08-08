using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYMappWeb.Migrations
{
    /// <inheritdoc />
    public partial class seedrolesmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] {"Id","Name","NormalizedName","ConcurrencyStamp"},
                values: new object[] {Guid.NewGuid().ToString(),"Captain", "CAPTAIN".ToUpper(),Guid.NewGuid().ToString()}
                );

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[] { Guid.NewGuid().ToString(), "Developer", "DEVELOPER".ToUpper(), Guid.NewGuid().ToString() }
                );

            migrationBuilder.InsertData(
               table: "AspNetRoles",
               columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
               values: new object[] { Guid.NewGuid().ToString(), "User", "USER".ToUpper(), Guid.NewGuid().ToString() }
               );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
               table: "AspNetRoles",
               keyColumn: "Name",
               keyValue: "Captain"
           );

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Name",
                keyValue: "Developer"
            );

            migrationBuilder.DeleteData(
               table: "AspNetRoles",
               keyColumn: "Name",
               keyValue: "User"
           );
        }
    }
}
