using System.Globalization;
using CashPrism.Web.Imports;

namespace CashPrism.Web.Tests.Unit.Imports;

/// <summary>
/// How the list of past imports writes a run's fields. The culture is passed in
/// rather than inherited: the host pins German, and a test that read the
/// machine's culture would pass or fail depending on which machine ran it.
/// </summary>
public sealed class ImportRunFormatTests
{
    private static readonly CultureInfo German = new("de-DE");

    public sealed class ImportedAt
    {
        [Fact]
        public void Writes_The_Date_And_The_Time_Of_Day()
        {
            var importedAt = new DateTimeOffset(2026, 9, 7, 18, 4, 0, TimeSpan.Zero);

            var written = ImportRunFormat.ImportedAt(importedAt, German);

            // The local time of whichever machine runs the test, so the assertion
            // is on the shape rather than on the hour.
            Assert.Equal(importedAt.LocalDateTime.ToString("g", German), written);
        }

        [Fact]
        public void Is_Written_In_The_Local_Time_Of_The_Machine()
        {
            var importedAt = new DateTimeOffset(2026, 9, 7, 18, 4, 0, TimeSpan.FromHours(9));

            var written = ImportRunFormat.ImportedAt(importedAt, German);

            Assert.Equal(importedAt.LocalDateTime.ToString("g", German), written);
            Assert.DoesNotContain("+", written, StringComparison.Ordinal);
        }
    }

    public sealed class ExportedOn
    {
        [Fact]
        public void Writes_A_Date_The_German_Way()
        {
            Assert.Equal("07.09.2026", ImportRunFormat.ExportedOn(new DateOnly(2026, 9, 7), German));
        }

        /// <summary>
        /// The caller decides what a run without an export date reads as, because
        /// that text is German and belongs in the resource file.
        /// </summary>
        [Fact]
        public void Says_Nothing_When_The_Sheet_Name_Carried_No_Date()
        {
            Assert.Null(ImportRunFormat.ExportedOn(null, German));
        }
    }

    public sealed class ImportedOn
    {
        [Fact]
        public void Writes_The_Day_Without_The_Time()
        {
            // Noon, so the local day is the same on every machine that runs it.
            var importedAt = new DateTimeOffset(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);

            Assert.Equal("07.09.2026", ImportRunFormat.ImportedOn(importedAt, German));
        }

        [Fact]
        public void Takes_The_Day_In_The_Local_Time_Of_The_Machine()
        {
            var importedAt = new DateTimeOffset(2026, 9, 7, 23, 30, 0, TimeSpan.FromHours(-10));

            var written = ImportRunFormat.ImportedOn(importedAt, German);

            Assert.Equal(importedAt.LocalDateTime.ToString("d", German), written);
        }
    }

    public sealed class Count
    {
        [Fact]
        public void Groups_A_Four_Digit_Count()
        {
            Assert.Equal("6.327", ImportRunFormat.Count(6327, German));
        }

        [Fact]
        public void Leaves_A_Small_Count_Alone()
        {
            Assert.Equal("0", ImportRunFormat.Count(0, German));
        }

        [Fact]
        public void Follows_The_Culture_It_Is_Given()
        {
            Assert.Equal("6,327", ImportRunFormat.Count(6327, CultureInfo.GetCultureInfo("en-GB")));
        }
    }

    public sealed class ShortHash
    {
        private const string ASha256 = "3b8f1c2d4e5a6b7c8d9e0f1a2b3c4d5e6f708192a3b4c5d6e7f8091a2b3c4d5e";

        [Fact]
        public void Keeps_Only_The_Leading_Characters_Of_A_Long_Hash()
        {
            Assert.Equal("3b8f1c2d4e5a", ImportRunFormat.ShortHash(ASha256));
        }

        [Fact]
        public void Shortens_To_The_Length_The_Column_Was_Built_For()
        {
            Assert.Equal(ImportRunFormat.HashPrefixLength, ImportRunFormat.ShortHash(ASha256).Length);
        }

        [Fact]
        public void Leaves_A_Hash_Shorter_Than_That_As_It_Is()
        {
            Assert.Equal("3b8f1c", ImportRunFormat.ShortHash("3b8f1c"));
        }

        [Fact]
        public void Leaves_A_Hash_Of_Exactly_That_Length_As_It_Is()
        {
            var twelve = ASha256[..ImportRunFormat.HashPrefixLength];

            Assert.Equal(twelve, ImportRunFormat.ShortHash(twelve));
        }
    }
}
