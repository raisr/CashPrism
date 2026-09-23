using CashPrism.Domain.Imports;

namespace CashPrism.Domain.Tests.Unit.Imports;

public sealed class ImportRunTests
{
    private static readonly DateTimeOffset AnImportTime = new(2026, 3, 14, 18, 0, 0, TimeSpan.Zero);

    private static ImportRun Create(
        DateOnly? exportedOn,
        DateTimeOffset? importedAt = null,
        string fileHash = "7b5c8f",
        Guid? id = null)
        => new(
            id ?? Guid.NewGuid(),
            fileName: "20260314_Export_Alle_Buchungen.xlsx",
            fileHash,
            sheetName: "20260314_Export_Alle_Buchungen",
            exportedOn,
            importedAt ?? AnImportTime);

    public sealed class Constructor
    {
        [Fact]
        public void Refuses_A_Blank_File_Hash()
            => Assert.Throws<ArgumentException>(() => Create(exportedOn: null, fileHash: " "));

        [Fact]
        public void Refuses_A_Run_Without_An_Identity()
            => Assert.Throws<ArgumentException>(() => Create(exportedOn: null, id: Guid.Empty));

        [Fact]
        public void Accepts_A_Sheet_Name_That_Carries_No_Readable_Export_Date()
        {
            var run = Create(exportedOn: null);

            Assert.Null(run.ExportedOn);
        }
    }

    public sealed class IsLaterThan
    {
        [Fact]
        public void Says_Yes_When_The_Export_Was_Taken_Later()
        {
            var earlier = Create(exportedOn: new DateOnly(2026, 3, 13));
            var later = Create(exportedOn: new DateOnly(2026, 3, 14));

            Assert.True(later.IsLaterThan(earlier));
        }

        [Fact]
        public void Says_No_When_The_Export_Was_Taken_Earlier()
        {
            var earlier = Create(exportedOn: new DateOnly(2026, 3, 13));
            var later = Create(exportedOn: new DateOnly(2026, 3, 14));

            Assert.False(earlier.IsLaterThan(later));
        }

        [Fact]
        public void Says_No_When_Both_Exports_Were_Taken_On_The_Same_Day()
        {
            var one = Create(exportedOn: new DateOnly(2026, 3, 14));
            var another = Create(exportedOn: new DateOnly(2026, 3, 14));

            Assert.False(one.IsLaterThan(another));
        }

        [Fact]
        public void Ignores_When_A_File_Was_Imported_As_Long_As_Both_Export_Dates_Are_Known()
        {
            var laterExportImportedFirst = Create(
                exportedOn: new DateOnly(2026, 3, 14),
                importedAt: AnImportTime);
            var earlierExportImportedLater = Create(
                exportedOn: new DateOnly(2026, 3, 13),
                importedAt: AnImportTime.AddDays(1));

            Assert.False(earlierExportImportedLater.IsLaterThan(laterExportImportedFirst));
        }

        [Fact]
        public void Falls_Back_To_The_Import_Time_When_An_Export_Date_Is_Unknown()
        {
            var withoutExportDate = Create(exportedOn: null, importedAt: AnImportTime.AddDays(1));
            var withExportDate = Create(exportedOn: new DateOnly(2026, 3, 14), importedAt: AnImportTime);

            Assert.True(withoutExportDate.IsLaterThan(withExportDate));
        }
    }
}
