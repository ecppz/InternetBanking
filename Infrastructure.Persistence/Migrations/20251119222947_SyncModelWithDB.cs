using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelWithDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_OwnerUserId_BeneficiaryAccountNumber",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "Beneficiary",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "BeneficiaryAccountNumber",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "BeneficiaryUserId",
                table: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Beneficiaries",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Beneficiaries",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_OwnerUserId_DocumentNumber",
                table: "Beneficiaries",
                columns: new[] { "OwnerUserId", "DocumentNumber" },
                unique: true);
        }
    }
}
