using CashPrism.Domain.Accounts;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;
using CashPrism.TestSupport.Bookings;

namespace CashPrism.Domain.Tests.Unit.Accounts;

public sealed class AccountBalanceRuleTests
{
    private const string Giro = "DE02120300000000202051";
    private const string Savings = "DE89370400440532013000";

    private static readonly DateOnly ExportDay = new(2026, 10, 5);

    private static ImportRun Run(DateOnly? exportedOn = null, DateTimeOffset? importedAt = null)
        => new(
            Guid.NewGuid(),
            fileName: "20261005_Export_Alle_Buchungen.xlsx",
            fileHash: "7b5c8f",
            sheetName: "20261005_Export_Alle_Buchungen",
            exportedOn,
            importedAt ?? new DateTimeOffset(2026, 10, 7, 18, 0, 0, TimeSpan.Zero));

    private static Booking Row(
        DateOnly bookedOn,
        long reportedBalanceInCents,
        string accountReference = Giro,
        string accountName = "Girokonto")
        => TestBookings.Create(
            fingerprint: Guid.NewGuid().ToString("N"),
            bookedOn: bookedOn.ToDateTime(TimeOnly.MinValue),
            accountReference: accountReference,
            accountName: accountName,
            reportedBalanceInCents: reportedBalanceInCents);

    public sealed class For
    {
        [Fact]
        public void Takes_The_Balance_Of_The_Topmost_Row()
        {
            var rows = new[] { Row(ExportDay, 150_000), Row(ExportDay.AddDays(-3), 90_000) };

            var balance = Assert.Single(AccountBalanceRule.For(Run(ExportDay), rows));

            Assert.Equal(150_000, balance.BalanceInCents);
        }

        [Fact]
        public void Skips_Rows_Dated_After_The_Export_Day()
        {
            var rows = new[] { Row(ExportDay.AddDays(1), 99_000), Row(ExportDay, 150_000) };

            var balance = Assert.Single(AccountBalanceRule.For(Run(ExportDay), rows));

            Assert.Equal(150_000, balance.BalanceInCents);
        }

        [Fact]
        public void Decides_Two_Rows_On_The_Same_Day_By_The_Order_Of_The_File()
        {
            var rows = new[] { Row(ExportDay, 120_000), Row(ExportDay, 150_000) };

            var balance = Assert.Single(AccountBalanceRule.For(Run(ExportDay), rows));

            Assert.Equal(120_000, balance.BalanceInCents);
        }

        [Fact]
        public void Cuts_Off_At_The_Import_Day_Without_An_Export_Date()
        {
            var importDay = new DateOnly(2026, 10, 7);
            var rows = new[] { Row(importDay.AddDays(1), 99_000), Row(importDay, 150_000) };

            var balance = Assert.Single(AccountBalanceRule.For(
                Run(exportedOn: null, importedAt: new DateTimeOffset(2026, 10, 7, 18, 0, 0, TimeSpan.Zero)),
                rows));

            Assert.Equal(150_000, balance.BalanceInCents);
        }

        [Fact]
        public void Takes_The_Topmost_Row_When_Every_Row_Lies_After_The_Export_Day()
        {
            var rows = new[] { Row(ExportDay.AddDays(2), 99_000), Row(ExportDay.AddDays(1), 98_000) };

            var balance = Assert.Single(AccountBalanceRule.For(Run(ExportDay), rows));

            Assert.Equal(99_000, balance.BalanceInCents);
        }

        [Fact]
        public void Takes_One_Balance_Per_Account()
        {
            var rows = new[]
            {
                Row(ExportDay, 150_000, Giro),
                Row(ExportDay, 800_000, Savings),
                Row(ExportDay.AddDays(-1), 140_000, Giro),
            };

            var balances = AccountBalanceRule.For(Run(ExportDay), rows);

            Assert.Equal(
                [(Giro, 150_000L), (Savings, 800_000L)],
                balances.Select(balance => (balance.AccountReference, balance.BalanceInCents)));
        }

        [Fact]
        public void Names_The_Account_After_Its_Newest_Booking()
        {
            var rows = new[]
            {
                Row(ExportDay.AddDays(-5), 150_000, accountName: "Alter Name"),
                Row(ExportDay.AddDays(1), 99_000, accountName: "Girokonto"),
            };

            var balance = Assert.Single(AccountBalanceRule.For(Run(ExportDay), rows));

            Assert.Equal("Girokonto", balance.AccountName);
        }

        [Fact]
        public void Dates_The_Account_By_Its_Newest_Booking_Even_After_The_Export_Day()
        {
            var rows = new[] { Row(ExportDay, 150_000), Row(ExportDay.AddDays(1), 99_000) };

            var balance = Assert.Single(AccountBalanceRule.For(Run(ExportDay), rows));

            Assert.Equal(ExportDay.AddDays(1).ToDateTime(TimeOnly.MinValue), balance.LatestBookedOn);
        }

        [Fact]
        public void Belongs_To_The_Run_That_Read_The_Export()
        {
            var run = Run(ExportDay);

            var balance = Assert.Single(AccountBalanceRule.For(run, [Row(ExportDay, 150_000)]));

            Assert.Equal(run.Id, balance.ImportRunId);
        }
    }
}
