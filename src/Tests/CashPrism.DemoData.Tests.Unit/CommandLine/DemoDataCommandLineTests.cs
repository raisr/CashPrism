using CashPrism.DemoData.CommandLine;

namespace CashPrism.DemoData.Tests.Unit.CommandLine;

public sealed class DemoDataCommandLineTests
{
    public sealed class Parse
    {
        private static readonly DateOnly Today = new(2026, 10, 6);

        [Fact]
        public void Out_Alone_Resolves_Options_Ending_Today()
        {
            var result = DemoDataCommandLine.Parse(["--out", "C:/out"], Today);

            Assert.True(result.IsSuccess);
            Assert.Equal("C:/out", result.Options!.OutputDirectory);
            Assert.Equal(Today, result.Options.Until);
        }

        [Fact]
        public void Until_Sets_The_Last_Day()
        {
            var result = DemoDataCommandLine.Parse(["--out", "C:/out", "--until", "2026-10-01"], Today);

            Assert.Equal(new DateOnly(2026, 10, 1), result.Options!.Until);
        }

        [Fact]
        public void Missing_Out_Fails()
        {
            var result = DemoDataCommandLine.Parse(["--until", "2026-10-01"], Today);

            Assert.False(result.IsSuccess);
            Assert.Contains("--out", result.ErrorMessage, StringComparison.Ordinal);
        }

        [Fact]
        public void Out_Without_A_Directory_Fails()
        {
            var result = DemoDataCommandLine.Parse(["--out"], Today);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Until_Without_A_Date_Fails()
        {
            var result = DemoDataCommandLine.Parse(["--out", "C:/out", "--until"], Today);

            Assert.False(result.IsSuccess);
        }

        [Theory]
        [InlineData("01.10.2026")]
        [InlineData("2026-13-01")]
        [InlineData("20261001")]
        public void Until_In_Another_Shape_Fails_And_Names_The_Value(string until)
        {
            var result = DemoDataCommandLine.Parse(["--out", "C:/out", "--until", until], Today);

            Assert.False(result.IsSuccess);
            Assert.Contains(until, result.ErrorMessage, StringComparison.Ordinal);
        }

        [Fact]
        public void An_Unknown_Argument_Fails_And_Names_It()
        {
            var result = DemoDataCommandLine.Parse(["--out", "C:/out", "--force"], Today);

            Assert.False(result.IsSuccess);
            Assert.Contains("--force", result.ErrorMessage, StringComparison.Ordinal);
        }
    }
}
