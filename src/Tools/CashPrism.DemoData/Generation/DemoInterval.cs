namespace CashPrism.DemoData.Generation;

/// <summary>
/// How often a contract recurs, as the German word <c>Analyse-Vertragsturnus</c>
/// carries and the number of months it stands for. The five words are the ones
/// a real export was measured to use.
/// </summary>
/// <param name="Word">The word the export writes.</param>
/// <param name="Months">The number of months between two bookings.</param>
public sealed record DemoInterval(string Word, int Months)
{
    /// <summary>Every month.</summary>
    public static readonly DemoInterval Monthly = new("monatlich", 1);

    /// <summary>Every second month.</summary>
    public static readonly DemoInterval BiMonthly = new("zweimonatlich", 2);

    /// <summary>Every quarter.</summary>
    public static readonly DemoInterval Quarterly = new("vierteljaehrlich", 3);

    /// <summary>Every half year.</summary>
    public static readonly DemoInterval HalfYearly = new("halbjaehrlich", 6);

    /// <summary>Every year.</summary>
    public static readonly DemoInterval Yearly = new("jaehrlich", 12);
}
