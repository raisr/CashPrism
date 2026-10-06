using System.Globalization;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// The bookings that happen once, or once a year, and exist to show a special
/// case a real export carries: the split booking, the booking excluded from the
/// disposable income without being a transfer, the account that falls silent
/// and the yearly donation tagged for the tax return.
/// </summary>
public static class OneOffBookings
{
    /// <summary>The one-off rows over the period <paramref name="plan"/> covers.</summary>
    public static IReadOnlyList<DemoBooking> Plan(DemoPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var months = plan.Months();

        return
        [
            .. DormantAccount(plan, months),
            .. SplitBooking(plan, months[7]),
            ExcludedPurchase(plan, months[10]),
            .. Donations(plan),
        ];
    }

    /// <summary>
    /// The old savings account pays its fee for four months, is then emptied
    /// into the call money account and books nothing ever after.
    /// </summary>
    private static IEnumerable<DemoBooking> DormantAccount(DemoPlan plan, IReadOnlyList<DateOnly> months)
    {
        var account = DemoHousehold.DormantAccount;
        const long Fee = 2_50;
        var balance = account.OpeningBalanceInCents;

        foreach (var month in months.Skip(1).Take(4))
        {
            balance -= Fee;

            yield return plan.Book(
                    account,
                    Midnight(DemoPlan.On(month, 30)),
                    -Fee,
                    DemoParty.Merchant("Musterbank"),
                    new("Finanzen", "Bankgebuehren"),
                    DemoTransactionKind.InterestAndFees)
                with
            {
                PaymentReference = "Entgelt Kontofuehrung",
            };
        }

        var closing = plan.Transfer(new DemoTransfer
        {
            From = account,
            To = DemoHousehold.CallMoneyAccount,
            Date = Midnight(DemoPlan.On(months[5], 10)),
            AmountInCents = balance,
            OutgoingKind = DemoTransactionKind.Transfer,
            OutgoingCategory = new("Sparen", "Sparen"),
            IncomingCategory = new("Sparen", "Sparen"),
            PaymentReference = "Kontoaufloesung Sparkonto",
        });

        foreach (var booking in closing)
        {
            yield return booking;
        }
    }

    /// <summary>
    /// One direct debit split into a gift and the rest: the original row, a
    /// part and the remainder, the two of them pointing back at the original
    /// and adding up to it to the cent.
    /// </summary>
    private static IEnumerable<DemoBooking> SplitBooking(DemoPlan plan, DateOnly month)
    {
        var date = Midnight(DemoPlan.On(month, 12));
        var party = DemoParty.Creditor("Versandhaus Lindenhof");
        const string Reference = "Bestellung 2208-4471 Versandhaus Lindenhof";

        var original = plan.Book(
                DemoHousehold.CurrentAccount,
                date,
                -149_97,
                party,
                new("Lifestyle", "Shopping"),
                DemoTransactionKind.DirectDebit)
            with
        {
            PaymentReference = Reference,
            SplitType = FinanzguruSplitType.Original,
        };

        yield return original;

        yield return plan.Book(
                DemoHousehold.CurrentAccount,
                date,
                -59_99,
                party,
                new("Lifestyle", "Geschenke"),
                DemoTransactionKind.DirectDebit)
            with
        {
            PaymentReference = Reference,
            SplitType = FinanzguruSplitType.Part,
            OriginalReferenceId = original.BookingId,
            Tags = "Geburtstag",
        };

        yield return plan.Book(
                DemoHousehold.CurrentAccount,
                date,
                -89_98,
                party,
                new("Lifestyle", "Bekleidung"),
                DemoTransactionKind.DirectDebit)
            with
        {
            PaymentReference = Reference,
            SplitType = FinanzguruSplitType.Remainder,
            OriginalReferenceId = original.BookingId,
        };
    }

    /// <summary>
    /// A large purchase the owner left out of the freely disposable income
    /// without it being a transfer — the case where the two flags disagree.
    /// </summary>
    private static DemoBooking ExcludedPurchase(DemoPlan plan, DateOnly month)
        => plan.Book(
                DemoHousehold.CurrentAccount,
                Midnight(DemoPlan.On(month, 14)),
                -2_349_00,
                DemoParty.Payee("Möbelhaus Eichenwerk"),
                new("Wohnen", "Einrichtung"),
                DemoTransactionKind.Transfer)
            with
        {
            PaymentReference = "Rechnung 77031 Wohnzimmer",
            IsExcludedFromDisposableIncome = true,
        };

    /// <summary>A donation every December, tagged with the tax year it belongs to.</summary>
    private static IEnumerable<DemoBooking> Donations(DemoPlan plan)
    {
        for (var year = plan.From.Year; year <= plan.Until.Year; year++)
        {
            var day = new DateOnly(year, 12, 15);

            if (!plan.Covers(day))
            {
                continue;
            }

            yield return plan.Book(
                    DemoHousehold.CurrentAccount,
                    Midnight(day),
                    -100_00,
                    DemoParty.Payee("Tierheim Musterstadt e.V."),
                    new("Finanzen", "Spende"),
                    DemoTransactionKind.Transfer)
                with
            {
                PaymentReference = "Spende",
                Tags = Invariant($"Steuer {year}"),
            };
        }
    }

    private static DateTime Midnight(DateOnly day) => day.ToDateTime(TimeOnly.MinValue);

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
