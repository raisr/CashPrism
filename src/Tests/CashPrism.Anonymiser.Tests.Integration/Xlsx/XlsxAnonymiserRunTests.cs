using System.IO.Compression;
using System.Text.RegularExpressions;
using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Anonymiser.Tests.Integration.Fixtures;
using CashPrism.Anonymiser.Xlsx;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Integration.Xlsx;

public sealed class XlsxAnonymiserRunTests
{
    public sealed class Run : IDisposable
    {
        private readonly string _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "cashprism-anonymiser-tests", Guid.NewGuid().ToString("N"))).FullName;

        private readonly string _outputDirectory;

        public Run()
        {
            _outputDirectory = Path.Combine(_root, "out");
        }

        public void Dispose() => Directory.Delete(_root, recursive: true);

        [Fact]
        public void Every_Zip_Entry_Other_Than_The_Worksheet_Is_Byte_Identical_To_The_Input()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            Assert.True(result.IsSuccess);

            using var expected = ZipFile.OpenRead(inputPath);
            using var actual = ZipFile.OpenRead(result.FileResults[0].OutputPath);

            var expectedNames = expected.Entries.Select(e => e.FullName).OrderBy(n => n, StringComparer.Ordinal);
            var actualNames = actual.Entries.Select(e => e.FullName).OrderBy(n => n, StringComparer.Ordinal);

            Assert.Equal(expectedNames, actualNames);

            foreach (var entry in expected.Entries.Where(e => e.FullName != "xl/worksheets/sheet1.xml"))
            {
                Assert.Equal(ReadAllBytes(entry), ReadAllBytes(actual.GetEntry(entry.FullName)!));
            }
        }

        [Fact]
        public void Kept_Columns_Stay_Byte_Identical_Outside_The_Replaced_Cells()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            var inputXml = ReadWorksheetXml(inputPath);
            var outputXml = ReadWorksheetXml(result.FileResults[0].OutputPath);

            var replacedLetters = InlineStringCells.ResolveColumnLetters(inputXml)
                .Where(pair => AnonymisationDictionaries.ReplacedColumns.Contains(pair.Key))
                .Select(pair => pair.Value)
                .ToHashSet(StringComparer.Ordinal);

            var normalisedInput = InlineStringCells.Rewrite(inputXml, replacedLetters, (_, _) => "X");
            var normalisedOutput = InlineStringCells.Rewrite(outputXml, replacedLetters, (_, _) => "X");

            Assert.Equal(normalisedInput, normalisedOutput);
        }

        [Fact]
        public void Replaced_Values_Become_Sequential_Placeholders()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var result = Anonymise([inputPath], _outputDirectory, force: false);
            var values = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.Counterparty);

            Assert.Equal(["Counterparty 01", "Counterparty 02"], values.OrderBy(v => v, StringComparer.Ordinal));
        }

        [Fact]
        public void The_Same_Counterparty_Always_Gets_The_Same_Placeholder()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [Row("Bakery", "01.03.2026"), Row("Bakery", "02.03.2026"), Row("Landlord", "03.03.2026")]));

            var result = Anonymise([inputPath], _outputDirectory, force: false);
            var values = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.Counterparty);

            Assert.Equal(2, values.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(values[0], values[1]);
        }

        [Fact]
        public void An_Own_Iban_Reappearing_As_A_Counterparty_Iban_Gets_The_Same_Replacement()
        {
            const string ownIban = "DE02120300000000202051";

            static Dictionary<string, string> Row(string bookingDate, string referenceAccount, string? counterpartyIban)
            {
                var row = new Dictionary<string, string>
                {
                    [FinanzguruColumns.BookingDate] = bookingDate,
                    [FinanzguruColumns.AccountReference] = referenceAccount,
                };

                if (counterpartyIban is not null)
                {
                    row[FinanzguruColumns.CounterpartyIban] = counterpartyIban;
                }

                return row;
            }

            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [Row("01.03.2026", ownIban, null), Row("02.03.2026", ownIban, ownIban)]));

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            var referenceValues = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.AccountReference);
            var counterpartyIbanValues = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.CounterpartyIban);

            Assert.Equal(referenceValues[0], referenceValues[1]);
            Assert.Equal(referenceValues[1], counterpartyIbanValues.Single());
        }

        [Fact]
        public void An_Empty_Cell_Stays_Empty()
        {
            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [new Dictionary<string, string> { [FinanzguruColumns.BookingDate] = "01.03.2026" }]));

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            Assert.True(result.IsSuccess);
            Assert.Empty(ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.Counterparty));
        }

        [Fact]
        public void An_Email_Shaped_Iban_Value_Gets_An_Email_Shaped_Replacement()
        {
            static Dictionary<string, string> Row(string bookingDate, string counterpartyIban) => new()
            {
                [FinanzguruColumns.BookingDate] = bookingDate,
                [FinanzguruColumns.CounterpartyIban] = counterpartyIban,
            };

            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All, [Row("01.03.2026", "payer@example.com")]));

            var result = Anonymise([inputPath], _outputDirectory, force: false);
            var values = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.CounterpartyIban);

            Assert.Matches("^account-[0-9]{2}@example\\.invalid$", values.Single());
        }

        [Fact]
        public void Two_Input_Files_Share_One_Dictionary()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            var firstInput = WriteInput(
                XlsxTestWorkbook.Build(FinanzguruColumns.All, [Row("Bakery", "01.03.2026")]), "a.xlsx");
            var secondInput = WriteInput(
                XlsxTestWorkbook.Build(FinanzguruColumns.All, [Row("Bakery", "02.03.2026")]), "b.xlsx");

            var result = Anonymise([firstInput, secondInput], _outputDirectory, force: false);

            Assert.True(result.IsSuccess);

            var firstValue = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.Counterparty).Single();
            var secondValue = ReadColumnValues(result.FileResults[1].OutputPath, FinanzguruColumns.Counterparty).Single();

            Assert.Equal(firstValue, secondValue);
        }

        [Fact]
        public void Reversing_The_Row_Order_Yields_The_Same_Value_To_Placeholder_Assignment()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            var rows = new[] { Row("Bakery", "01.03.2026"), Row("Landlord", "02.03.2026"), Row("Gym", "03.03.2026") };

            var forwardInput = WriteInput(XlsxTestWorkbook.Build(FinanzguruColumns.All, rows), "forward.xlsx");
            var reversedInput = WriteInput(
                XlsxTestWorkbook.Build(FinanzguruColumns.All, [.. rows.Reverse()]), "reversed.xlsx");

            var forwardResult = Anonymise([forwardInput], _outputDirectory, force: false);
            var reversedResult = Anonymise([reversedInput], _outputDirectory, force: false);

            var forwardByCounterparty = ReadColumnValues(forwardResult.FileResults[0].OutputPath, FinanzguruColumns.Counterparty);
            var reversedByCounterparty = ReadColumnValues(reversedResult.FileResults[0].OutputPath, FinanzguruColumns.Counterparty);

            // "Bakery" is row 0 forward and row 2 reversed — same original value, must land on the same placeholder.
            Assert.Equal(forwardByCounterparty[0], reversedByCounterparty[2]);
        }

        [Fact]
        public void Running_Twice_Produces_A_Byte_Identical_Worksheet()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var first = Anonymise([inputPath], _outputDirectory, force: false);
            var firstBytes = ReadAllBytes(first.FileResults[0].OutputPath);

            var second = Anonymise([inputPath], _outputDirectory, force: true);
            var secondBytes = ReadAllBytes(second.FileResults[0].OutputPath);

            Assert.Equal(firstBytes, secondBytes);
        }

        [Fact]
        public void SelfCheck_Failure_Deletes_The_Output_And_Reports_The_Column()
        {
            // A contrived but deterministic trigger: with exactly two distinct
            // Mandatsreferenz values, the second one sorted ordinally is
            // assigned "Mandate 02" — and one of the raw input values already
            // *is* that literal string, so the self-check finds its own
            // original text still present in the output and aborts.
            static Dictionary<string, string> Row(string bookingDate, string mandateReference) => new()
            {
                [FinanzguruColumns.BookingDate] = bookingDate,
                [FinanzguruColumns.MandateReference] = mandateReference,
            };

            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All, [Row("01.03.2026", "AAA"), Row("02.03.2026", "Mandate 02")]));

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.MandateReference, result.ErrorMessage);
            Assert.False(File.Exists(Path.Combine(_outputDirectory, "export-anonymised.xlsx")));
        }

        [Fact]
        public void Missing_Known_Column_Aborts_Naming_It()
        {
            var headerNames = FinanzguruColumns.All.Where(c => c != FinanzguruColumns.Tags).ToArray();
            var inputPath = WriteInput(XlsxTestWorkbook.Build(headerNames, []));

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.Tags, result.ErrorMessage);
        }

        [Fact]
        public void Existing_Output_Aborts_Without_Force()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var first = Anonymise([inputPath], _outputDirectory, force: false);
            Assert.True(first.IsSuccess);

            var second = Anonymise([inputPath], _outputDirectory, force: false);

            Assert.False(second.IsSuccess);
            Assert.Contains("already exists", second.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Existing_Output_Is_Overwritten_With_Force()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            Assert.True(Anonymise([inputPath], _outputDirectory, force: false).IsSuccess);

            var second = Anonymise([inputPath], _outputDirectory, force: true);

            Assert.True(second.IsSuccess);
        }

        [Fact]
        public void Without_Scale_Amounts_Are_Unchanged()
        {
            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All, [MoneyRow("01.03.2026", "-10.00", "200.00")]));

            var result = Anonymise([inputPath], _outputDirectory, force: false);

            Assert.Equal(["-10.00"], ReadMoneyValues(result.FileResults[0].OutputPath, FinanzguruColumns.Amount));
            Assert.Equal(["200.00"], ReadMoneyValues(result.FileResults[0].OutputPath, FinanzguruColumns.Balance));
        }

        [Fact]
        public void Scale_Halves_Betrag_And_Kontostand_In_Every_Row_Rounded_To_Two_Decimals()
        {
            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [MoneyRow("01.03.2026", "-10.01", "200.00"), MoneyRow("02.03.2026", "5.00", "190.00")]));

            var result = Anonymise([inputPath], _outputDirectory, force: false, scale: 0.5m);

            Assert.Equal(["-5.01", "2.50"], ReadMoneyValues(result.FileResults[0].OutputPath, FinanzguruColumns.Amount));
            Assert.Equal(["100.00", "95.00"], ReadMoneyValues(result.FileResults[0].OutputPath, FinanzguruColumns.Balance));
        }

        [Fact]
        public void Scale_Touches_Only_Betrag_And_Kontostand()
        {
            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All, [MoneyRow("01.03.2026", "-10.00", "200.00")]));

            var result = Anonymise([inputPath], _outputDirectory, force: false, scale: 0.5m);

            var inputXml = ReadWorksheetXml(inputPath);
            var outputXml = ReadWorksheetXml(result.FileResults[0].OutputPath);

            var columnLetters = InlineStringCells.ResolveColumnLetters(inputXml);
            var replacedLetters = AnonymisationDictionaries.ReplacedColumns
                .Select(column => columnLetters[column])
                .ToHashSet(StringComparer.Ordinal);
            var moneyLetters = new HashSet<string>(
                [columnLetters[FinanzguruColumns.Amount], columnLetters[FinanzguruColumns.Balance]], StringComparer.Ordinal);

            var normalisedInput = NumericCells.Rewrite(
                InlineStringCells.Rewrite(inputXml, replacedLetters, (_, _) => "X"), moneyLetters, _ => 0m);
            var normalisedOutput = NumericCells.Rewrite(
                InlineStringCells.Rewrite(outputXml, replacedLetters, (_, _) => "X"), moneyLetters, _ => 0m);

            Assert.Equal(normalisedInput, normalisedOutput);
        }

        [Fact]
        public void MaxRows_Writes_The_Header_Plus_The_First_N_Data_Rows()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [
                    Row("A", "01.03.2026"), Row("B", "02.03.2026"), Row("C", "03.03.2026"),
                    Row("D", "04.03.2026"), Row("E", "05.03.2026"),
                ]));

            var result = Anonymise([inputPath], _outputDirectory, force: false, maxRows: 3);

            Assert.Equal(5, result.FileResults[0].RowsRead);
            Assert.Equal(3, result.FileResults[0].RowsWritten);
            Assert.Equal(3, ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.Counterparty).Count);
        }

        [Fact]
        public void MaxRows_Larger_Than_The_Data_Rows_Writes_Every_Row_And_Does_Not_Fail()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var result = Anonymise([inputPath], _outputDirectory, force: false, maxRows: 100);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.FileResults[0].RowsRead);
            Assert.Equal(2, result.FileResults[0].RowsWritten);
        }

        [Fact]
        public void MaxRows_Keeps_Placeholder_Numbers_Dense()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            // File order is newest first, so the two rows --max-rows 2 keeps are
            // "Landlord" and "Cinema" — neither is first alphabetically among all
            // five. A dictionary built from the whole file would give them gappy
            // numbers; one built from only the retained rows gives "01" and "02".
            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [
                    Row("Landlord", "01.03.2026"), Row("Cinema", "02.03.2026"), Row("Bakery", "03.03.2026"),
                    Row("Gym", "04.03.2026"), Row("Pharmacy", "05.03.2026"),
                ]));

            var result = Anonymise([inputPath], _outputDirectory, force: false, maxRows: 2);

            var values = ReadColumnValues(result.FileResults[0].OutputPath, FinanzguruColumns.Counterparty);

            Assert.Equal(["Counterparty 01", "Counterparty 02"], values.OrderBy(v => v, StringComparer.Ordinal));
        }

        [Fact]
        public void MaxRows_And_Scale_Combine_With_Force()
        {
            var inputPath = WriteInput(XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [
                    MoneyRow("01.03.2026", "-10.00", "200.00"), MoneyRow("02.03.2026", "5.00", "190.00"),
                    MoneyRow("03.03.2026", "3.00", "193.00"),
                ]));

            Assert.True(Anonymise([inputPath], _outputDirectory, force: false, scale: 0.5m, maxRows: 2).IsSuccess);

            var second = Anonymise([inputPath], _outputDirectory, force: true, scale: 0.5m, maxRows: 2);

            Assert.True(second.IsSuccess);
            Assert.Equal(2, second.FileResults[0].RowsWritten);
            Assert.Equal(["-5.00", "2.50"], ReadMoneyValues(second.FileResults[0].OutputPath, FinanzguruColumns.Amount));
        }

        private static Dictionary<string, string> MoneyRow(string bookingDate, string amount, string balance) => new()
        {
            [FinanzguruColumns.BookingDate] = bookingDate,
            [FinanzguruColumns.Amount] = amount,
            [FinanzguruColumns.Balance] = balance,
        };

        private static List<string> ReadMoneyValues(string xlsxPath, string columnName)
        {
            var worksheetXml = ReadWorksheetXml(xlsxPath);
            var letter = InlineStringCells.ResolveColumnLetters(worksheetXml)[columnName];
            var pattern = new Regex($"""<c r="{letter}[0-9]+"[^>]*><v>(?<value>[^<]*)</v></c>""");

            return [.. pattern.Matches(worksheetXml).Select(match => match.Groups["value"].Value)];
        }

        /// <summary>
        /// <see cref="XlsxAnonymiserRun.Run"/> with the two new switches
        /// defaulted, so every test that predates them stays unchanged.
        /// </summary>
        private static XlsxAnonymiserRunResult Anonymise(
            IReadOnlyList<string> inputPaths, string outputDirectory, bool force, decimal scale = 1.0m, int? maxRows = null)
            => XlsxAnonymiserRun.Run(inputPaths, outputDirectory, force, scale, maxRows);

        private static byte[] BuildValidWorkbook()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            return XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [Row("Bakery", "01.03.2026"), Row("Landlord", "02.03.2026")]);
        }

        private string WriteInput(byte[] content, string fileName = "export.xlsx")
        {
            var path = Path.Combine(_root, fileName);
            File.WriteAllBytes(path, content);

            return path;
        }

        private static string ReadWorksheetXml(string xlsxPath)
        {
            using var archive = ZipFile.OpenRead(xlsxPath);

            return new StreamReader(archive.GetEntry("xl/worksheets/sheet1.xml")!.Open()).ReadToEnd();
        }

        private static List<string> ReadColumnValues(string xlsxPath, string columnName)
        {
            var worksheetXml = ReadWorksheetXml(xlsxPath);
            var letter = InlineStringCells.ResolveColumnLetters(worksheetXml)[columnName];

            return [.. InlineStringCells.CollectValues(worksheetXml, new HashSet<string>([letter]))[letter]];
        }

        private static byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

        private static byte[] ReadAllBytes(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            using var buffer = new MemoryStream();

            stream.CopyTo(buffer);

            return buffer.ToArray();
        }
    }
}
