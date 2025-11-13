using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewPropertyToCreditCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CreditCards");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "CreditCards",
                type: "datetime2",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CreditCards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CreditCards");

            migrationBuilder.AlterColumn<string>(
                name: "ExpirationDate",
                table: "CreditCards",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CreditCards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
