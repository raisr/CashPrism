using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Stores a money amount as whole cents in an integer column.
/// </summary>
/// <remarks>
/// <para>
/// SQLite has no decimal type. Left alone, EF Core keeps a <see cref="decimal"/>
/// as text, and text compares lexicographically: <c>ORDER BY</c> on an amount and
/// <c>SUM</c> over one both return nonsense, which for an application built to add
/// up bookings is not a trade-off but a defect.
/// </para>
/// <para>
/// Every amount in a FinanzGuru export carries exactly two decimal places (see
/// <c>docs/finanzguru-export.md</c>), so cents lose nothing. The conversion is
/// deliberately not silent about the rest: an amount with more precision than that
/// would be rounded, and rounding money quietly is how a cent goes missing, so it
/// is rejected instead.
/// </para>
/// </remarks>
public sealed class MoneyConverter : ValueConverter<decimal, long>
{
    private const int CentsPerUnit = 100;

    /// <summary>Creates the converter.</summary>
    public MoneyConverter()
        : base(amount => ToCents(amount), cents => cents / (decimal)CentsPerUnit)
    {
    }

    private static long ToCents(decimal amount)
    {
        var cents = amount * CentsPerUnit;

        if (cents != decimal.Truncate(cents))
        {
            throw new InvalidOperationException(
                $"The amount {amount} has more precision than cents and would be rounded on the way into the database.");
        }

        return (long)cents;
    }
}
