using System.Globalization;
using Bunit;
using CashPrism.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class PagerTests
{
    public sealed class Render : IAsyncLifetime
    {
        private const string InfoFormat = "{0:N0}–{1:N0} von {2:N0} Buchungen";

        private readonly BunitContext context = new();
        private readonly CultureInfo formerCulture = CultureInfo.CurrentCulture;

        // The host pins German; the test does the same rather than inherit
        // whatever the machine running it is set to.
        public Render()
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            context.Services.AddLocalization();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            CultureInfo.CurrentCulture = formerCulture;
            await context.DisposeAsync();
        }

        [Fact]
        public void Says_Which_Entries_Of_How_Many_Are_On_Screen_With_German_Separators()
        {
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327)
                .Add(p => p.Page, 40));

            Assert.Equal("1.001–1.025 von 6.327 Buchungen", pager.Find(".cp-pager__info").TextContent);
        }

        [Fact]
        public void Counts_Only_What_Is_Left_On_The_Last_Page()
        {
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327)
                .Add(p => p.Page, 253));

            Assert.Equal("6.326–6.327 von 6.327 Buchungen", pager.Find(".cp-pager__info").TextContent);
        }

        [Fact]
        public void Marks_The_Current_Page()
        {
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327)
                .Add(p => p.Page, 10));

            var current = pager.Find(".cp-pager__page--on");
            Assert.Equal("11", current.TextContent.Trim());
            Assert.Equal("page", current.GetAttribute("aria-current"));
        }

        [Fact]
        public void Asks_For_The_Page_That_Was_Clicked()
        {
            int? asked = null;
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327)
                .Add(p => p.PageChanged, page => asked = page));

            pager.FindAll(".cp-pager__page").Single(button => button.TextContent.Trim() == "254").Click();

            Assert.Equal(253, asked);
        }

        [Fact]
        public void Offers_No_Way_Back_From_The_First_Page()
        {
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327));

            Assert.True(pager.Find("button[aria-label='Vorherige Seite']").HasAttribute("disabled"));
            Assert.False(pager.Find("button[aria-label='Nächste Seite']").HasAttribute("disabled"));
        }

        [Fact]
        public void Asks_For_The_Page_Size_That_Was_Picked()
        {
            int? asked = null;
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327)
                .Add(p => p.PageSizeChanged, size => asked = size));

            pager.Find("select").Change("100");

            Assert.Equal(100, asked);
        }

        [Fact]
        public void Ignores_A_Page_Size_It_Did_Not_Offer()
        {
            int? asked = null;
            var pager = context.Render<Pager>(parameters => parameters
                .Add(p => p.InfoFormat, InfoFormat)
                .Add(p => p.Total, 6327)
                .Add(p => p.PageSizeChanged, size => asked = size));

            pager.Find("select").Change("6327");

            Assert.Null(asked);
        }
    }
}
