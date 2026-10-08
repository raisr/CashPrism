using CashPrism.Application.CashFlow;
using CashPrism.Domain.CashFlow;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Sums income and spending in the database. The counting rule is the domain's
/// own expression, so the query and every other analysis count the same
/// bookings.
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
        // maximum translatable — see ImportRunConfiguration.
        var importedAt = await runs
            .OrderByDescending(run => run.ImportedAt)
            .Select(run => (DateTimeOffset?)run.ImportedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return importedAt is { } latest ? DateOnly.FromDateTime(latest.UtcDateTime) : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonthlyCashFlow>> ReadMonthsAsync(
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default)
    {
        var months = await Entries(firstMonth, lastMonth)
            .GroupBy(entry => new { entry.BookedOn.Year, entry.BookedOn.Month })
            .Select(month => new
            {
                month.Key.Year,
                month.Key.Month,
                IncomeInCents = month.Sum(entry => entry.IncomeInCents),
                SpendingInCents = month.Sum(entry => entry.SpendingInCents),
            })
            .OrderBy(month => month.Year)
            .ThenBy(month => month.Month)
            .ToListAsync(cancellationToken);

        return months
            .Select(month => new MonthlyCashFlow(
                new DateOnly(month.Year, month.Month, 1),
                month.IncomeInCents,
                month.SpendingInCents))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategorySpending>> ReadSpendingByCategoryAsync(
        DateOnly month,
        CancellationToken cancellationToken = default)
    {
        var categories = await Entries(month, month)
            .GroupBy(entry => entry.Category)
            .Select(category => new
            {
                Category = category.Key,
                SpendingInCents = category.Sum(entry => entry.SpendingInCents),
            })
            .Where(category => category.SpendingInCents > 0)
            .OrderByDescending(category => category.SpendingInCents)
            .ThenBy(category => category.Category)
            .ToListAsync(cancellationToken);

        return categories
            .Select(category => new CategorySpending(category.Category, category.SpendingInCents))
            .ToList();
    }

    /// <summary>
    /// The counted bookings of the months from <paramref name="firstMonth"/> to
    /// <paramref name="lastMonth"/>, as what each contributes. The range is
    /// half-open on the day after the last month, because a booking date may
    /// carry a time and the last day of a month would otherwise lose the
    /// bookings posted after midnight.
    /// </summary>
    private IQueryable<CashFlowEntry> Entries(DateOnly firstMonth, DateOnly lastMonth)
    {
        var from = firstMonth.ToDateTime(TimeOnly.MinValue);
        var until = lastMonth.AddMonths(1).ToDateTime(TimeOnly.MinValue);

        return context.Bookings
            .AsNoTracking()
            .Where(CashFlowRule.Counts)
            .Where(booking => booking.BookedOn >= from && booking.BookedOn < until)
            .Select(CashFlowRule.ToEntry);
    }
}
