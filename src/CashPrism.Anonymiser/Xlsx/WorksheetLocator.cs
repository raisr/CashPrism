using System.IO.Compression;
using System.Xml.Linq;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Finds the zip entry name of the workbook's worksheet part. The FinanzGuru
/// export carries exactly one worksheet, but its entry is never named
/// <c>sheet1.xml</c> by convention alone — <c>workbook.xml</c> and its
/// relationships part are the only reliable source, so this reads those rather
/// than guessing a path.
/// </summary>
public static class WorksheetLocator
{
    private static readonly XNamespace SpreadsheetNamespace =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    private static readonly XNamespace DocumentRelationshipsNamespace =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static readonly XNamespace PackageRelationshipsNamespace =
        "http://schemas.openxmlformats.org/package/2006/relationships";

    /// <summary>
    /// Resolves the zip entry name of the first worksheet, e.g.
    /// <c>xl/worksheets/sheet1.xml</c>. <see langword="null"/> when the archive
    /// is not shaped like a workbook — <c>workbook.xml</c>, its relationships
    /// part, or a first sheet with a resolvable target is missing.
    /// </summary>
    public static string? Locate(ZipArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);

        var workbookEntry = archive.GetEntry("xl/workbook.xml");
        var relationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");

        if (workbookEntry is null || relationshipsEntry is null)
        {
            return null;
        }

        var workbook = LoadXml(workbookEntry);
        var firstSheet = workbook.Descendants(SpreadsheetNamespace + "sheet").FirstOrDefault();
        var relationshipId = (string?)firstSheet?.Attribute(DocumentRelationshipsNamespace + "id");

        if (relationshipId is null)
        {
            return null;
        }

        var relationships = LoadXml(relationshipsEntry);
        var target = relationships
            .Descendants(PackageRelationshipsNamespace + "Relationship")
            .FirstOrDefault(relationship => (string?)relationship.Attribute("Id") == relationshipId)
            ?.Attribute("Target")?.Value;

        return target is null ? null : $"xl/{target}";
    }

    private static XDocument LoadXml(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();

        return XDocument.Load(stream);
    }
}
