using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Reads the contracts out of the stored bookings.
/// </summary>
public sealed class ContractReader : IContractReader
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    /// <param name="context">The database.</param>
    public ContractReader(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default)
        => CurrentToQuery.ReadAsync(context, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// The newest booking per contract is picked in memory: only contract
    /// bookings are loaded, and only the columns a contract is described by,
    /// which stays small next to a translated "first row per group" query
    /// SQLite would have to run as a window function. The fingerprint breaks a
    /// tie on the same day, so the same data always yields the same contract.
    /// </remarks>
    public async Task<IReadOnlyList<Contract>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await context.Bookings
            .AsNoTracking()
            .Where(booking => booking.ContractId != "")
            .Select(booking => new
            {
                booking.ContractId,
                booking.ContractInterval,
                booking.Counterparty,
                booking.Category,
                booking.SubCategory,
                booking.AmountInCents,
                booking.Currency,
                booking.BookedOn,
                booking.IsTransfer,
                booking.Fingerprint,
            })
            .ToListAsync(cancellationToken);

        return [.. rows
            .GroupBy(row => row.ContractId, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(row => row.BookedOn)
                .ThenByDescending(row => row.Fingerprint, StringComparer.Ordinal)
                .First())
            .Select(row => new Contract(
                row.ContractId,
                row.ContractInterval,
                row.Counterparty,
                row.Category,
                row.SubCategory,
                row.AmountInCents,
                row.Currency,
                row.BookedOn,
                row.IsTransfer))];
    }
}
