using CashPrism.Anonymiser.CommandLine;

namespace CashPrism.Anonymiser.Tests.Unit.CommandLine;

public sealed class AnonymiserCommandLineTests
{
    public sealed class Parse
    {
        [Fact]
        public void Single_Input_With_Out_Resolves_Options()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out"]);

            Assert.True(result.IsSuccess);
            Assert.Equal(["input.xlsx"], result.Options!.InputFiles);
            Assert.Equal("C:/out", result.Options.OutputDirectory);
            Assert.False(result.Options.Force);
            Assert.Equal(1.0m, result.Options.Scale);
            Assert.Null(result.Options.MaxRows);
        }

        [Fact]
        public void Several_Inputs_Are_Kept_In_Order()
        {
            var result = AnonymiserCommandLine.Parse(["a.xlsx", "b.xlsx", "c.xlsx", "--out", "C:/out"]);

            Assert.True(result.IsSuccess);
            Assert.Equal(["a.xlsx", "b.xlsx", "c.xlsx"], result.Options!.InputFiles);
        }

        [Fact]
        public void Force_Switch_Sets_Force()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--force"]);

            Assert.True(result.IsSuccess);
            Assert.True(result.Options!.Force);
        }

        [Fact]
        public void Options_May_Appear_Before_The_Input_Files()
        {
            var result = AnonymiserCommandLine.Parse(["--force", "--out", "C:/out", "input.xlsx"]);

            Assert.True(result.IsSuccess);
            Assert.Equal(["input.xlsx"], result.Options!.InputFiles);
            Assert.True(result.Options.Force);
        }

        [Fact]
        public void Missing_Out_Fails()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx"]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--out", result.ErrorMessage);
        }

        [Fact]
        public void Missing_Input_Files_Fails()
        {
            var result = AnonymiserCommandLine.Parse(["--out", "C:/out"]);

            Assert.False(result.IsSuccess);
            Assert.Contains("input", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Out_Without_A_Value_Fails()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out"]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--out", result.ErrorMessage);
        }

        [Fact]
        public void Unknown_Option_Fails_Naming_It()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--bogus"]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--bogus", result.ErrorMessage);
        }

        [Fact]
        public void Empty_Arguments_Fail()
        {
            var result = AnonymiserCommandLine.Parse([]);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Scale_Switch_Sets_Scale()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--scale", "0.5"]);

            Assert.True(result.IsSuccess);
            Assert.Equal(0.5m, result.Options!.Scale);
        }

        [Fact]
        public void Scale_Without_A_Value_Fails()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--scale"]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--scale", result.ErrorMessage);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("not-a-number")]
        public void Scale_That_Is_Not_A_Positive_Number_Fails(string value)
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--scale", value]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--scale", result.ErrorMessage);
        }

        [Fact]
        public void MaxRows_Switch_Sets_MaxRows()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--max-rows", "3"]);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Options!.MaxRows);
        }

        [Fact]
        public void MaxRows_Without_A_Value_Fails()
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--max-rows"]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--max-rows", result.ErrorMessage);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("not-a-number")]
        public void MaxRows_That_Is_Not_A_Non_Negative_Whole_Number_Fails(string value)
        {
            var result = AnonymiserCommandLine.Parse(["input.xlsx", "--out", "C:/out", "--max-rows", value]);

            Assert.False(result.IsSuccess);
            Assert.Contains("--max-rows", result.ErrorMessage);
        }

        [Fact]
        public void Scale_And_MaxRows_Combine_With_Force()
        {
            var result = AnonymiserCommandLine.Parse(
                ["input.xlsx", "--out", "C:/out", "--force", "--scale", "0.5", "--max-rows", "3"]);

            Assert.True(result.IsSuccess);
            Assert.True(result.Options!.Force);
            Assert.Equal(0.5m, result.Options.Scale);
            Assert.Equal(3, result.Options.MaxRows);
        }
    }
}
