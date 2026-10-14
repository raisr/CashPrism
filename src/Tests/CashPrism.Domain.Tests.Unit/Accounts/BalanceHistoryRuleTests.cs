using CashPrism.Domain.Accounts;
using CashPrism.Domain.Bookings;

namespace CashPrism.Domain.Tests.Unit.Accounts;

public sealed class BalanceHistoryRuleTests
{
    private const string Giro = "DE02120300000000202051";

    private static readonly DateOnly ExportDay = new(2026, 10, 5);
    private static readonly DateOnly EndOfAugust = new(2026, 8, 31);
    private static readonly DateOnly EndOfSeptember = new(2026, 9, 30);

    private static BalanceMovement Move(
        DateOnly bookedOn,
        long amountInCents,
        long reportedBalanceInCents,
        SplitRole splitRole = SplitRole.None)
        => new(Giro, bookedOn, amountInCents, splitRole, reportedBalanceInCents);

    public sealed class For
    {
        [Fact]
        public void Subtracts_Every_Booking_After_The_Day_Up_To_The_Export()
        {
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [
                    Move(new DateOnly(2026, 10, 2), -20000, 100000),
                    Move(new DateOnly(2026, 9, 30), 50000, 120000),
                    Move(new DateOnly(2026, 9, 12), -10000, 70000),
                ],
                [EndOfSeptember, new DateOnly(2026, 9, 29)]);

            Assert.Equal([120000L, 70000L], history.BalancesInCents);
        }

        [Fact]
        public void Carries_The_Balance_Through_A_Month_Without_Bookings()
        {
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [
                    Move(new DateOnly(2026, 10, 2), -20000, 100000),
                    Move(new DateOnly(2026, 8, 20), 50000, 120000),
                ],
                [EndOfAugust, EndOfSeptember]);

            Assert.Equal([120000L, 120000L], history.BalancesInCents);
        }

        [Fact]
        public void Counts_A_Split_Booking_Once_Through_Its_Parts()
        {
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [
                    Move(new DateOnly(2026, 10, 2), -10000, 100000, SplitRole.Original),
                    Move(new DateOnly(2026, 10, 2), -6000, 100000, SplitRole.Part),
                    Move(new DateOnly(2026, 10, 2), -4000, 100000, SplitRole.Remainder),
                    Move(new DateOnly(2026, 9, 12), -500, 110000),
                ],
                [EndOfSeptember]);

            Assert.Equal([110000L], history.BalancesInCents);
        }

        [Fact]
        public void Leaves_Out_A_Booking_Dated_After_The_Export()
        {
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [
                    Move(new DateOnly(2026, 10, 7), -30000, 70000),
                    Move(new DateOnly(2026, 10, 2), -20000, 100000),
                    Move(new DateOnly(2026, 9, 12), 500, 120000),
                ],
                [EndOfSeptember, ExportDay]);

            Assert.Equal([120000L, 100000L], history.BalancesInCents);
        }

        [Fact]
        public void Gives_No_Balance_For_A_Day_Before_The_First_Booking()
        {
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [Move(new DateOnly(2026, 9, 12), 100000, 100000)],
                [EndOfAugust, EndOfSeptember]);

            Assert.Equal([null, 100000L], history.BalancesInCents);
        }

        [Fact]
        public void Accepts_The_Balance_Of_Any_Row_On_The_Last_Booking_Day()
        {
            // Two rows on one day: the stored bookings do not keep the order
            // that says which of them closed the day.
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [
                    Move(new DateOnly(2026, 10, 2), -20000, 100000),
                    Move(new DateOnly(2026, 9, 30), -1000, 121000),
                    Move(new DateOnly(2026, 9, 30), -1000, 120000),
                ],
                [EndOfSeptember]);

            Assert.True(history.Holds);
        }

        [Fact]
        public void Has_No_History_When_One_Day_Disagrees_With_The_Export()
        {
            var history = BalanceHistoryRule.For(
                100000,
                ExportDay,
                [
                    Move(new DateOnly(2026, 10, 2), -20000, 100000),
                    Move(new DateOnly(2026, 9, 30), 50000, 120000),
                    Move(new DateOnly(2026, 8, 12), -10000, 99999),
                ],
                [EndOfAugust, EndOfSeptember]);

            Assert.Equal(BalanceHistory.DoesNotHold, history);
        }
    }
}
