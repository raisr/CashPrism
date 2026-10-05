using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Projects one row of a FinanzGuru export onto a <see cref="Booking"/>: 23 of
/// the export's 29 columns, with the German yes/no and split words translated.
/// </summary>
/// <remarks>
/// The required values are checked here rather than left to the model's guard
/// clauses. Both would reject the same row, but a guard clause throws, and a row
/// that is missing a value is an expected property of a file a person picked —
/// so it is reported as a result and the import can name every bad row at once
/// instead of stopping at the first.
/// </remarks>
public static class FinanzguruBooking
{
    /// <summary>
    /// Projects <paramref name="row"/>.
    /// </summary>
    /// <param name="row">The row to project.</param>
    /// <param name="importRunId">The run the booking's state comes from.</param>
    /// <returns>The booking, or every reason this row does not describe one.</returns>
    /// <exception cref="ArgumentException"><paramref name="importRunId"/> is empty.</exception>
    public static FinanzguruBookingResult Create(FinanzguruExportRow row, Guid importRunId)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (importRunId == Guid.Empty)
        {
            throw new ArgumentException("A booking needs the run it came from.", nameof(importRunId));
        }

        var errors = new List<ImportError>();

        var fingerprint = Required(row, FinanzguruColumns.BookingId, errors);
        var currency = Required(row, FinanzguruColumns.Currency, errors);
        var accountReference = Required(row, FinanzguruColumns.AccountReference, errors);

        var isTransfer = Flag(row, FinanzguruColumns.IsInternalTransfer, errors);
        var isContract = Flag(row, FinanzguruColumns.IsContract, errors);
        var isExcludedFromDisposableIncome = Flag(row, FinanzguruColumns.ExcludedFromDisposableIncome, errors);

        var splitRole = FinanzguruSplitType.Parse(
            Value(row, FinanzguruColumns.SplitType),
            row.RowNumber);

        if (!splitRole.IsSuccess)
        {
            errors.Add(splitRole.Error!);
        }

        var originalFingerprint = Value(row, FinanzguruColumns.OriginalReferenceId);
        originalFingerprint = string.IsNullOrWhiteSpace(originalFingerprint) ? null : originalFingerprint;

        if (splitRole.IsSuccess
            && splitRole.Value is SplitRole.Part or SplitRole.Remainder
            && originalFingerprint is null)
        {
            errors.Add(ImportError.SplitPartWithoutOriginal(
                FinanzguruColumns.SplitType,
                row.RowNumber,
                FinanzguruColumns.OriginalReferenceId));
        }

        if (errors.Count > 0)
        {
            return FinanzguruBookingResult.Failure(errors);
        }

        return FinanzguruBookingResult.Success(new Booking(
            fingerprint!,
            row.BookedOn,
            row.AmountInCents,
            currency!,
            accountReference!,
            accountName: Value(row, FinanzguruColumns.AccountName),
            counterparty: Value(row, FinanzguruColumns.Counterparty),
            counterpartyAccount: Value(row, FinanzguruColumns.CounterpartyIban),
            paymentReference: Value(row, FinanzguruColumns.PaymentReference),
            category: Value(row, FinanzguruColumns.MainCategory),
            subCategory: Value(row, FinanzguruColumns.SubCategory),
            isTransfer!.Value,
            splitRole.Value!.Value,
            originalFingerprint,
            row.BalanceInCents,
            transactionKind: Value(row, FinanzguruColumns.TransactionKind),
            isContract!.Value,
            contractInterval: Value(row, FinanzguruColumns.ContractInterval),
            contractId: Value(row, FinanzguruColumns.ContractId),
            isExcludedFromDisposableIncome!.Value,
            mandateReference: Value(row, FinanzguruColumns.MandateReference),
            creditorId: Value(row, FinanzguruColumns.CreditorId),
            tags: Value(row, FinanzguruColumns.Tags),
            importRunId));
    }

    private static string Value(FinanzguruExportRow row, string column)
        => row.Values.TryGetValue(column, out var value) ? value : string.Empty;

    private static bool? Flag(FinanzguruExportRow row, string column, List<ImportError> errors)
    {
        var flag = FinanzguruFlag.Parse(Value(row, column), column, row.RowNumber);

        if (flag.IsSuccess)
        {
            return flag.Value;
        }

        errors.Add(flag.Error!);

        return null;
    }

    private static string? Required(FinanzguruExportRow row, string column, List<ImportError> errors)
    {
        var value = Value(row, column);

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        errors.Add(ImportError.EmptyValue(column, row.RowNumber));

        return null;
    }
}
