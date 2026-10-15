using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;

namespace CashPrism.Application.Imports;

/// <summary>
/// What one batch of an import changes. The use case decides, the store writes —
/// and it writes a batch at a time rather than a file at a time, because an
/// export carries the owner's whole history and always will.
/// </summary>
/// <param name="BookingsToInsert">Bookings not stored before.</param>
/// <param name="BookingsToUpdate">
/// Bookings already stored, to be replaced by this later state. The fingerprint
/// identifies which stored booking each one replaces.
/// </param>
/// <param name="RawRowsToStore">
/// The rows behind those two lists, one per booking. An unchanged booking
/// contributes none.
/// </param>
public sealed record ImportBatch(
    IReadOnlyList<Booking> BookingsToInsert,
    IReadOnlyList<Booking> BookingsToUpdate,
    IReadOnlyList<RawRow> RawRowsToStore);
