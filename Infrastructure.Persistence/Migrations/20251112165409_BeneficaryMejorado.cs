using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BeneficaryMejorado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_OwnerUserId_DocumentNumber",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Beneficiaries",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryAccountNumber",
                table: "Beneficiaries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BeneficiaryUserId",
                table: "Beneficiaries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_OwnerUserId_BeneficiaryAccountNumber",
                table: "Beneficiaries",
                columns: new[] { "OwnerUserId", "BeneficiaryAccountNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_OwnerUserId_BeneficiaryAccountNumber",
                table: "Beneficiaries");

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
