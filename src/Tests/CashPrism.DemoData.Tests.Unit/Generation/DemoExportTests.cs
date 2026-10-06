using CashPrism.DemoData.Generation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.DemoData.Tests.Unit.Generation;

public sealed class DemoExportTests
{
    /// <summary>
    /// One test per special case a real export was measured to carry, so a
    /// change to the generator that drops one fails by name. See
    /// <c>docs/finanzguru-export.md</c> for the measurements.
    /// </summary>
    public sealed class Generate
    {
        private static readonly DateOnly Until = new(2026, 10, 1);

        private static readonly IReadOnlyList<DemoRow> Rows = DemoExport.Generate(Until);

        private static readonly IReadOnlyList<DemoBooking> Bookings = Rows.Select(row => row.Booking).ToList();

        [Fact]
        public void Gives_The_Same_Rows_For_The_Same_Day()
            => Assert.Equal(Rows, DemoExport.Generate(Until));

        [Fact]
        public void Gives_Other_Rows_For_Another_Day()
            => Assert.NotEqual(Rows, DemoExport.Generate(Until.AddDays(-1)));

        [Fact]
        public void Covers_The_Three_Years_That_End_On_The_Given_Day()
        {
            Assert.All(Bookings, booking => Assert.InRange(
                DateOnly.FromDateTime(booking.Date),
                new DateOnly(2023, 10, 2),
                Until));
            Assert.InRange(DateOnly.FromDateTime(Bookings[^1].Date), new DateOnly(2023, 10, 2), new DateOnly(2023, 10, 9));
        }

        [Fact]
        public void Orders_The_Rows_Newest_First()
            => Assert.Equal(Bookings.OrderByDescending(booking => booking.Date).Select(b => b.Date), Bookings.Select(b => b.Date));

        [Fact]
        public void Keeps_Seven_Accounts()
            => Assert.Equal(7, Bookings.Select(booking => booking.Account).Distinct().Count());

        [Fact]
        public void Gives_Every_Booking_A_Distinct_Id_Of_40_Hex_Characters()
        {
            Assert.All(Bookings, booking => Assert.Matches("^[0-9a-f]{40}$", booking.BookingId));
            Assert.Equal(Bookings.Count, Bookings.Select(booking => booking.BookingId).Distinct().Count());
        }

        [Fact]
        public void Uses_Only_The_Measured_Category_Catalogue()
            => Assert.All(Bookings, booking => Assert.Contains(booking.Category, CategoryCatalogue.Pairs));

        [Fact]
        public void Writes_Umlauts_Out_In_Every_Category()
            => Assert.All(Bookings, booking => Assert.DoesNotMatch("[äöüÄÖÜß]", booking.Category.Main + booking.Category.Sub));

        [Fact]
        public void Has_Contracts_At_Every_Interval()
            => Assert.Equal(
                ["halbjaehrlich", "jaehrlich", "monatlich", "vierteljaehrlich", "zweimonatlich"],
                Bookings.Where(b => b.Contract is not null).Select(b => b.Contract!.Interval.Word).Distinct().Order());

        [Fact]
        public void Gives_Every_Contract_An_Id_Of_32_Hex_Characters()
            => Assert.All(
                Bookings.Where(booking => booking.Contract is not null),
                booking => Assert.Matches("^[0-9a-f]{32}$", booking.Contract!.Id));

        [Fact]
        public void Has_Income_Contracts()
            => Assert.Contains(Bookings, booking => booking.Contract is not null && booking.AmountInCents > 0);

        [Fact]
        public void Has_A_Contract_That_Ended_More_Than_A_Year_Before_The_Export()
            => Assert.Contains(
                Bookings.Where(booking => booking.Contract is not null).GroupBy(booking => booking.Contract!.Id),
                contract => contract.Max(booking => booking.Date) < Until.AddYears(-1).ToDateTime(TimeOnly.MinValue));

        [Fact]
        public void Books_Every_Transfer_As_A_Pair_Between_Two_Own_Accounts()
            => Assert.All(
                Bookings.Where(booking => booking.IsInternalTransfer),
                transfer => Assert.Contains(Bookings, other =>
                    other.IsInternalTransfer
                    && other.Date == transfer.Date
                    && other.AmountInCents == -transfer.AmountInCents
                    && other.Account != transfer.Account));

        [Fact]
        public void Has_Exactly_One_Booking_Excluded_From_Disposable_Income_That_Is_Not_A_Transfer()
            => Assert.Single(Bookings, booking => booking.IsExcludedFromDisposableIncome && !booking.IsInternalTransfer);

        [Fact]
        public void Has_One_Split_Booking_Whose_Parts_Point_Back_And_Add_Up_To_The_Original()
        {
            var original = Assert.Single(Bookings, booking => booking.SplitType == FinanzguruSplitType.Original);
            var part = Assert.Single(Bookings, booking => booking.SplitType == FinanzguruSplitType.Part);
            var remainder = Assert.Single(Bookings, booking => booking.SplitType == FinanzguruSplitType.Remainder);

            Assert.Equal(original.BookingId, part.OriginalReferenceId);
            Assert.Equal(original.BookingId, remainder.OriginalReferenceId);
            Assert.Equal(original.AmountInCents, part.AmountInCents + remainder.AmountInCents);
        }

        [Fact]
        public void Leaves_The_Back_Reference_Empty_Outside_A_Split()
            => Assert.All(
                Bookings.Where(booking => booking.SplitType.Length == 0),
                booking => Assert.Empty(booking.OriginalReferenceId));

        [Fact]
        public void Starts_Every_Card_Payment_Reference_With_An_Iso_Timestamp()
        {
            var cardPayments = Bookings.Where(booking => booking.TransactionKind == DemoTransactionKind.CardPayment).ToList();

            Assert.NotEmpty(cardPayments);
            Assert.All(cardPayments, booking => Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2} ", booking.PaymentReference));
        }

        [Fact]
        public void Gives_Mandate_And_Creditor_Id_To_Direct_Debits_And_Nothing_Else()
            => Assert.All(Bookings, booking => Assert.Equal(
                booking.TransactionKind == DemoTransactionKind.DirectDebit,
                booking.Party.MandateReference.Length > 0 && booking.Party.CreditorId.Length > 0));

        [Fact]
        public void Gives_Valid_Creditor_Ids()
            => Assert.All(
                Bookings.Where(booking => booking.Party.CreditorId.Length > 0),
                booking => Assert.Matches("^DE[0-9]{2}ZZZ[0-9]{11}$", booking.Party.CreditorId));

        [Fact]
        public void Names_The_Payment_Provider_Account_By_A_Handle()
            => Assert.DoesNotMatch("^DE[0-9]{20}$", DemoHousehold.ProviderAccount.Reference);

        [Fact]
        public void Puts_An_Email_Address_In_The_Iban_Column_Only_On_The_Payment_Provider_Account()
        {
            var byEmail = Bookings.Where(booking => booking.Party.Iban.Contains('@', StringComparison.Ordinal)).ToList();

            Assert.NotEmpty(byEmail);
            Assert.All(byEmail, booking => Assert.Equal(DemoHousehold.ProviderAccount, booking.Account));
        }

        [Fact]
        public void Gives_A_Time_Of_Day_To_Exactly_The_Rows_Paid_To_An_Email_Address()
            => Assert.All(Bookings, booking => Assert.Equal(
                booking.Party.Iban.Contains('@', StringComparison.Ordinal),
                booking.Date.TimeOfDay != TimeSpan.Zero));

        [Fact]
        public void Names_The_Credit_Card_By_A_Uuid()
            => Assert.Matches("^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$", DemoHousehold.CreditCard.Reference);

        [Fact]
        public void Puts_Tags_On_A_Few_Rows()
        {
            var tagged = Bookings.Count(booking => booking.Tags.Length > 0);

            Assert.InRange(tagged, 3, Bookings.Count / 20);
        }

        [Fact]
        public void Has_An_Account_That_Fell_Silent_More_Than_A_Year_Before_The_Export()
            => Assert.Contains(
                Bookings.GroupBy(booking => booking.Account),
                account => account.Max(booking => booking.Date) < Until.AddYears(-1).ToDateTime(TimeOnly.MinValue));

        [Fact]
        public void Derives_The_Period_Labels_From_The_Booking_Day()
            => Assert.All(Rows, row =>
            {
                var day = DateOnly.FromDateTime(row.Booking.Date);

                Assert.Equal(
                    (FinanzguruPeriodLabels.Week(day), FinanzguruPeriodLabels.Month(day), FinanzguruPeriodLabels.Quarter(day), day.Year),
                    (row.Week, row.Month, row.Quarter, row.Year));
            });

        [Fact]
        public void Calls_A_Positive_Amount_Income_And_Any_Other_Spending()
            => Assert.All(Rows, row => Assert.Equal(
                row.Booking.AmountInCents > 0 ? "Einnahmen" : "Ausgaben",
                row.AmountDirection));
    }
}
