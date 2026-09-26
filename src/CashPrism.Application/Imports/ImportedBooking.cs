using CashPrism.Domain.Bookings;

namespace CashPrism.Application.Imports;

/// <summary>
/// One row of an export, as the import use case needs it: the booking the row
/// projects to, and the row itself.
/// </summary>
/// <param name="Booking">The booking the row describes.</param>
/// <param name="RawJson">
/// The row as a JSON object of the export's columns, verbatim. It is what a
/// stored raw row holds, and comparing it with the stored one is what decides
/// whether anything about this booking changed — so the source has to render it
/// deterministically, or an unchanged row looks like a changed one.
/// </param>
/// <param name="RowNumber">
/// The one-based row of the source file, so a message can point at it.
/// </param>
public sealed record ImportedBooking(Booking Booking, string RawJson, int RowNumber);
