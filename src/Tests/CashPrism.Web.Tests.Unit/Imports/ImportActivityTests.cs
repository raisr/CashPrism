using CashPrism.Web.Imports;
using CashPrism.Web.StoredData;

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
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);

            var started = await activity.RunAsync(() => Task.FromResult(AMessage("fertig")));

            Assert.True(started);
            Assert.Equal("fertig", activity.Result!.Headline);
            Assert.False(activity.IsRunning);
        }

        [Fact]
        public async Task Says_It_Is_Running_While_It_Does()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
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
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
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
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            await activity.RunAsync(() => Task.FromResult(AMessage("erster")));

            var started = await activity.RunAsync(() => Task.FromResult(AMessage("zweiter")));

            Assert.True(started);
            Assert.Equal("zweiter", activity.Result!.Headline);
        }

        [Fact]
        public async Task Clears_The_Previous_Result_When_A_New_Import_Starts()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
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
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => activity.RunAsync(() => throw new InvalidOperationException("kaputt")));

            Assert.False(activity.IsRunning);
        }

        [Fact]
        public async Task Announces_The_Start_And_The_End()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var announcements = 0;
            activity.Changed += () => announcements++;

            await activity.RunAsync(() => Task.FromResult(AMessage()));

            Assert.Equal(2, announcements);
        }

        [Fact]
        public async Task Announces_The_Completion_Once_The_Import_Has_Finished()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var completions = 0;
            changes.Changed += () => completions++;

            await activity.RunAsync(() => Task.FromResult(AMessage()));

            Assert.Equal(1, completions);
        }

        [Fact]
        public async Task Does_Not_Announce_The_Completion_While_The_Import_Runs()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var completed = false;
            changes.Changed += () => completed = true;
            var release = new TaskCompletionSource();
            var running = activity.RunAsync(async () =>
            {
                await release.Task;

                return AMessage();
            });

            Assert.False(completed);

            release.SetResult();
            await running;
        }

        [Fact]
        public async Task Does_Not_Announce_The_Completion_When_The_Import_Throws()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var completed = false;
            changes.Changed += () => completed = true;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => activity.RunAsync(() => throw new InvalidOperationException("kaputt")));

            Assert.False(completed);
        }

        [Fact]
        public async Task Does_Not_Announce_A_Completion_For_An_Import_It_Turned_Away()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var release = new TaskCompletionSource();
            var first = activity.RunAsync(async () =>
            {
                await release.Task;

                return AMessage("erster");
            });
            var completions = 0;
            changes.Changed += () => completions++;

            await activity.RunAsync(() => Task.FromResult(AMessage("zweiter")));
            release.SetResult();
            await first;

            Assert.Equal(1, completions);
        }
    }

    public sealed class Report
    {
        [Fact]
        public void Keeps_A_Message_Without_Running_Anything()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);

            activity.Report(AMessage("zu groß"));

            Assert.Equal("zu groß", activity.Result!.Headline);
            Assert.False(activity.IsRunning);
        }

        [Fact]
        public void Announces_The_Message()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var announced = false;
            activity.Changed += () => announced = true;

            activity.Report(AMessage());

            Assert.True(announced);
        }

        [Fact]
        public void Does_Not_Announce_A_Completion()
        {
            var changes = new StoredDataChanges();
            var activity = new ImportActivity(changes);
            var completed = false;
            changes.Changed += () => completed = true;

            activity.Report(AMessage());

            Assert.False(completed);
        }
    }
}
