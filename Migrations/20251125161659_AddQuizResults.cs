using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdamsScienceHub.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: false),
                    DateTaken = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalQuestions = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "INTEGER", nullable: false),
                    WrongAnswers = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeSpent = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizResults_UserId",
                table: "QuizResults",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizResults");
        }
    }
}
