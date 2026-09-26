namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruRawRowTests
{
    public sealed class ToJson
    {
        [Fact]
        public void Writes_The_Columns_In_The_Order_The_Export_Writes_Them()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FinanzguruColumns.Tags] = "urlaub",
                [FinanzguruColumns.BookingDate] = "2026-03-03T00:00:00",
                [FinanzguruColumns.Currency] = "EUR",
            };

            var json = FinanzguruRawRow.ToJson(values);

            Assert.Equal(
                """{"Buchungstag":"2026-03-03T00:00:00","Waehrung":"EUR","Tags":"urlaub"}""",
                json);
        }

        /// <summary>
        /// The stored text is compared byte for byte to decide whether a booking
        /// changed, so the same row has to render identically however the
        /// dictionary was built.
        /// </summary>
        [Fact]
        public void Renders_The_Same_Row_Identically_Whatever_Order_It_Was_Built_In()
        {
            var one = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FinanzguruColumns.Currency] = "EUR",
                [FinanzguruColumns.Amount] = "-63.17",
            };
            var another = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FinanzguruColumns.Amount] = "-63.17",
                [FinanzguruColumns.Currency] = "EUR",
            };

            Assert.Equal(FinanzguruRawRow.ToJson(one), FinanzguruRawRow.ToJson(another));
        }

        [Fact]
        public void Puts_A_Column_It_Does_Not_Know_After_The_Ones_It_Does()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Analyse-Nebelkerze"] = "ja",
                [FinanzguruColumns.Currency] = "EUR",
            };

            var json = FinanzguruRawRow.ToJson(values);

            Assert.Equal("""{"Waehrung":"EUR","Analyse-Nebelkerze":"ja"}""", json);
        }

        [Fact]
        public void Keeps_An_Empty_Cell_As_An_Empty_Value()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FinanzguruColumns.EndToEndReference] = string.Empty,
            };

            Assert.Equal("""{"E-Ref":""}""", FinanzguruRawRow.ToJson(values));
        }

        /// <summary>
        /// A payment reference carries whatever the bank sent, line breaks
        /// included — 92 rows of the measured export do.
        /// </summary>
        [Fact]
        public void Escapes_A_Line_Break_Without_Escaping_Everything_Else()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FinanzguruColumns.PaymentReference] = "Rechnung\nMüller & Söhne",
            };

            Assert.Equal(
                """{"Verwendungszweck":"Rechnung\nMüller & Söhne"}""",
                FinanzguruRawRow.ToJson(values));
        }
    }
}
