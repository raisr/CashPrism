using System.Globalization;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// The spending that does not repeat on a schedule: card payments, cash, the
/// credit card and its monthly settlement, the payment provider and its
/// top-ups, and a summer holiday each year.
/// </summary>
public static class EverydayBookings
{
    private static readonly IReadOnlyList<string> Groceries = ["Frischmarkt", "Grünkorb Biomarkt", "Backstube Korn"];

    private static readonly IReadOnlyList<string> Restaurants = ["Trattoria Bella Vista", "Imbiss Zum Hafen", "Café Lindenblüte"];

    private static readonly IReadOnlyList<(string Merchant, DemoCategory Category)> OnlineShops =
    [
        ("Versandhaus Lindenhof", new("Lifestyle", "Shopping")),
        ("Technikwelt Online", new("Lifestyle", "Elektrohandel")),
        ("Kleiderei", new("Lifestyle", "Bekleidung")),
    ];

    private static readonly IReadOnlyList<(DemoParty Party, DemoCategory Category)> ProviderMerchants =
    [
        (DemoParty.ByEmail("Buchladen Ahorn", "shop@buchladen-ahorn.example"), new("Freizeit", "Buecher & Zeitungen")),
        (DemoParty.ByEmail("Spielezentrale", "billing@spielezentrale.example"), new("Freizeit", "Gaming")),
        (DemoParty.ByEmail("Tom Kramer", "tom.kramer@mail.example"), new("Lifestyle", "Shopping")),
    ];

    private static readonly IReadOnlyList<string> HolidayMerchants =
    [
        "Ferienhof Dünenblick",
        "Strandkorbverleih Ostsee",
        "Fischhaus am Deich",
        "Inselfähre Nord",
    ];

    private static readonly DemoParty ProviderCollector = DemoParty.Creditor("Paynet Europe");

    private static readonly DemoParty CardIssuer = DemoParty.Creditor("Musterbank Kreditkarte");

    private static readonly DemoParty CashMachine = DemoParty.Merchant("Geldautomat Musterbank");

    /// <summary>The everyday rows over the period <paramref name="plan"/> covers.</summary>
    public static IReadOnlyList<DemoBooking> Plan(DemoPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var bookings = new List<DemoBooking>();

        foreach (var day in plan.Days())
        {
            bookings.AddRange(Day(plan, day));
        }

        var settlements = CreditCardSettlements(plan, bookings).ToList();
        bookings.AddRange(settlements);

        return bookings;
    }

    private static IEnumerable<DemoBooking> Day(DemoPlan plan, DateOnly day)
    {
        var joint = DemoHousehold.JointAccount;
        var current = DemoHousehold.CurrentAccount;
        var card = DemoHousehold.CreditCard;
        var isSunday = day.DayOfWeek == DayOfWeek.Sunday;

        if (!isSunday && plan.Chance(0.45))
        {
            yield return CardPayment(plan, joint, day, plan.Pick(Groceries), plan.Cents(8_40, 96_80), new("Essen & Trinken", "Lebensmittel"));
        }

        if (!isSunday && plan.Chance(0.07))
        {
            yield return CardPayment(plan, joint, day, "Drogerie Lindner", plan.Cents(4_95, 38_60), new("Drogerie", "Drogerie"));
        }

        if (!isSunday && plan.Chance(0.03))
        {
            yield return CardPayment(plan, joint, day, "Löwen-Apotheke", plan.Cents(3_50, 42_00), new("Gesundheit", "Apotheke"));
        }

        if (day.Day == 10)
        {
            yield return CardPayment(plan, joint, day, "Tierbedarf Pfote", plan.Cents(24_00, 61_00), new("Haustiere", "Futter & Tierbedarf"));
        }

        if (plan.Chance(0.08))
        {
            yield return CardPayment(plan, current, day, "Tankstelle Am Ring", plan.Cents(42_00, 86_00), new("Mobilitaet", "Tanken"));
        }

        if (plan.Chance(0.07))
        {
            yield return CardPayment(plan, current, day, plan.Pick(Restaurants), plan.Cents(14_50, 78_00), new("Essen & Trinken", "Restaurants"));
        }

        if (plan.Chance(0.03))
        {
            yield return CardPayment(plan, current, day, "Pizzabote Express", plan.Cents(18_00, 42_00), new("Essen & Trinken", "Lieferservice"));
        }

        if (day.Day == 6)
        {
            var amount = plan.Pick<long>([100_00, 150_00, 200_00]);

            yield return plan.Book(current, Midnight(day), -amount, CashMachine, new("Sonstiges", "Bargeld"), DemoTransactionKind.CashWithdrawal) with
            {
                PaymentReference = Invariant($"{CardReference(plan, current, day)} Bargeldauszahlung"),
            };
        }

        if (plan.Chance(0.05))
        {
            yield return CardPayment(plan, card, day, "Nahverkehr Musterstadt", plan.Cents(2_90, 24_00), new("Mobilitaet", "Bus & Bahn"));
        }

        if (plan.Chance(0.05))
        {
            var (merchant, category) = plan.Pick(OnlineShops);

            yield return CardPayment(plan, card, day, merchant, plan.Cents(12_99, 189_00), category);
        }

        if (day.Month == 7 && day.Day is >= 12 and <= 23 && plan.Chance(0.5))
        {
            yield return CardPayment(plan, card, day, plan.Pick(HolidayMerchants), plan.Cents(9_80, 240_00), new("Freizeit", "Urlaub")) with
            {
                Tags = Invariant($"Urlaub {day.Year}"),
            };
        }

        if (plan.Chance(0.06))
        {
            foreach (var booking in ProviderPayment(plan, day))
            {
                yield return booking;
            }
        }
    }

    /// <summary>
    /// A payment through the payment provider leaves three rows: the provider
    /// collects the money from the current account, books it in as a top-up and
    /// pays it out to a party it names by email address. Only the payment out
    /// carries a time of day.
    /// </summary>
    private static IEnumerable<DemoBooking> ProviderPayment(DemoPlan plan, DateOnly day)
    {
        var provider = DemoHousehold.ProviderAccount;
        var (party, category) = plan.Pick(ProviderMerchants);
        var amount = plan.Cents(6_50, 64_90);

        var topUp = plan.Transfer(new DemoTransfer
        {
            From = DemoHousehold.CurrentAccount,
            To = provider,
            Date = Midnight(day),
            AmountInCents = amount,
            OutgoingKind = DemoTransactionKind.DirectDebit,
            OutgoingCategory = new("Sonstiges", "Sonstige Ausgaben"),
            IncomingCategory = new("Einnahmen", "Sonstige Einnahmen"),
            IncomingKind = DemoTransactionKind.None,
            Payee = ProviderCollector,
            PaymentReference = Invariant($"Paynet Zahlung an {party.Name}"),
        });

        foreach (var booking in topUp)
        {
            yield return booking;
        }

        yield return plan.Book(provider, Midnight(day) + plan.TimeOfDay(), -amount, party, category, DemoTransactionKind.Other);
    }

    /// <summary>
    /// The credit card is settled from the current account on the third of the
    /// following month, for exactly what was spent on it.
    /// </summary>
    private static IEnumerable<DemoBooking> CreditCardSettlements(DemoPlan plan, IReadOnlyList<DemoBooking> bookings)
    {
        var card = DemoHousehold.CreditCard;
        var spendingByMonth = bookings
            .Where(booking => booking.Account == card)
            .GroupBy(booking => new DateOnly(booking.Date.Year, booking.Date.Month, 1))
            .OrderBy(month => month.Key);

        foreach (var month in spendingByMonth)
        {
            var settledOn = DemoPlan.On(month.Key.AddMonths(1), 3);

            if (!plan.Covers(settledOn))
            {
                continue;
            }

            var transfer = plan.Transfer(new DemoTransfer
            {
                From = DemoHousehold.CurrentAccount,
                To = card,
                Date = Midnight(settledOn),
                AmountInCents = -month.Sum(booking => booking.AmountInCents),
                OutgoingKind = DemoTransactionKind.DirectDebit,
                OutgoingCategory = new("Sonstiges", "Kreditkartenabrechnung"),
                IncomingCategory = new("Sonstiges", "Kreditkartenabrechnung"),
                Payee = CardIssuer,
                PaymentReference = Invariant($"Kreditkartenabrechnung {month.Key:MM/yyyy}"),
            });

            foreach (var booking in transfer)
            {
                yield return booking;
            }
        }
    }

    private static DemoBooking CardPayment(
        DemoPlan plan,
        DemoAccount account,
        DateOnly day,
        string merchant,
        long amountInCents,
        DemoCategory category)
        => plan.Book(account, Midnight(day), -amountInCents, DemoParty.Merchant(merchant), category, DemoTransactionKind.CardPayment) with
        {
            PaymentReference = CardReference(plan, account, day),
        };

    /// <summary>
    /// A card payment's reference as the bank transmits it: the moment of
    /// payment as an ISO timestamp, then the card and its expiry.
    /// </summary>
    private static string CardReference(DemoPlan plan, DemoAccount account, DateOnly day)
        => Invariant($"{Midnight(day) + plan.TimeOfDay():yyyy-MM-ddTHH:mm}      {account.CardLabel} 2029-08");

    private static DateTime Midnight(DateOnly day) => day.ToDateTime(TimeOnly.MinValue);

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
