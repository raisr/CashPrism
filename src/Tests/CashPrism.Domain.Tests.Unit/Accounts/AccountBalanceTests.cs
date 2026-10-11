using CashPrism.Domain.Accounts;

namespace CashPrism.Domain.Tests.Unit.Accounts;

public sealed class AccountBalanceTests
{
    public sealed class Constructor
    {
        private static AccountBalance Create(Guid? importRunId = null, string accountReference = "DE02120300000000202051", string currency = "EUR")
            => new(importRunId ?? Guid.NewGuid(), accountReference, "Girokonto", 150_000, currency, new DateTime(2026, 10, 5));

        [Fact]
        public void Refuses_A_Balance_Without_Its_Import_Run()
            => Assert.Throws<ArgumentException>(() => Create(importRunId: Guid.Empty));

        [Fact]
        public void Refuses_A_Balance_Without_An_Account()
            => Assert.Throws<ArgumentException>(() => Create(accountReference: " "));

        [Fact]
        public void Refuses_A_Balance_Without_A_Currency()
            => Assert.Throws<ArgumentException>(() => Create(currency: ""));
    }
}
