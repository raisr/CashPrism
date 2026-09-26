using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashPrism.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImportCountsAndBookingSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingsInserted",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BookingsUnchanged",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BookingsUpdated",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RowsRead",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceImportRunId",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SourceImportRunId",
                table: "Bookings",
                column: "SourceImportRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_SourceImportRunId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingsInserted",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "BookingsUnchanged",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "BookingsUpdated",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "RowsRead",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "SourceImportRunId",
                table: "Bookings");
        }
    }
}
