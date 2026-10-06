namespace CashPrism.DemoData.Generation;

/// <summary>A recognised contract a booking belongs to.</summary>
/// <param name="Id">What <c>Analyse-Vertrags-ID</c> carries.</param>
/// <param name="Interval">How often the contract recurs.</param>
public sealed record DemoContract(string Id, DemoInterval Interval);
