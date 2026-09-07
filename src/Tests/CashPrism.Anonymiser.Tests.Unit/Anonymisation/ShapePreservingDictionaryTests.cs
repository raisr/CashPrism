using System.Text.RegularExpressions;
using CashPrism.Anonymiser.Anonymisation;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class ShapePreservingDictionaryTests
{
    public sealed class AssignGroup
    {
        [Fact]
        public void An_Iban_Shaped_Value_Gets_An_Iban_Shaped_Replacement_Of_The_Same_Length()
        {
            var dictionary = new ShapePreservingDictionary(totalDistinctCount: 1);
            const string original = "DE02120300000000202051";

            dictionary.AssignGroup([original]);

            var replacement = dictionary.Map[original];

            Assert.Equal(original.Length, replacement.Length);
            Assert.StartsWith("DE", replacement, StringComparison.Ordinal);
            Assert.NotEqual(original, replacement);
        }

        [Fact]
        public void An_Email_Shaped_Value_Becomes_An_Account_Address_Under_Example_Invalid()
        {
            var dictionary = new ShapePreservingDictionary(totalDistinctCount: 1);

            dictionary.AssignGroup(["payer@example.com"]);

            Assert.Matches("^account-[0-9]{2}@example\\.invalid$", dictionary.Map["payer@example.com"]);
        }

        [Fact]
        public void An_Opaque_Value_Keeps_Its_Length()
        {
            var dictionary = new ShapePreservingDictionary(totalDistinctCount: 1);
            const string original = "OPQ-7F3";

            dictionary.AssignGroup([original]);

            Assert.Equal(original.Length, dictionary.Map[original].Length);
        }

        [Fact]
        public void Own_Values_Get_The_Lowest_Indices_Regardless_Of_Alphabetical_Position()
        {
            var dictionary = new ShapePreservingDictionary(totalDistinctCount: 2);

            dictionary.AssignGroup(["ZZ0000000000000000000000"]);
            dictionary.AssignGroup(["AA0000000000000000000000"]);

            Assert.EndsWith("1", dictionary.Map["ZZ0000000000000000000000"], StringComparison.Ordinal);
            Assert.EndsWith("2", dictionary.Map["AA0000000000000000000000"], StringComparison.Ordinal);
        }

        [Fact]
        public void The_Same_Value_In_Both_Groups_Gets_One_Shared_Replacement()
        {
            var dictionary = new ShapePreservingDictionary(totalDistinctCount: 1);
            const string ownIban = "DE02120300000000202051";

            dictionary.AssignGroup([ownIban]);
            dictionary.AssignGroup([ownIban]);

            Assert.Single(dictionary.Map);
        }

        [Fact]
        public void Reversing_The_Input_Order_Yields_The_Same_Assignment()
        {
            string[] values = ["ZZ0000000000000000000000", "AA0000000000000000000000", "MM0000000000000000000000"];

            var forward = new ShapePreservingDictionary(values.Length);
            forward.AssignGroup(values);

            var reversed = new ShapePreservingDictionary(values.Length);
            reversed.AssignGroup(values.Reverse());

            foreach (var value in values)
            {
                Assert.Equal(forward.Map[value], reversed.Map[value]);
            }
        }

        [Fact]
        public void Email_Width_Reflects_The_Total_Count_Given_Upfront_Not_Just_This_Group()
        {
            var dictionary = new ShapePreservingDictionary(totalDistinctCount: 150);

            dictionary.AssignGroup(["payer@example.com"]);

            Assert.Matches("^account-[0-9]{3}@example\\.invalid$", dictionary.Map["payer@example.com"]);
        }
    }
}
