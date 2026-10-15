using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashPrism.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountAssignments",
                columns: table => new
                {
                    AccountReference = table.Column<string>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAssignments", x => x.AccountReference);
                    table.ForeignKey(
                        name: "FK_AccountAssignments_AccountGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "AccountGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAssignments_GroupId",
                table: "AccountAssignments",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAssignments");

            migrationBuilder.DropTable(
                name: "AccountGroups");
        }
    }
}
