using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashPrism.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoreRemainingExportColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractId",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContractInterval",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreditorId",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsContract",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExcludedFromDisposableIncome",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MandateReference",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ReportedBalanceInCents",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransactionKind",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ContractInterval",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreditorId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsContract",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsExcludedFromDisposableIncome",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "MandateReference",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReportedBalanceInCents",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransactionKind",
                table: "Bookings");
        }
    }
}
