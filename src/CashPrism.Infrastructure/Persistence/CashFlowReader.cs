using CashPrism.Application.CashFlow;
using CashPrism.Domain.CashFlow;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Sums the counted bookings in the database. Which bookings count is the
/// domain's own expression, so the query and every other analysis count the
/// same bookings.
/// </summary>
public sealed class CashFlowReader : ICashFlowReader
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    /// <param name="context">The database.</param>
    public CashFlowReader(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default)
    {
        var runs = context.ImportRuns.AsNoTracking();

        var exportedOn = await runs.MaxAsync(run => run.ExportedOn, cancellationToken);

        if (exportedOn.HasValue)
        {
            return exportedOn;
        }

        // ImportedAt is stored as a UTC DateTime, which is what makes the
        // order translatable — see ImportRunConfiguration.
        var importedAt = await runs
            .OrderByDescending(run => run.ImportedAt)
            .Select(run => (DateTimeOffset?)run.ImportedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return importedAt is { } latest ? DateOnly.FromDateTime(latest.UtcDateTime) : null;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The range is half-open on the day after the last month, because a
    /// booking date may carry a time and the last day of a month would
    /// otherwise lose the bookings posted after midnight.
    /// </remarks>
    public async Task<IReadOnlyList<CategoryNet>> ReadCategoryNetsAsync(
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default)
    {
        var from = firstMonth.ToDateTime(TimeOnly.MinValue);
        var until = lastMonth.AddMonths(1).ToDateTime(TimeOnly.MinValue);

        var nets = await context.Bookings
            .AsNoTracking()
            .Where(CashFlowRule.Counts)
            .Where(booking => booking.BookedOn >= from && booking.BookedOn < until)
            .GroupBy(booking => new { booking.BookedOn.Year, booking.BookedOn.Month, booking.Category })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                group.Key.Category,
                NetInCents = group.Sum(booking => booking.AmountInCents),
            })
            .OrderBy(net => net.Year)
            .ThenBy(net => net.Month)
            .ThenBy(net => net.Category)
            .ToListAsync(cancellationToken);

        return nets
            .Select(net => new CategoryNet(new DateOnly(net.Year, net.Month, 1), net.Category, net.NetInCents))
            .ToList();
    }
}
