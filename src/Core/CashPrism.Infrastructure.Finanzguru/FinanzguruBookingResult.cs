using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The outcome of projecting one row onto a booking: either the booking, or
/// everything that is wrong with the row.
/// </summary>
/// <param name="Value">The booking, or <c>null</c> on failure.</param>
/// <param name="Errors">
/// What is wrong with the row, one entry per problem. Empty on success.
/// </param>
public sealed record FinanzguruBookingResult(Booking? Value, IReadOnlyList<ImportError> Errors)
{
    /// <summary>Whether the row describes a booking.</summary>
    public bool IsSuccess => Value is not null;

    /// <summary>A successful result carrying <paramref name="booking"/>.</summary>
    /// <param name="booking">The booking the row describes.</param>
    public static FinanzguruBookingResult Success(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        return new FinanzguruBookingResult(booking, Errors: []);
    }

    /// <summary>A failed result carrying <paramref name="errors"/>.</summary>
    /// <param name="errors">What is wrong with the row. At least one entry.</param>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static FinanzguruBookingResult Failure(IReadOnlyList<ImportError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("A rejected row has to say what is wrong.", nameof(errors));
        }

        return new FinanzguruBookingResult(Value: null, errors);
    }
}
