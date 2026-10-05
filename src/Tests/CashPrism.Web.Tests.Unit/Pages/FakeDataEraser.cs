using CashPrism.Application.Persistence;

namespace CashPrism.Web.Tests.Unit.Pages;

/// <summary>
/// Stands in for deleting all data: counts the calls, runs what the test says
/// deleting does to its other fakes, and can be held open to look at the page
/// while the deletion is in flight.
/// </summary>
public sealed class FakeDataEraser : IDataEraser
{
    private TaskCompletionSource? hold;

    /// <summary>How often everything was deleted.</summary>
    public int Erasures { get; private set; }

    /// <summary>What deleting does to the other fakes, such as emptying the readers.</summary>
    public Action OnErase { get; set; } = () => { };

    /// <summary>Makes the next deletion wait until <see cref="Release"/>.</summary>
    public void Hold() => hold = new TaskCompletionSource();

    /// <summary>Lets a held deletion finish.</summary>
    public void Release() => hold?.SetResult();

    public async Task EraseAllAsync(CancellationToken cancellationToken = default)
    {
        if (hold is not null)
        {
            await hold.Task;
        }

        Erasures++;
        OnErase();
    }
}
