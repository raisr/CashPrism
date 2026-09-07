namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruColumnMapTests
{
    public sealed class Resolve
    {
        [Fact]
        public void All_Known_Columns_Present_Resolves_Every_Column()
        {
            var headerRow = FinanzguruColumns.All.ToArray();

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.MissingColumns);
            Assert.Empty(result.UnknownColumns);
            Assert.Empty(result.DuplicateColumns);
            Assert.Equal(FinanzguruColumns.All.Count, result.Columns.Count);
            Assert.All(headerRow, header => Assert.Equal(
                Array.IndexOf(headerRow, header),
                result.Columns[header]));
        }

        [Fact]
        public void Missing_Column_Fails_Naming_It()
        {
            var headerRow = FinanzguruColumns.All
                .Where(column => column != FinanzguruColumns.EndToEndReference)
                .ToArray();

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.False(result.IsSuccess);
            Assert.Equal([FinanzguruColumns.EndToEndReference], result.MissingColumns);
        }

        [Fact]
        public void Several_Missing_Columns_Are_All_Named_In_Export_Order()
        {
            var headerRow = FinanzguruColumns.All
                .Where(column => column != FinanzguruColumns.Tags)
                .Where(column => column != FinanzguruColumns.Amount)
                .ToArray();

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.False(result.IsSuccess);
            Assert.Equal([FinanzguruColumns.Amount, FinanzguruColumns.Tags], result.MissingColumns);
        }

        [Fact]
        public void Failure_Yields_No_Partial_Mapping()
        {
            var headerRow = FinanzguruColumns.All
                .Where(column => column != FinanzguruColumns.Amount)
                .ToArray();

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.Empty(result.Columns);
        }

        [Fact]
        public void Unknown_Extra_Column_Succeeds_And_Reports_It()
        {
            string[] headerRow = [.. FinanzguruColumns.All, "Analyse-Sondertarif"];

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.True(result.IsSuccess);
            Assert.Equal(["Analyse-Sondertarif"], result.UnknownColumns);
            Assert.Equal(FinanzguruColumns.All.Count, result.Columns.Count);
        }

        [Fact]
        public void Repeated_Unknown_Column_Is_Reported_Once()
        {
            string[] headerRow = [.. FinanzguruColumns.All, "Analyse-Sondertarif", "Analyse-Sondertarif"];

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.True(result.IsSuccess);
            Assert.Equal(["Analyse-Sondertarif"], result.UnknownColumns);
        }

        [Fact]
        public void Shuffled_Order_Resolves_To_The_Shuffled_Indices()
        {
            var headerRow = FinanzguruColumns.All.Reverse().ToArray();

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Columns[FinanzguruColumns.Tags]);
            Assert.Equal(headerRow.Length - 1, result.Columns[FinanzguruColumns.BookingDate]);
            Assert.All(headerRow, header => Assert.Equal(
                Array.IndexOf(headerRow, header),
                result.Columns[header]));
        }

        [Fact]
        public void Duplicate_Known_Column_Fails_Naming_It()
        {
            string[] headerRow = [.. FinanzguruColumns.All, FinanzguruColumns.Amount];

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.False(result.IsSuccess);
            Assert.Equal([FinanzguruColumns.Amount], result.DuplicateColumns);
            Assert.Empty(result.MissingColumns);
            Assert.Empty(result.Columns);
        }

        [Fact]
        public void Blank_Header_Cells_Are_Ignored()
        {
            string[] headerRow = [.. FinanzguruColumns.All, "", "   "];

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.UnknownColumns);
        }

        [Fact]
        public void Surrounding_Whitespace_Is_Ignored()
        {
            var headerRow = FinanzguruColumns.All
                .Select(column => $"  {column} ")
                .ToArray();

            var result = FinanzguruColumnMap.Resolve(headerRow);

            Assert.True(result.IsSuccess);
            Assert.Equal(FinanzguruColumns.All.Count, result.Columns.Count);
        }

        [Fact]
        public void Empty_Header_Row_Fails_Naming_Every_Column()
        {
            var result = FinanzguruColumnMap.Resolve([]);

            Assert.False(result.IsSuccess);
            Assert.Equal(FinanzguruColumns.All, result.MissingColumns);
        }
    }
}
