using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStories_Epics_EpicId",
                table: "UserStories");

            migrationBuilder.AlterColumn<int>(
                name: "EpicId",
                table: "UserStories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Epics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Epics_ProjectId",
                table: "Epics",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Epics_Projects_ProjectId",
                table: "Epics",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStories_Epics_EpicId",
                table: "UserStories",
                column: "EpicId",
                principalTable: "Epics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Epics_Projects_ProjectId",
                table: "Epics");

            migrationBuilder.DropForeignKey(
                name: "FK_UserStories_Epics_EpicId",
                table: "UserStories");

            migrationBuilder.DropIndex(
                name: "IX_Epics_ProjectId",
                table: "Epics");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Epics");

            migrationBuilder.AlterColumn<int>(
                name: "EpicId",
                table: "UserStories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStories_Epics_EpicId",
                table: "UserStories",
                column: "EpicId",
                principalTable: "Epics",
                principalColumn: "Id");
        }
    }
}
