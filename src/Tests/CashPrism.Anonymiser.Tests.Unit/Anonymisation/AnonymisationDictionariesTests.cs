using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class AnonymisationDictionariesTests
{
    public sealed class Build
    {
        [Fact]
        public void Referenzkonto_And_Counterparty_Iban_Share_One_Dictionary()
        {
            const string ownIban = "DE02120300000000202051";

            var dictionaries = AnonymisationDictionaries.Build(
                [
                    OneFile(
                        (FinanzguruColumns.AccountReference, [ownIban]),
                        (FinanzguruColumns.CounterpartyIban, [ownIban])),
                ]);

            Assert.Equal(
                dictionaries.Replace(FinanzguruColumns.AccountReference, ownIban),
                dictionaries.Replace(FinanzguruColumns.CounterpartyIban, ownIban));
        }

        [Fact]
        public void Name_Referenzkonto_And_Counterparty_Share_One_Dictionary()
        {
            const string ownAccountName = "Girokonto";

            var dictionaries = AnonymisationDictionaries.Build(
                [
                    OneFile(
                        (FinanzguruColumns.AccountName, [ownAccountName]),
                        (FinanzguruColumns.Counterparty, [ownAccountName])),
                ]);

            Assert.Equal(
                dictionaries.Replace(FinanzguruColumns.AccountName, ownAccountName),
                dictionaries.Replace(FinanzguruColumns.Counterparty, ownAccountName));
            Assert.StartsWith("Account ", dictionaries.Replace(FinanzguruColumns.AccountName, ownAccountName), StringComparison.Ordinal);
        }

        [Fact]
        public void Booking_Id_And_Referenz_Original_Id_Share_One_Dictionary()
        {
            const string bookingId = "abc123";

            var dictionaries = AnonymisationDictionaries.Build(
                [
                    OneFile(
                        (FinanzguruColumns.BookingId, [bookingId]),
                        (FinanzguruColumns.OriginalReferenceId, [bookingId])),
                ]);

            Assert.Equal(
                dictionaries.Replace(FinanzguruColumns.BookingId, bookingId),
                dictionaries.Replace(FinanzguruColumns.OriginalReferenceId, bookingId));
        }

        [Fact]
        public void Several_Files_Share_The_Same_Dictionary()
        {
            var dictionaries = AnonymisationDictionaries.Build(
                [
                    OneFile((FinanzguruColumns.Tags, ["holiday"])),
                    OneFile((FinanzguruColumns.Tags, ["holiday"])),
                ]);

            Assert.Single(dictionaries.OriginalValues(FinanzguruColumns.Tags));
        }

        [Fact]
        public void OriginalValues_Reports_Every_Distinct_Value_Seen_Across_The_Run()
        {
            var dictionaries = AnonymisationDictionaries.Build(
                [
                    OneFile((FinanzguruColumns.Tags, ["holiday", "holiday", "business"])),
                ]);

            Assert.Equal(2, dictionaries.OriginalValues(FinanzguruColumns.Tags).Count);
        }

        [Fact]
        public void Replace_Rejects_A_Column_That_Is_Not_Replaced()
        {
            var dictionaries = AnonymisationDictionaries.Build([OneFile()]);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => dictionaries.Replace(FinanzguruColumns.BookingDate, "01.03.2026"));
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> OneFile(
            params (string Column, string[] Values)[] columns)
        {
            var values = AnonymisationDictionaries.ReplacedColumns.ToDictionary(
                column => column, _ => (IReadOnlyList<string>)Array.Empty<string>(), StringComparer.Ordinal);

            foreach (var (column, columnValues) in columns)
            {
                values[column] = columnValues;
            }

            return values;
        }
    }
}
