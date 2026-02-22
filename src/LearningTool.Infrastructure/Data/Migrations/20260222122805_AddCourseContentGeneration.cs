using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningTool.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseContentGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContentGeneratedAt",
                table: "Courses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LearningPlan",
                table: "Courses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemPrompt",
                table: "Courses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentGeneratedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LearningPlan",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SystemPrompt",
                table: "Courses");
        }
    }
}
