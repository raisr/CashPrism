namespace CashPrism.Application.Imports;

/// <summary>
/// What is wrong with a file that could not be imported. Carried across the
/// layers instead of a sentence, so the UI can say it in its own language; see
/// <see cref="ImportError"/> for the values that go with each code.
/// </summary>
public enum ImportErrorCode
{
    /// <summary>The file is not a workbook at all.</summary>
    NotASpreadsheet = 0,

    /// <summary>The workbook carries no worksheet.</summary>
    NoWorksheet = 1,

    /// <summary>The worksheet name carries no export date.</summary>
    SheetNameWithoutExportDate = 2,

    /// <summary>The header row lacks one or more known columns.</summary>
    MissingColumns = 3,

    /// <summary>The header row carries one or more known columns twice.</summary>
    DuplicateColumns = 4,

    /// <summary>A cell that has to carry a value is empty.</summary>
    EmptyValue = 5,

    /// <summary>A date cell carries something that is not a date.</summary>
    NotADate = 6,

    /// <summary>A money cell carries something that is not a number.</summary>
    NotAnAmount = 7,

    /// <summary>A money cell carries more than two decimal places.</summary>
    AmountNotWholeCents = 8,

    /// <summary>A money cell carries a number too large to hold in cents.</summary>
    AmountTooLarge = 9,

    /// <summary>A yes/no cell carries neither word.</summary>
    NotAFlag = 10,

    /// <summary>The split-type cell carries a word that is not a known role.</summary>
    UnknownSplitType = 11,

    /// <summary>A split part does not say which booking it is part of.</summary>
    SplitPartWithoutOriginal = 12,

    /// <summary>Two rows carry the same booking id.</summary>
    RepeatedBookingId = 13,
}
