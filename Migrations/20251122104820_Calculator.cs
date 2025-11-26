using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdamsScienceHub.Migrations
{
    /// <inheritdoc />
    public partial class Calculator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCalculator",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCalculator",
                table: "Subjects");
        }
    }
}
