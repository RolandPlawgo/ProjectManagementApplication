using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssignedUserAddedToSubtask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Subtasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subtasks_AssignedUserId",
                table: "Subtasks",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_AspNetUsers_AssignedUserId",
                table: "Subtasks",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_AspNetUsers_AssignedUserId",
                table: "Subtasks");

            migrationBuilder.DropIndex(
                name: "IX_Subtasks_AssignedUserId",
                table: "Subtasks");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Subtasks");
        }
    }
}
