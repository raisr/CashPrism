using CashPrism.Application.Access;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class LoginThrottleTests
{
    private static void Fail(LoginThrottle throttle, int times)
    {
        for (var attempt = 0; attempt < times; attempt++)
        {
            throttle.RecordFailure();
        }
    }

    public sealed class LockedUntil
    {
        [Fact]
        public void Accepts_Logins_After_Four_Wrong_Passwords()
        {
            var throttle = new LoginThrottle(new SteppingClock());
            Fail(throttle, 4);

            Assert.Null(throttle.LockedUntil());
        }

        [Fact]
        public void Locks_For_One_Minute_After_Five_Wrong_Passwords()
        {
            var clock = new SteppingClock();
            var throttle = new LoginThrottle(clock);
            Fail(throttle, 5);

            Assert.Equal(clock.UtcNow + TimeSpan.FromMinutes(1), throttle.LockedUntil());
        }

        [Fact]
        public void Accepts_Logins_Again_Once_The_Minute_Is_Over()
        {
            var clock = new SteppingClock();
            var throttle = new LoginThrottle(clock);
            Fail(throttle, 5);

            clock.Advance(TimeSpan.FromMinutes(1));

            Assert.Null(throttle.LockedUntil());
        }

        [Fact]
        public void Locks_For_Two_Minutes_After_Five_More_Wrong_Passwords()
        {
            var clock = new SteppingClock();
            var throttle = new LoginThrottle(clock);
            Fail(throttle, 5);
            clock.Advance(TimeSpan.FromMinutes(1));

            Fail(throttle, 5);

            Assert.Equal(clock.UtcNow + TimeSpan.FromMinutes(2), throttle.LockedUntil());
        }

        [Fact]
        public void Locks_For_No_Longer_Than_Fifteen_Minutes()
        {
            var clock = new SteppingClock();
            var throttle = new LoginThrottle(clock);
            for (var lockout = 0; lockout < 6; lockout++)
            {
                Fail(throttle, 5);
                clock.Advance(LoginThrottle.LongestLockout);
            }

            Fail(throttle, 5);

            Assert.Equal(clock.UtcNow + TimeSpan.FromMinutes(15), throttle.LockedUntil());
        }

        [Fact]
        public void Does_Not_Count_Wrong_Passwords_During_A_Lockout()
        {
            var clock = new SteppingClock();
            var throttle = new LoginThrottle(clock);
            Fail(throttle, 5);
            Fail(throttle, 5);
            clock.Advance(TimeSpan.FromMinutes(1));

            Fail(throttle, 4);

            Assert.Null(throttle.LockedUntil());
        }

        [Fact]
        public void Starts_Over_At_One_Minute_After_A_Correct_Password()
        {
            var clock = new SteppingClock();
            var throttle = new LoginThrottle(clock);
            Fail(throttle, 5);
            clock.Advance(TimeSpan.FromMinutes(1));
            throttle.RecordSuccess();

            Fail(throttle, 5);

            Assert.Equal(clock.UtcNow + TimeSpan.FromMinutes(1), throttle.LockedUntil());
        }

        [Fact]
        public void Forgets_Wrong_Passwords_Before_A_Correct_One()
        {
            var throttle = new LoginThrottle(new SteppingClock());
            Fail(throttle, 4);
            throttle.RecordSuccess();

            Fail(throttle, 4);

            Assert.Null(throttle.LockedUntil());
        }
    }
}
