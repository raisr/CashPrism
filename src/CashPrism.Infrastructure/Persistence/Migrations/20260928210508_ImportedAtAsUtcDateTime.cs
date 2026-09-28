using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashPrism.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Rewrites <c>ImportRuns.ImportedAt</c> from the text a
    /// <c>DateTimeOffset</c> is stored as to the text a UTC <c>DateTime</c> is
    /// stored as, so the column can be ordered, ranged and indexed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The column type does not change — it is <c>TEXT</c> either way, which is
    /// why the generated migration was empty and this is written by hand. What
    /// changes is the content: <c>2026-09-07 18:00:00.1234567+00:00</c> becomes
    /// <c>2026-09-07 18:00:00.1234567</c>. The offset is always six characters,
    /// and dropping it is what makes the remaining text compare correctly.
    /// </para>
    /// <para>
    /// Both statements are deliberately narrow. Only the rows that actually
    /// carry <c>+00:00</c> are touched, because that is the only offset the
    /// application's clock ever wrote. A row carrying any other offset is left
    /// alone and will fail loudly when it is read, rather than being silently
    /// shifted by however many hours it was written with.
    /// </para>
    /// </remarks>
    public partial class ImportedAtAsUtcDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE ImportRuns
                SET ImportedAt = substr(ImportedAt, 1, length(ImportedAt) - 6)
                WHERE ImportedAt LIKE '%+00:00';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE ImportRuns
                SET ImportedAt = ImportedAt || '+00:00'
                WHERE ImportedAt NOT LIKE '%+00:00';
                """);
        }
    }
}
