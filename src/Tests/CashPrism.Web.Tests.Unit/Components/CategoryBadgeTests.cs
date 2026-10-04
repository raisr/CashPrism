using Bunit;
using CashPrism.Web.Bookings;
using CashPrism.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class CategoryBadgeTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render() => context.Services.AddLocalization();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Names_The_Category_In_German()
        {
            var badge = context.Render<CategoryBadge>(parameters => parameters
                .Add(p => p.Category, CategoryStyle.Subscriptions));

            Assert.Equal("Verträge & Abos", badge.Find(".cp-badge").TextContent);
        }

        public static TheoryData<CategoryStyle> EveryCategory =>
        [
            CategoryStyle.Housing,
            CategoryStyle.Mobility,
            CategoryStyle.Subscriptions,
            CategoryStyle.Insurance,
            CategoryStyle.Groceries,
            CategoryStyle.Health,
            CategoryStyle.Shopping,
            CategoryStyle.Leisure,
            CategoryStyle.Other,
            CategoryStyle.Transfer,
            CategoryStyle.Income,
        ];

        /// <summary>A key missing from the resource file would show up as the key itself.</summary>
        [Theory]
        [MemberData(nameof(EveryCategory))]
        public void Finds_A_Name_For_Every_Category(CategoryStyle category)
        {
            var badge = context.Render<CategoryBadge>(parameters => parameters
                .Add(p => p.Category, category));

            Assert.NotEqual(category.NameKey, badge.Find(".cp-badge").TextContent);
        }

        /// <summary>
        /// The pill's tint and its dot are both drawn by the stylesheet from
        /// <c>--c</c>, so this is what decides that each category's badge
        /// carries its own colour.
        /// </summary>
        [Theory]
        [MemberData(nameof(EveryCategory))]
        public void Hands_The_Pill_The_Colour_It_Is_Tinted_With(CategoryStyle category)
        {
            var badge = context.Render<CategoryBadge>(parameters => parameters
                .Add(p => p.Category, category));

            Assert.Equal($"--c:{category.Colour}", badge.Find(".cp-badge--category").GetAttribute("style"));
        }
    }
}
