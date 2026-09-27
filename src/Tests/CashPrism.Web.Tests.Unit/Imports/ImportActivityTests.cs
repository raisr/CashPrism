using CashPrism.Web.Imports;

namespace CashPrism.Web.Tests.Unit.Imports;

public sealed class ImportActivityTests
{
    private static ImportFeedbackMessage AMessage(string headline = "fertig")
        => new(ImportFeedbackSeverity.Success, headline, Details: []);

    public sealed class RunAsync
    {
        [Fact]
        public async Task Runs_The_Import_And_Keeps_What_It_Said()
        {
            var activity = new ImportActivity();

            var started = await activity.RunAsync(() => Task.FromResult(AMessage("fertig")));

            Assert.True(started);
            Assert.Equal("fertig", activity.Result!.Headline);
            Assert.False(activity.IsRunning);
        }

        [Fact]
        public async Task Says_It_Is_Running_While_It_Does()
        {
            var activity = new ImportActivity();
            var release = new TaskCompletionSource();
            var running = activity.RunAsync(async () =>
            {
                await release.Task;

                return AMessage();
            });

            Assert.True(activity.IsRunning);

            release.SetResult();
            await running;

            Assert.False(activity.IsRunning);
        }

        /// <summary>
        /// The case the disabled button cannot cover: the page was left and came
        /// back, or a second tab is open on the same circuit.
        /// </summary>
        [Fact]
        public async Task Refuses_A_Second_Import_While_One_Is_In_Flight()
        {
            var activity = new ImportActivity();
            var release = new TaskCompletionSource();
            var first = activity.RunAsync(async () =>
            {
                await release.Task;

                return AMessage("erster");
            });

            var second = await activity.RunAsync(() => Task.FromResult(AMessage("zweiter")));

            release.SetResult();
            await first;

            Assert.False(second);
            Assert.Equal("erster", activity.Result!.Headline);
        }

        [Fact]
        public async Task Accepts_Another_Import_Once_The_First_Has_Finished()
        {
            var activity = new ImportActivity();
            await activity.RunAsync(() => Task.FromResult(AMessage("erster")));

            var started = await activity.RunAsync(() => Task.FromResult(AMessage("zweiter")));

            Assert.True(started);
            Assert.Equal("zweiter", activity.Result!.Headline);
        }

        [Fact]
        public async Task Clears_The_Previous_Result_When_A_New_Import_Starts()
        {
            var activity = new ImportActivity();
            await activity.RunAsync(() => Task.FromResult(AMessage("erster")));
            var release = new TaskCompletionSource();
            var second = activity.RunAsync(async () =>
            {
                await release.Task;

                return AMessage("zweiter");
            });

            Assert.Null(activity.Result);

            release.SetResult();
            await second;
        }

        [Fact]
        public async Task Stops_Running_When_The_Import_Throws()
        {
            var activity = new ImportActivity();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => activity.RunAsync(() => throw new InvalidOperationException("kaputt")));

            Assert.False(activity.IsRunning);
        }

        [Fact]
        public async Task Announces_The_Start_And_The_End()
        {
            var activity = new ImportActivity();
            var announcements = 0;
            activity.Changed += () => announcements++;

            await activity.RunAsync(() => Task.FromResult(AMessage()));

            Assert.Equal(2, announcements);
        }
    }

    public sealed class Report
    {
        [Fact]
        public void Keeps_A_Message_Without_Running_Anything()
        {
            var activity = new ImportActivity();

            activity.Report(AMessage("zu groß"));

            Assert.Equal("zu groß", activity.Result!.Headline);
            Assert.False(activity.IsRunning);
        }

        [Fact]
        public void Announces_The_Message()
        {
            var activity = new ImportActivity();
            var announced = false;
            activity.Changed += () => announced = true;

            activity.Report(AMessage());

            Assert.True(announced);
        }
    }
}
