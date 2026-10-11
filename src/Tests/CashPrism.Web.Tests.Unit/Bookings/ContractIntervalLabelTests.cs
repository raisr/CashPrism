using CashPrism.Web.Bookings;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// Every contract interval a measured export contains, spelled as Finanzguru
/// spells it.
/// </summary>
public sealed class ContractIntervalLabelTests
{
    public sealed class KeyFor
    {
        public static TheoryData<string, string> KnownIntervals => new()
        {
            { "monatlich", "ContractIntervalMonthly" },
            { "zweimonatlich", "ContractIntervalBiMonthly" },
            { "vierteljaehrlich", "ContractIntervalQuarterly" },
            { "halbjaehrlich", "ContractIntervalHalfYearly" },
            { "jaehrlich", "ContractIntervalYearly" },
        };

        [Theory]
        [MemberData(nameof(KnownIntervals))]
        public void Names_The_Resource_Key_Of_A_Known_Interval(string contractInterval, string key)
            => Assert.Equal(key, ContractIntervalLabel.KeyFor(contractInterval));

        [Fact]
        public void Has_No_Key_For_An_Interval_It_Does_Not_Know()
            => Assert.Null(ContractIntervalLabel.KeyFor("woechentlich"));
    }
}
