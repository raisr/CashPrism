using CashPrism.Domain.Imports;

namespace CashPrism.Application.Imports;

/// <summary>
/// What is already stored about one booking: which run its state came from, and
/// what that run's row said. Together they answer the two questions a re-import
/// asks — has anything changed, and is the file in front of us the later one.
/// </summary>
/// <param name="Fingerprint">The booking this state belongs to.</param>
/// <param name="SourceRun">The run whose export the stored state came from.</param>
/// <param name="RawJson">The stored row, for comparison with the incoming one.</param>
public sealed record StoredBookingState(string Fingerprint, ImportRun SourceRun, string RawJson);
