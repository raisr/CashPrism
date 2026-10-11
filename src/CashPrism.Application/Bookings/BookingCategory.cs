namespace CashPrism.Application.Bookings;

/// <summary>A main category the booking list can be narrowed to, with the sub-categories it holds.</summary>
/// <param name="Name">Finanzguru's main category, as the export carries it.</param>
/// <param name="SubCategories">The sub-categories stored under it, in ordinal order.</param>
public sealed record BookingCategory(string Name, IReadOnlyList<string> SubCategories);
