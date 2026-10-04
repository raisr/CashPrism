using Bunit;
using CashPrism.Application.Bookings;
using CashPrism.Application.Imports;
using CashPrism.Web.Imports;
using CashPrism.Web.Layout;
using CashPrism.Web.Tests.Unit.Bookings;
using CashPrism.Web.Tests.Unit.Imports;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Layout;

/// <summary>
/// What the drawer needs to render: the strings, the import it listens to, and
/// the stored data it reads — the last two backed by the fakes passed in.
/// </summary>
internal static class SidebarServices
{
    public static void AddSidebarServices(
        this BunitContext context,
        FakeBookingReader bookings,
        FakeImportRunReader importRuns)
    {
        context.Services.AddLocalization();
        context.Services.AddSingleton<ImportActivity>();
        context.Services.AddSingleton<IBookingReader>(bookings);
        context.Services.AddSingleton<IImportRunReader>(importRuns);
        context.Services.AddScoped<LibrarySummaryReader>();
    }
}
