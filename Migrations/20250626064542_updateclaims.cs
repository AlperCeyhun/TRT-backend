using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRT_backend.Migrations
{
    /// <inheritdoc />
    public partial class updateclaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 7,
                column: "ClaimName",
                value: "Edit User's Claim");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 7,
                column: "ClaimName",
                value: "Add Claim to User");

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "ClaimName" },
                values: new object[] { 2, "Delete User" });

            migrationBuilder.InsertData(
                table: "RoleClaims",
                columns: new[] { "Id", "ClaimId", "RoleId" },
                values: new object[] { 2, 2, 1 });
        }
    }
}
