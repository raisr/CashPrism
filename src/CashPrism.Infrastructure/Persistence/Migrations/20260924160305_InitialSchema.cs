using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashPrism.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Fingerprint = table.Column<string>(type: "TEXT", nullable: false),
                    BookedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AmountInCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    AccountReference = table.Column<string>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    Counterparty = table.Column<string>(type: "TEXT", nullable: false),
                    CounterpartyAccount = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentReference = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    SubCategory = table.Column<string>(type: "TEXT", nullable: false),
                    IsTransfer = table.Column<bool>(type: "INTEGER", nullable: false),
                    SplitRole = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFingerprint = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Fingerprint);
                });

            migrationBuilder.CreateTable(
                name: "ImportRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", nullable: false),
                    SheetName = table.Column<string>(type: "TEXT", nullable: false),
                    ExportedOn = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ImportedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawRows",
                columns: table => new
                {
                    ImportRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Fingerprint = table.Column<string>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawRows", x => new { x.ImportRunId, x.Fingerprint });
                    table.ForeignKey(
                        name: "FK_RawRows_ImportRuns_ImportRunId",
                        column: x => x.ImportRunId,
                        principalTable: "ImportRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportRuns_FileHash",
                table: "ImportRuns",
                column: "FileHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "RawRows");

            migrationBuilder.DropTable(
                name: "ImportRuns");
        }
    }
}
