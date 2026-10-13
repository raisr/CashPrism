using CashPrism.Application.Contracts;
using CashPrism.TestSupport.Contracts;

namespace CashPrism.Application.Tests.Unit.Contracts;

public sealed class OverdueDismisserTests
{
    public sealed class DismissAsync
    {
        [Fact]
        public async Task Stores_The_Contract_With_Its_Expected_Date()
        {
            var store = new FakeDismissedOverdueStore();

            await new OverdueDismisser(store).DismissAsync("0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f", new DateOnly(2026, 10, 1));

            Assert.Equal(
                [("0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f", new DateOnly(2026, 10, 1))],
                store.Stored.Select(dismissal => (dismissal.ContractId, dismissal.ExpectedOn)));
        }
    }
}
