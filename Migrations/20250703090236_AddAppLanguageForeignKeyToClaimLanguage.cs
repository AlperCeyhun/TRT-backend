using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRT_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAppLanguageForeignKeyToClaimLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "ClaimLanguages");

            migrationBuilder.AddColumn<int>(
                name: "AppLanguageId",
                table: "ClaimLanguages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimLanguages_AppLanguageId",
                table: "ClaimLanguages",
                column: "AppLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimLanguages_AppLanguages_AppLanguageId",
                table: "ClaimLanguages",
                column: "AppLanguageId",
                principalTable: "AppLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimLanguages_AppLanguages_AppLanguageId",
                table: "ClaimLanguages");

            migrationBuilder.DropIndex(
                name: "IX_ClaimLanguages_AppLanguageId",
                table: "ClaimLanguages");

            migrationBuilder.DropColumn(
                name: "AppLanguageId",
                table: "ClaimLanguages");

            migrationBuilder.AddColumn<string>(
                name: "LanguageId",
                table: "ClaimLanguages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
