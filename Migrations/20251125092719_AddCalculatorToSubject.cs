using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdamsScienceHub.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculatorToSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowCalculator",
                table: "Subjects",
                newName: "CalculatorEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CalculatorEnabled",
                table: "Subjects",
                newName: "AllowCalculator");
        }
    }
}
