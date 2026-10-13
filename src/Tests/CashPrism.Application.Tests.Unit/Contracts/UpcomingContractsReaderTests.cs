using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using CashPrism.TestSupport.Contracts;

namespace CashPrism.Application.Tests.Unit.Contracts;

public sealed class UpcomingContractsReaderTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 13);

    /// <summary>A monthly contract whose next booking is expected on <paramref name="expectedOn"/>.</summary>
    private static Contract Expecting(string counterparty, DateOnly expectedOn, string? contractId = null)
        => new(
            contractId ?? Guid.NewGuid().ToString("N"),
            "monatlich",
            counterparty,
            "Sonstiges",
            "Sonstige Ausgaben",
            -1_000,
            "EUR",
            expectedOn.AddMonths(-1).ToDateTime(TimeOnly.MinValue),
            false);

    private static async Task<IReadOnlyList<string>> ReadNamesAsync(Contract[] contracts, params DismissedOverdue[] dismissals)
    {
        var upcoming = await new UpcomingContractsReader(new ContractForecastReader(
            new FakeContractReader(ExportedOn, contracts),
            new FakeDismissedOverdueStore(dismissals))).ReadAsync();

        return [.. upcoming!.Contracts.Select(forecast => forecast.Contract.Counterparty)];
    }

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            var upcoming = await new UpcomingContractsReader(new ContractForecastReader(
                new FakeContractReader(currentTo: null),
                new FakeDismissedOverdueStore())).ReadAsync();

            Assert.Null(upcoming);
        }

        [Fact]
        public async Task Lists_A_Booking_Expected_Thirty_Days_After_The_Export_Date()
            => Assert.Equal(["Strom"], await ReadNamesAsync([Expecting("Strom", ExportedOn.AddDays(30))]));

        [Fact]
        public async Task Leaves_Out_A_Booking_Expected_Thirty_One_Days_After_The_Export_Date()
            => Assert.Empty(await ReadNamesAsync([Expecting("Strom", ExportedOn.AddDays(31))]));

        [Fact]
        public async Task Lists_A_Booking_Due_But_Not_Yet_Overdue()
            => Assert.Equal(["Strom"], await ReadNamesAsync([Expecting("Strom", ExportedOn.AddDays(-Contract.OverdueToleranceInDays))]));

        [Fact]
        public async Task Lists_An_Overdue_Booking()
            => Assert.Equal(["Strom"], await ReadNamesAsync([Expecting("Strom", ExportedOn.AddDays(-20))]));

        [Fact]
        public async Task Leaves_Out_An_Overdue_Booking_Whose_Marker_Was_Dismissed()
        {
            var expectedOn = ExportedOn.AddDays(-20);

            Assert.Empty(await ReadNamesAsync(
                [Expecting("Strom", expectedOn, "0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f")],
                new DismissedOverdue("0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f", expectedOn)));
        }

        [Fact]
        public async Task Lists_Overdue_Bookings_First_The_Longest_Overdue_On_Top_Then_The_Rest_By_Date()
            => Assert.Equal(
                ["Lange überfällig", "Überfällig", "Bald", "Später"],
                await ReadNamesAsync(
                [
                    Expecting("Später", ExportedOn.AddDays(20)),
                    Expecting("Überfällig", ExportedOn.AddDays(-7)),
                    Expecting("Bald", ExportedOn.AddDays(2)),
                    Expecting("Lange überfällig", ExportedOn.AddDays(-14)),
                ]));
    }
}
