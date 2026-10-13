using CashPrism.Domain.Contracts;

namespace CashPrism.Domain.Tests.Unit.Contracts;

public sealed class DismissedOverdueTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Refuses_A_Dismissal_Without_A_Contract()
            => Assert.Throws<ArgumentException>(() => new DismissedOverdue(" ", new DateOnly(2026, 10, 1)));
    }
}
