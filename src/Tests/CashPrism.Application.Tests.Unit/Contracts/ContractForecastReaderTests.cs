using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using CashPrism.TestSupport.Contracts;

namespace CashPrism.Application.Tests.Unit.Contracts;

public sealed class ContractForecastReaderTests
{
    private const string AContract = "0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f";

    // A monthly contract last booked on 1 September is expected on 1 October
    // and ten days overdue on 11 October.
    private static readonly DateOnly ExportedOn = new(2026, 10, 11);
    private static readonly DateOnly ExpectedOn = new(2026, 10, 1);

    private static Contract Overdue()
        => new(AContract, "monatlich", "Stadtwerke", "Wohnen", "Strom", -9_000, "EUR", new DateTime(2026, 9, 1), false);

    private static async Task<ContractForecast> ReadOneAsync(params DismissedOverdue[] dismissals)
    {
        var forecasts = await new ContractForecastReader(
            new FakeContractReader(ExportedOn, Overdue()),
            new FakeDismissedOverdueStore(dismissals)).ReadAsync();

        return forecasts!.Contracts.Single();
    }

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            var forecasts = await new ContractForecastReader(
                new FakeContractReader(currentTo: null),
                new FakeDismissedOverdueStore()).ReadAsync();

            Assert.Null(forecasts);
        }

        [Fact]
        public async Task Gives_A_Contract_Its_Next_Booking_And_Days_Overdue()
        {
            var forecast = await ReadOneAsync();

            Assert.Equal((ExpectedOn, 10), (forecast.NextBookingOn, forecast.DaysOverdue));
        }

        [Fact]
        public async Task Hides_The_Marker_Dismissed_For_The_Expected_Date()
        {
            var forecast = await ReadOneAsync(new DismissedOverdue(AContract, ExpectedOn));

            Assert.Equal((ExpectedOn, (int?)null), (forecast.NextBookingOn, forecast.DaysOverdue));
        }

        [Fact]
        public async Task Shows_The_Marker_Dismissed_For_An_Earlier_Date()
        {
            var forecast = await ReadOneAsync(new DismissedOverdue(AContract, new DateOnly(2026, 9, 1)));

            Assert.Equal(10, forecast.DaysOverdue);
        }
    }
}
