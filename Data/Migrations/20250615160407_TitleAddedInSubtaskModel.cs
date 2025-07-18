﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class TitleAddedInSubtaskModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Subtasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Subtasks");
        }
    }
}
