using System.Globalization;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// The demo export as a whole: three years of a fictional household's
/// bookings, ending on a given day. The same day always gives the same rows.
/// </summary>
public static class DemoExport
{
    /// <summary>
    /// The seed of the random number generator. Fixed, so a run is repeatable;
    /// changing it changes every amount and every committed demo file with it.
    /// </summary>
    public const int Seed = 114;

    /// <summary>The rows of the demo export that ends on <paramref name="until"/>, newest first.</summary>
    public static IReadOnlyList<DemoRow> Generate(DateOnly until)
    {
        var plan = new DemoPlan(until, Seed);

        return DemoLedger.Rows(
        [
            .. ContractBookings.Plan(plan),
            .. EverydayBookings.Plan(plan),
            .. OneOffBookings.Plan(plan),
        ]);
    }

    /// <summary>The worksheet name an export taken on <paramref name="until"/> carries.</summary>
    public static string SheetName(DateOnly until)
        => until.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + FinanzguruSheetName.Suffix;
}
