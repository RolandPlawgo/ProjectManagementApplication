using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedProjectsFirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserStories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserStories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserStories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserStories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Epics",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Epics",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Description", "Name", "SprintDuration" },
                values: new object[] { 100, "Sample project added by deafult", "Sample Project", 2 });

            migrationBuilder.InsertData(
                table: "Epics",
                columns: new[] { "Id", "ProjectId", "Title" },
                values: new object[,]
                {
                    { 1, 100, "Epic: Authentication" },
                    { 2, 100, "Epic: Authorization" }
                });

            migrationBuilder.InsertData(
                table: "UserStories",
                columns: new[] { "Id", "Description", "EpicId", "SprintId", "Status", "Title" },
                values: new object[,]
                {
                    { 1, "Allow users to log in", 1, null, 0, "Login page" },
                    { 2, "Allow user registration", 1, null, 0, "Registration" },
                    { 3, "CRUD roles", 2, null, 0, "Role management" },
                    { 4, "Implement claim checks", 2, null, 0, "Claim-based auth" }
                });
        }
    }
}
