using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResultFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswersCount",
                table: "TestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "TestResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Grade",
                table: "TestResults",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestionsCount",
                table: "TestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedAnswer",
                table: "TestAnswerResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackNotes",
                table: "TestAnswerResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "KeywordMatchPercentage",
                table: "TestAnswerResults",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectAnswersCount",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "TotalQuestionsCount",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "FeedbackNotes",
                table: "TestAnswerResults");

            migrationBuilder.DropColumn(
                name: "KeywordMatchPercentage",
                table: "TestAnswerResults");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "TestResults",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedAnswer",
                table: "TestAnswerResults",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
