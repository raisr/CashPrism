using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Reads the balance movements out of the database. Every booking is read,
/// because checking a month end may need the last booking day long before the
/// months shown; five small columns per booking keep that light.
/// </summary>
public sealed class BalanceMovementReader : IBalanceMovementReader
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    /// <param name="context">The database.</param>
    public BalanceMovementReader(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BalanceMovement>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await context.Bookings
            .AsNoTracking()
            .Select(booking => new
            {
                booking.AccountReference,
                booking.BookedOn,
                booking.AmountInCents,
                booking.SplitRole,
                booking.ReportedBalanceInCents,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new BalanceMovement(
                row.AccountReference,
                DateOnly.FromDateTime(row.BookedOn),
                row.AmountInCents,
                row.SplitRole,
                row.ReportedBalanceInCents))
            .ToList();
    }
}
