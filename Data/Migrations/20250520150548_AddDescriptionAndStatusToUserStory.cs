using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionAndStatusToUserStory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UserStories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserStories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "UserStories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserStories");
        }
    }
}
