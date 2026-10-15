using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// Reads the contracts the stored bookings carry.
/// </summary>
public interface IContractReader
{
    /// <summary>
    /// The date the stored data is current to: the latest export date among
    /// the import runs, or the day of the latest import where no run carries
    /// an export date. <see langword="null"/> when nothing was imported.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Every contract identifier the stored bookings carry, each described by
    /// its newest booking, in no particular order. Where two bookings of a
    /// contract share the newest day, either may describe it.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<Contract>> ReadAllAsync(CancellationToken cancellationToken = default);
}
