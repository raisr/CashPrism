using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;

namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruBookingTests
{
    private const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";
    private const string AnotherFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    private static readonly Guid ARunId = Guid.Parse("8f3b1c2d-4e5f-4a6b-8c9d-0e1f2a3b4c5d");

    private static FinanzguruExportRow Row(params (string Column, string Value)[] overrides)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FinanzguruColumns.BookingId] = AFingerprint,
            [FinanzguruColumns.Currency] = "EUR",
            [FinanzguruColumns.AccountReference] = "DE02120300000000202051",
            [FinanzguruColumns.AccountName] = "Girokonto",
            [FinanzguruColumns.Counterparty] = "Supermarkt",
            [FinanzguruColumns.CounterpartyIban] = "DE02500105170137075030",
            [FinanzguruColumns.PaymentReference] = "Kartenzahlung",
            [FinanzguruColumns.MainCategory] = "Lebensmittel",
            [FinanzguruColumns.SubCategory] = "Supermarkt",
            [FinanzguruColumns.IsInternalTransfer] = "nein",
            [FinanzguruColumns.SplitType] = string.Empty,
            [FinanzguruColumns.OriginalReferenceId] = string.Empty,
        };

        foreach (var (column, value) in overrides)
        {
            values[column] = value;
        }

        return new FinanzguruExportRow(
            RowNumber: 2,
            BookedOn: new DateTime(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified),
            AmountInCents: -6317,
            BalanceInCents: 324000,
            values);
    }

    public sealed class Create
    {
        [Fact]
        public void Takes_The_Booking_Id_As_The_Identity()
            => Assert.Equal(AFingerprint, FinanzguruBooking.Create(Row(), ARunId).Value!.Fingerprint);

        [Fact]
        public void Keeps_The_Typed_Date_And_Amount_The_Reader_Produced()
        {
            var booking = FinanzguruBooking.Create(Row(), ARunId).Value!;

            Assert.Equal(new DateTime(2026, 3, 12, 9, 41, 0), booking.BookedOn);
            Assert.Equal(-6317L, booking.AmountInCents);
        }

        [Fact]
        public void Records_The_Run_Its_State_Came_From()
            => Assert.Equal(ARunId, FinanzguruBooking.Create(Row(), ARunId).Value!.SourceImportRunId);

        [Fact]
        public void Translates_The_German_Word_For_A_Transfer()
        {
            var row = Row((FinanzguruColumns.IsInternalTransfer, "ja"));

            Assert.True(FinanzguruBooking.Create(row, ARunId).Value!.IsTransfer);
        }

        [Fact]
        public void Translates_The_German_Word_For_A_Split_Part()
        {
            var row = Row(
                (FinanzguruColumns.SplitType, "Teilbuchung"),
                (FinanzguruColumns.OriginalReferenceId, AnotherFingerprint));

            var booking = FinanzguruBooking.Create(row, ARunId).Value!;

            Assert.Equal(SplitRole.Part, booking.SplitRole);
            Assert.Equal(AnotherFingerprint, booking.OriginalFingerprint);
        }

        [Fact]
        public void Reads_An_Empty_Back_Reference_As_No_Back_Reference()
            => Assert.Null(FinanzguruBooking.Create(Row(), ARunId).Value!.OriginalFingerprint);

        [Fact]
        public void Fails_And_Names_The_Column_When_The_Booking_Id_Is_Empty()
        {
            var result = FinanzguruBooking.Create(Row((FinanzguruColumns.BookingId, " ")), ARunId);

            Assert.False(result.IsSuccess);
            Assert.Equivalent(ImportError.EmptyValue(FinanzguruColumns.BookingId, 2), result.Errors.Single(), strict: true);
        }

        [Fact]
        public void Fails_When_The_Currency_Is_Empty()
        {
            var result = FinanzguruBooking.Create(Row((FinanzguruColumns.Currency, "")), ARunId);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Fails_When_The_Account_Is_Empty()
        {
            var result = FinanzguruBooking.Create(Row((FinanzguruColumns.AccountReference, "")), ARunId);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Fails_When_The_Transfer_Column_Carries_A_Word_It_Does_Not_Know()
        {
            var result = FinanzguruBooking.Create(
                Row((FinanzguruColumns.IsInternalTransfer, "vielleicht")),
                ARunId);

            Assert.False(result.IsSuccess);
            Assert.Equivalent(
                ImportError.NotAFlag(FinanzguruColumns.IsInternalTransfer, 2, "vielleicht", FinanzguruFlag.Yes, FinanzguruFlag.No),
                result.Errors.Single(),
                strict: true);
        }

        [Fact]
        public void Fails_When_A_Split_Part_Does_Not_Say_Which_Booking_It_Belongs_To()
        {
            var result = FinanzguruBooking.Create(Row((FinanzguruColumns.SplitType, "Teilbuchung")), ARunId);

            Assert.False(result.IsSuccess);
            Assert.Equivalent(
                ImportError.SplitPartWithoutOriginal(FinanzguruColumns.SplitType, 2, FinanzguruColumns.OriginalReferenceId),
                result.Errors.Single(),
                strict: true);
        }

        /// <summary>
        /// One error per problem, so a person fixing an export is told
        /// everything that is wrong with a row rather than the first thing.
        /// </summary>
        [Fact]
        public void Names_Every_Problem_A_Row_Has_At_Once()
        {
            var row = Row(
                (FinanzguruColumns.BookingId, ""),
                (FinanzguruColumns.Currency, ""),
                (FinanzguruColumns.IsInternalTransfer, "vielleicht"));

            var result = FinanzguruBooking.Create(row, ARunId);

            Assert.Equal(3, result.Errors.Count);
        }

        [Fact]
        public void Rejects_A_Booking_Without_The_Run_It_Came_From()
            => Assert.Throws<ArgumentException>(() => FinanzguruBooking.Create(Row(), Guid.Empty));
    }
}
