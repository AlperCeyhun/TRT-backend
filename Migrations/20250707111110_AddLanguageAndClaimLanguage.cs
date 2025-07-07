using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TRT_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageAndClaimLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "tr", "Türkçe" },
                    { 2, "en", "English" },
                    { 3, "fr", "Français" }
                });

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.InsertData(
                table: "ClaimLanguages",
                columns: new[] { "Id", "ClaimId", "Description", "LanguageId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Yeni görev oluşturma izni", 1, "Görev Ekle" },
                    { 2, 2, "Görev silme izni", 1, "Görev Sil" },
                    { 3, 3, "Görev başlığını değiştirme izni", 1, "Görev Başlığını Düzenle" },
                    { 4, 4, "Görev açıklamasını değiştirme izni", 1, "Görev Açıklamasını Düzenle" },
                    { 5, 5, "Görev durumunu değiştirme izni", 1, "Görev Durumunu Düzenle" },
                    { 6, 6, "Görev atayanlarını değiştirme izni", 1, "Görev Atayanları Düzenle" },
                    { 7, 1, "Permission to create new task", 2, "Add Task" },
                    { 8, 2, "Permission to delete task", 2, "Delete Task" },
                    { 9, 3, "Permission to edit task title", 2, "Edit Task Title" },
                    { 10, 4, "Permission to edit task description", 2, "Edit Task Description" },
                    { 11, 5, "Permission to edit task status", 2, "Edit Task Status" },
                    { 12, 6, "Permission to edit task assignees", 2, "Edit Task Assignees" },
                    { 13, 1, "Permission d'ajouter une tâche", 3, "Ajouter Tâche" },
                    { 14, 2, "Permission de supprimer une tâche", 3, "Supprimer Tâche" },
                    { 15, 3, "Permission de modifier le titre de la tâche", 3, "Modifier Titre Tâche" },
                    { 16, 4, "Permission de modifier la description de la tâche", 3, "Modifier Description Tâche" },
                    { 17, 5, "Permission de modifier le statut de la tâche", 3, "Modifier Statut Tâche" },
                    { 18, 6, "Permission de modifier les assignés de la tâche", 3, "Modifier Assignés Tâche" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ClaimLanguages",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
