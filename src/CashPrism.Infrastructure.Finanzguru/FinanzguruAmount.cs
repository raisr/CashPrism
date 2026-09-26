using System.Globalization;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Reads the money columns of a FinanzGuru export. The export stores them as
/// ordinary numeric cells and only the number format makes them read as money,
/// so something has to turn that number into the whole cents the rest of
/// CashPrism holds money in — and it is this layer, because a spreadsheet cell
/// is a property of the export, not of a booking.
/// </summary>
/// <remarks>
/// <para>
/// The conversion goes through <see cref="decimal"/> rather than multiplying the
/// <see cref="double"/> by 100 directly. A binary float cannot hold 1234.56
/// exactly, and the nearest double times 100 lands just beside the whole cent;
/// converting to decimal first rounds to the 15 significant digits the double
/// actually carries, which is the value the spreadsheet meant.
/// </para>
/// <para>
/// The conversion is strict on purpose, for the same reason
/// <see cref="FinanzguruFlag"/> is. Every money cell in both measured exports
/// carries exactly two decimal places; a third one is a change to the export we
/// want to be told about, and rounding it away would instead turn it into a
/// silently wrong total.
/// </para>
/// </remarks>
public static class FinanzguruAmount
{
    private const decimal CentsPerUnit = 100m;

    /// <summary>
    /// Converts one money cell to whole cents. A value that is not a whole
    /// number of cents fails and says where.
    /// </summary>
    /// <param name="value">The cell's value, as the export stores it.</param>
    /// <param name="column">The header name of the column, for the failure message.</param>
    /// <param name="row">The one-based worksheet row the cell sits in, for the failure message.</param>
    /// <returns>
    /// A successful result carrying the amount in whole cents, or a failure
    /// naming the column and the row. See <see cref="FinanzguruAmountResult"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="column"/> is missing.</exception>
    public static FinanzguruAmountResult ToCents(double value, string column, int row)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return FinanzguruAmountResult.Failure(
                $"Column '{column}' in row {row} carries '{value.ToString(CultureInfo.InvariantCulture)}', which is not an amount.");
        }

        decimal units;

        try
        {
            units = (decimal)value;
        }
        catch (OverflowException)
        {
            return FinanzguruAmountResult.Failure(
                $"Column '{column}' in row {row} carries '{value.ToString(CultureInfo.InvariantCulture)}', which is too large to be an amount.");
        }

        var cents = units * CentsPerUnit;
        var wholeCents = decimal.Truncate(cents);

        if (cents != wholeCents)
        {
            return FinanzguruAmountResult.Failure(
                $"Column '{column}' in row {row} carries '{units.ToString(CultureInfo.InvariantCulture)}', "
                + "which is not a whole number of cents; expected at most two decimal places.");
        }

        if (wholeCents < long.MinValue || wholeCents > long.MaxValue)
        {
            return FinanzguruAmountResult.Failure(
                $"Column '{column}' in row {row} carries '{units.ToString(CultureInfo.InvariantCulture)}', which is too large to be an amount.");
        }

        return FinanzguruAmountResult.Success((long)wholeCents);
    }
}
