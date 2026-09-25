using CashPrism.Web.Localisation;
using CashPrism.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Tests.Unit.Localisation;

public sealed class ResourceMudLocalizerTests
{
    /// <summary>
    /// Builds the localiser over the real <c>Strings.resx</c> — the point of
    /// these tests is that the resource file and MudBlazor's keys line up, and a
    /// stubbed localiser would prove nothing about that.
    /// </summary>
    private static ResourceMudLocalizer Create()
    {
        var provider = new ServiceCollection()
            // AddLocalization's factory takes an ILoggerFactory, which a bare
            // container has no answer for.
            .AddLogging()
            .AddLocalization()
            .BuildServiceProvider();

        return new ResourceMudLocalizer(provider.GetRequiredService<IStringLocalizer<Strings>>());
    }

    public sealed class Indexer
    {
        [Fact]
        public void Answers_In_German_When_The_Resource_File_Carries_The_Key()
        {
            var localizer = Create();

            var localised = localizer["MudDataGridPager_RowsPerPage"];

            Assert.Equal("Zeilen pro Seite:", localised.Value);
            Assert.False(localised.ResourceNotFound);
        }

        [Fact]
        public void Reports_The_Key_As_Not_Found_When_The_Resource_File_Is_Silent()
        {
            var localizer = Create();

            // MudBlazor reads ResourceNotFound and only then falls back to its
            // own English default. A localiser that answered here — with the key
            // itself, say — would render raw identifiers in the UI instead.
            var localised = localizer["MudBlazor_KeyThatStringsResxDoesNotCarry"];

            Assert.True(localised.ResourceNotFound);
        }
    }
}
