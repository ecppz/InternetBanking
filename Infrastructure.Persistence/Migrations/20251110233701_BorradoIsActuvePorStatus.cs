using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BorradoIsActuvePorStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SavingsAccounts");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SavingsAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SavingsAccounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SavingsAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
