using CashPrism.Web.Bookings;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// Every main category the measured exports contain, and every sub-category
/// that moves a booking into a different design category than its main one —
/// spelled as Finanzguru spells them (see <c>docs/finanzguru-export.md</c>).
/// </summary>
public sealed class CategoryStyleTests
{
    public sealed class For
    {
        // A sub-category the mapping does not single out, so the main category decides.
        private const string AnySubCategory = "Sonstige Ausgaben";

        public static TheoryData<string, CategoryStyle> MainCategories => new()
        {
            { "Drogerie", CategoryStyle.Shopping },
            { "Einnahmen", CategoryStyle.Income },
            { "Essen & Trinken", CategoryStyle.Groceries },
            { "Finanzen", CategoryStyle.Other },
            { "Freizeit", CategoryStyle.Leisure },
            { "Gesundheit", CategoryStyle.Health },
            { "Haustiere", CategoryStyle.Other },
            { "Kinder", CategoryStyle.Other },
            { "Lifestyle", CategoryStyle.Shopping },
            { "Mobilitaet", CategoryStyle.Mobility },
            { "Sonstiges", CategoryStyle.Other },
            { "Sparen", CategoryStyle.Other },
            { "Versicherungen", CategoryStyle.Insurance },
            { "Wohnen", CategoryStyle.Housing },
        };

        public static TheoryData<string, string, CategoryStyle> SubCategories => new()
        {
            { "Essen & Trinken", "Lebensmittel", CategoryStyle.Groceries },
            { "Essen & Trinken", "Restaurants", CategoryStyle.Leisure },
            { "Essen & Trinken", "Lieferservice", CategoryStyle.Leisure },
            { "Lifestyle", "Mobilfunk", CategoryStyle.Subscriptions },
            { "Lifestyle", "Cloud-Dienste", CategoryStyle.Subscriptions },
            { "Lifestyle", "Prime-Mitgliedschaft", CategoryStyle.Subscriptions },
            { "Freizeit", "Serien & Filme", CategoryStyle.Subscriptions },
            { "Freizeit", "Musik & Podcasts", CategoryStyle.Subscriptions },
            { "Freizeit", "Mitgliedschaft", CategoryStyle.Subscriptions },
            { "Wohnen", "Internet & Telefon", CategoryStyle.Subscriptions },
            { "Wohnen", "Rundfunkgebuehren", CategoryStyle.Subscriptions },
            { "Wohnen", "Strom", CategoryStyle.Housing },
        };

        [Theory]
        [MemberData(nameof(MainCategories))]
        public void Maps_A_Main_Category_Of_The_Export(string category, CategoryStyle expected)
        {
            Assert.Equal(expected, CategoryStyle.For(category, AnySubCategory, isTransfer: false));
        }

        [Theory]
        [MemberData(nameof(SubCategories))]
        public void Maps_A_Sub_Category_By_Its_Main_Category_Unless_The_Design_Files_It_Elsewhere(
            string category,
            string subCategory,
            CategoryStyle expected)
        {
            Assert.Equal(expected, CategoryStyle.For(category, subCategory, isTransfer: false));
        }

        [Fact]
        public void Falls_Back_To_Other_For_A_Name_It_Does_Not_Know()
        {
            Assert.Equal(CategoryStyle.Other, CategoryStyle.For("Bildung", "Studiengebuehren", isTransfer: false));
        }

        [Fact]
        public void Takes_A_Sub_Category_Only_Under_Its_Own_Main_Category()
        {
            Assert.Equal(CategoryStyle.Other, CategoryStyle.For("Sonstiges", "Restaurants", isTransfer: false));
        }

        [Fact]
        public void Shows_A_Transfer_As_A_Transfer_Whatever_Its_Category()
        {
            Assert.Equal(CategoryStyle.Transfer, CategoryStyle.For("Sparen", "Sparen", isTransfer: true));
        }
    }

    /// <summary>
    /// The colours and glyphs of <c>design/README.md</c> §1.5 and the
    /// iconography of <c>design/DESIGN_SYSTEM.md</c>.
    /// </summary>
    public sealed class ForMainCategory
    {
        [Fact]
        public void Shows_A_Measured_Main_Category_In_Its_Design_Category()
            => Assert.Same(CategoryStyle.Mobility, CategoryStyle.ForMainCategory("Mobilitaet"));

        [Fact]
        public void Shows_An_Unknown_Main_Category_As_Other()
            => Assert.Same(CategoryStyle.Other, CategoryStyle.ForMainCategory("Meine Kategorie"));
    }

    public sealed class TheDesignCategories
    {
        public static TheoryData<CategoryStyle, string, string> Design => new()
        {
            { CategoryStyle.Housing, "var(--cp-prism-violet)", "house" },
            { CategoryStyle.Mobility, "var(--cp-prism-blue)", "car" },
            { CategoryStyle.Subscriptions, "var(--cp-prism-cyan)", "repeat" },
            { CategoryStyle.Insurance, "var(--cp-prism-teal)", "shield" },
            { CategoryStyle.Groceries, "var(--cp-prism-green)", "shopping-basket" },
            { CategoryStyle.Health, "var(--cp-prism-amber)", "heart-pulse" },
            { CategoryStyle.Shopping, "var(--cp-prism-orange)", "shopping-bag" },
            { CategoryStyle.Leisure, "var(--cp-prism-rose)", "popcorn" },
            { CategoryStyle.Other, "var(--cp-prism-slate)", "circle-ellipsis" },
            { CategoryStyle.Transfer, "var(--cp-prism-slate)", "arrow-left-right" },
            { CategoryStyle.Income, "var(--cp-income)", "banknote" },
        };

        [Theory]
        [MemberData(nameof(Design))]
        public void Carry_The_Colour_And_The_Glyph_Of_The_Design(CategoryStyle style, string colour, string icon)
        {
            Assert.Equal(colour, style.Colour);
            Assert.Equal(icon, style.Icon);
        }
    }
}
