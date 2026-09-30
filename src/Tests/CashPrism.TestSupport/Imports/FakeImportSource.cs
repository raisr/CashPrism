using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;

namespace CashPrism.TestSupport.Imports;

/// <summary>
/// Hands the use case a prepared export instead of reading a file, so a test can
/// say what the file contained in one line. A row is a fingerprint plus the
/// payload that decides whether it counts as changed; the booking is built around
/// it, because what the projection says is not what these tests are about.
/// </summary>
public sealed class FakeImportSource : IImportSource
{
    private const string SheetName = "20260907_Export_Alle_Buchungen";

    private readonly DateOnly? exportedOn;
    private readonly (string Fingerprint, string Payload)[] rows;
    private readonly IReadOnlyList<string> unknownColumns;
    private readonly ImportError? error;

    private FakeImportSource(
        DateOnly? exportedOn,
        (string Fingerprint, string Payload)[] rows,
        IReadOnlyList<string> unknownColumns,
        ImportError? error)
    {
        this.exportedOn = exportedOn;
        this.rows = rows;
        this.unknownColumns = unknownColumns;
        this.error = error;
    }

    /// <summary>The run id the use case passed in, or <c>null</c> when it never read.</summary>
    public Guid? ReadForRunId { get; private set; }

    /// <summary>A source whose file carries <paramref name="rows"/>.</summary>
    public static FakeImportSource Carrying(
        DateOnly? exportedOn,
        params (string Fingerprint, string Payload)[] rows)
        => new(exportedOn, rows, unknownColumns: [], error: null);

    /// <summary>A source whose file also carries columns we do not know.</summary>
    public static FakeImportSource CarryingUnknownColumns(
        DateOnly exportedOn,
        IReadOnlyList<string> unknownColumns,
        params (string Fingerprint, string Payload)[] rows)
        => new(exportedOn, rows, unknownColumns, error: null);

    /// <summary>A source whose file cannot be read.</summary>
    public static FakeImportSource Failing(ImportError error)
        => new(exportedOn: null, rows: [], unknownColumns: [], error);

    public Task<ImportSourceResult> ReadAsync(
        Stream stream,
        Guid importRunId,
        CancellationToken cancellationToken = default)
    {
        ReadForRunId = importRunId;

        if (error is not null)
        {
            return Task.FromResult(ImportSourceResult.Failure(error));
        }

        var bookings = rows
            .Select((row, index) => new ImportedBooking(
                CreateBooking(row.Fingerprint, importRunId),
                RawJson: row.Payload,
                RowNumber: index + 2))
            .ToArray();

        return Task.FromResult(ImportSourceResult.Success(
            new ImportedData(SheetName, exportedOn, bookings, unknownColumns)));
    }

    private static Booking CreateBooking(string fingerprint, Guid importRunId)
        => new(
            fingerprint,
            new DateTime(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified),
            amountInCents: -6317,
            currency: "EUR",
            accountReference: "DE02120300000000202051",
            accountName: "Girokonto",
            counterparty: "Supermarkt",
            counterpartyAccount: string.Empty,
            paymentReference: string.Empty,
            category: "Lebensmittel",
            subCategory: "Supermarkt",
            isTransfer: false,
            SplitRole.None,
            originalFingerprint: null,
            importRunId);
}
