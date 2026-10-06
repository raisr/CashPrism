using System.Globalization;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// The payments that repeat: income, the mortgage, the household's contracts at
/// every interval a real export carries, the standing orders between its own
/// accounts, and the bank's fees and interest.
/// </summary>
public static class ContractBookings
{
    /// <summary>The rows every recurring payment leaves over the period <paramref name="plan"/> covers.</summary>
    public static IReadOnlyList<DemoBooking> Plan(DemoPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return Payments(plan).SelectMany(payment => payment.Book(plan)).ToList();
    }

    private static IEnumerable<RecurringPayment> Payments(DemoPlan plan)
    {
        var current = DemoHousehold.CurrentAccount;

        yield return new RecurringPayment
        {
            Name = "Gehalt",
            Account = current,
            Party = DemoParty.Payee("Nordlicht Logistik GmbH"),
            Day = 27,
            Interval = DemoInterval.Monthly,
            AmountInCents = 3_412_00,
            YearlyIncreaseInCents = 95_00,
            Category = new("Einnahmen", "Lohn / Gehalt"),
            TransactionKind = DemoTransactionKind.Transfer,
            Reference = day => Invariant($"LOHN/GEHALT {day:MM/yyyy}"),
        };

        yield return new RecurringPayment
        {
            Name = "Kindergeld",
            Account = current,
            Party = DemoParty.Payee("Familienkasse"),
            Day = 15,
            Interval = DemoInterval.Monthly,
            AmountInCents = 255_00,
            Category = new("Einnahmen", "Kindergeld"),
            TransactionKind = DemoTransactionKind.Transfer,
            Reference = day => Invariant($"KINDERGELD {day:MM/yyyy} KG-NR 123FK456789"),
        };

        yield return new RecurringPayment
        {
            Name = "Baufinanzierung",
            Account = current,
            Party = DemoParty.Creditor("Musterbank Baufinanzierung") with
            {
                Iban = DemoHousehold.LoanAccount.Reference,
            },
            Day = 30,
            Interval = DemoInterval.Monthly,
            AmountInCents = -980_00,
            Category = new("Finanzen", "Kredit"),
            TransactionKind = DemoTransactionKind.DirectDebit,
            Reference = day => Invariant($"Rate Darlehen 4711-0815 {day:MM/yyyy}"),
            TransferTo = DemoHousehold.LoanAccount,
        };

        yield return new RecurringPayment
        {
            Name = "Hausgeld",
            Account = current,
            Party = DemoParty.Payee("Hausverwaltung Ahorn & Partner"),
            Day = 1,
            Interval = DemoInterval.Monthly,
            AmountInCents = -320_00,
            Category = new("Wohnen", "Sonstiges Wohnen"),
            TransactionKind = DemoTransactionKind.StandingOrder,
            Reference = _ => "Hausgeld Wohnung 3 Lindenweg 12",
        };

        yield return DirectDebit("Strom", "Stadtwerke Musterstadt", 5, DemoInterval.Monthly, -84_00, new("Wohnen", "Strom"));
        yield return DirectDebit("Internet", "Netzwerk Nord GmbH", 3, DemoInterval.Monthly, -39_99, new("Wohnen", "Internet & Telefon"));
        yield return DirectDebit("Mobilfunk", "Funkwelle Mobil", 8, DemoInterval.Monthly, -17_99, new("Lifestyle", "Mobilfunk"));
        yield return DirectDebit("Musik", "Klangraum Music", 18, DemoInterval.Monthly, -10_99, new("Freizeit", "Musik & Podcasts"));
        yield return DirectDebit("Fitness", "Fitwerk Studio", 2, DemoInterval.Monthly, -29_90, new("Freizeit", "Sport"));
        yield return DirectDebit("Kita", "Kita Sonnenblume e.V.", 4, DemoInterval.Monthly, -180_00, new("Kinder", "Kinderbetreuung"));
        yield return DirectDebit("Wasser", "Wasserverband Musterstadt", 20, DemoInterval.BiMonthly, -46_00, new("Wohnen", "Sonstiges Wohnen"));
        yield return DirectDebit("Rundfunk", "Rundfunk-Beitragsstelle", 15, DemoInterval.Quarterly, -55_08, new("Wohnen", "Rundfunkgebuehren")) with { FirstMonth = 1 };
        yield return DirectDebit("KFZ-Versicherung", "Hanse Assekuranz AG", 1, DemoInterval.HalfYearly, -312_40, new("Versicherungen", "KFZ-Versicherung")) with { FirstMonth = 2 };
        yield return DirectDebit("Haftpflicht", "Lindwurm Versicherung AG", 1, DemoInterval.Yearly, -68_50, new("Versicherungen", "Haftpflichtversicherung")) with { FirstMonth = 4 };
        yield return DirectDebit("Hausrat", "Lindwurm Versicherung AG", 1, DemoInterval.Yearly, -96_00, new("Versicherungen", "Hausratversicherung")) with { FirstMonth = 9 };

        // The contract that ends: cancelled a little over a year before the
        // export was taken, so its last booking lies well behind the others.
        yield return DirectDebit("Streaming", "Flimmerkiste Streaming", 12, DemoInterval.Monthly, -12_99, new("Freizeit", "Serien & Filme")) with
        {
            LastDay = plan.Until.AddMonths(-14),
        };

        yield return new RecurringPayment
        {
            Name = "Sparen",
            Account = current,
            Party = DemoParty.Own(DemoHousehold.CallMoneyAccount),
            Day = 28,
            Interval = DemoInterval.Monthly,
            AmountInCents = -300_00,
            Category = new("Sparen", "Sparen"),
            TransactionKind = DemoTransactionKind.StandingOrder,
            Reference = _ => "Sparrate Tagesgeld",
            TransferTo = DemoHousehold.CallMoneyAccount,
            IsExcludedFromDisposableIncome = true,
        };

        yield return new RecurringPayment
        {
            Name = "Haushaltsgeld",
            Account = current,
            Party = DemoParty.Own(DemoHousehold.JointAccount),
            Day = 28,
            Interval = DemoInterval.Monthly,
            AmountInCents = -900_00,
            Category = new("Sonstiges", "Sonstige Ausgaben"),
            TransactionKind = DemoTransactionKind.StandingOrder,
            Reference = _ => "Haushaltsgeld",
            TransferTo = DemoHousehold.JointAccount,
            IncomingCategory = new("Einnahmen", "Sonstige Einnahmen"),
            IsContract = false,
        };

        yield return new RecurringPayment
        {
            Name = "Taschengeld",
            Account = current,
            Party = DemoParty.Payee("Lina Beispiel"),
            Day = 1,
            Interval = DemoInterval.Monthly,
            AmountInCents = -40_00,
            Category = new("Kinder", "Taschengeld"),
            TransactionKind = DemoTransactionKind.StandingOrder,
            Reference = _ => "Taschengeld Lina",
        };

        yield return new RecurringPayment
        {
            Name = "Kontofuehrung",
            Account = current,
            Party = DemoParty.Merchant("Musterbank"),
            Day = 30,
            Interval = DemoInterval.Monthly,
            AmountInCents = -4_95,
            Category = new("Finanzen", "Bankgebuehren"),
            TransactionKind = DemoTransactionKind.InterestAndFees,
            Reference = _ => "Entgelt Kontofuehrung",
            IsContract = false,
        };

        yield return new RecurringPayment
        {
            Name = "Zinsen",
            Account = DemoHousehold.CallMoneyAccount,
            Party = DemoParty.Merchant("Musterbank"),
            Day = 30,
            Interval = DemoInterval.Quarterly,
            FirstMonth = 2,
            AmountInCents = 11_37,
            YearlyIncreaseInCents = 3_10,
            Category = new("Einnahmen", "Kapitalertraege"),
            TransactionKind = DemoTransactionKind.InterestAndFees,
            Reference = _ => "Zinsgutschrift",
            IsContract = false,
        };
    }

    private static RecurringPayment DirectDebit(
        string name,
        string creditor,
        int day,
        DemoInterval interval,
        long amountInCents,
        DemoCategory category)
        => new()
        {
            Name = name,
            Account = DemoHousehold.CurrentAccount,
            Party = DemoParty.Creditor(creditor),
            Day = day,
            Interval = interval,
            AmountInCents = amountInCents,
            Category = category,
            TransactionKind = DemoTransactionKind.DirectDebit,
            Reference = date => Invariant($"{creditor} {name} {date:MM/yyyy}"),
        };

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
