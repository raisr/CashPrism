namespace CashPrism.Application.Paging;

/// <summary>
/// One page of a list, and how long the list is.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
/// <param name="Items">The entries on this page, in the order they were asked for.</param>
/// <param name="TotalCount">
/// How many entries there are in total, not how many this page carries. It is
/// what a pager counts its pages from.
/// </param>
public sealed record Page<T>(IReadOnlyList<T> Items, int TotalCount);
