using CashPrism.Anonymiser.Anonymisation;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class LabelledPlaceholderDictionaryTests
{
    public sealed class AssignGroup
    {
        [Fact]
        public void Assigns_Placeholders_In_Ordinal_Sorted_Order_Not_Input_Order()
        {
            var dictionary = new LabelledPlaceholderDictionary();

            dictionary.AssignGroup(["Charlie", "Alpha", "Bravo"], "Counterparty");

            Assert.Equal("Counterparty 01", dictionary.Map["Alpha"]);
            Assert.Equal("Counterparty 02", dictionary.Map["Bravo"]);
            Assert.Equal("Counterparty 03", dictionary.Map["Charlie"]);
        }

        [Fact]
        public void Duplicate_Values_In_One_Group_Share_One_Placeholder()
        {
            var dictionary = new LabelledPlaceholderDictionary();

            dictionary.AssignGroup(["Bakery", "Bakery", "Landlord"], "Counterparty");

            Assert.Equal(2, dictionary.Map.Count);
            Assert.Equal("Counterparty 01", dictionary.Map["Bakery"]);
        }

        [Fact]
        public void A_Later_Group_Does_Not_Reassign_A_Value_The_First_Group_Already_Placed()
        {
            var dictionary = new LabelledPlaceholderDictionary();

            dictionary.AssignGroup(["Checking Account"], "Account");
            dictionary.AssignGroup(["Checking Account", "Bakery"], "Counterparty");

            Assert.Equal("Account 01", dictionary.Map["Checking Account"]);
            Assert.Equal("Counterparty 01", dictionary.Map["Bakery"]);
            Assert.Equal(2, dictionary.Map.Count);
        }

        [Fact]
        public void Width_Is_Sized_To_The_Group_Own_Count_With_A_Minimum_Of_Two_Digits()
        {
            var dictionary = new LabelledPlaceholderDictionary();

            dictionary.AssignGroup(["Only One"], "Account");

            Assert.Equal("Account 01", dictionary.Map["Only One"]);
        }

        [Fact]
        public void Width_Grows_To_Fit_A_Group_Larger_Than_Ninety_Nine()
        {
            var dictionary = new LabelledPlaceholderDictionary();
            var values = Enumerable.Range(0, 100).Select(i => $"Party{i:D3}").ToArray();

            dictionary.AssignGroup(values, "Counterparty");

            Assert.Equal("Counterparty 001", dictionary.Map[values.OrderBy(v => v, StringComparer.Ordinal).First()]);
            Assert.Equal("Counterparty 100", dictionary.Map[values.OrderBy(v => v, StringComparer.Ordinal).Last()]);
        }

        [Fact]
        public void Reversing_The_Input_Order_Yields_The_Same_Assignment()
        {
            string[] values = ["Charlie", "Alpha", "Bravo", "Delta"];

            var forward = new LabelledPlaceholderDictionary();
            forward.AssignGroup(values, "Counterparty");

            var reversed = new LabelledPlaceholderDictionary();
            reversed.AssignGroup(values.Reverse(), "Counterparty");

            Assert.Equal(forward.Map.Count, reversed.Map.Count);

            foreach (var (value, placeholder) in forward.Map)
            {
                Assert.Equal(placeholder, reversed.Map[value]);
            }
        }

        [Fact]
        public void Empty_Group_Assigns_Nothing()
        {
            var dictionary = new LabelledPlaceholderDictionary();

            dictionary.AssignGroup([], "Counterparty");

            Assert.Empty(dictionary.Map);
        }
    }
}
