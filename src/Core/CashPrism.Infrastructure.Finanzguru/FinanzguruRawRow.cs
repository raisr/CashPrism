using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Renders one row of an export as the JSON a raw row stores.
/// </summary>
/// <remarks>
/// <para>
/// The rendering has to be <b>deterministic</b>: the same row has to produce the
/// same bytes on every import, because comparing this text with the stored one is
/// what decides whether a booking changed. A serialiser that reordered the keys,
/// or indented differently between versions, would make every unchanged row look
/// changed and store the whole export again on every import.
/// </para>
/// <para>
/// So the keys are written in the export's own column order rather than in
/// whatever order the row happened to be built in, unknown columns follow in
/// ordinal order, and nothing is indented.
/// </para>
/// </remarks>
public static class FinanzguruRawRow
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = false,

        // The payment reference is free text and carries whatever the bank sent:
        // ampersands, angle brackets, umlauts. Escaping them by default would be
        // correct but unreadable in a database browser, and nothing here is ever
        // written into HTML.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Renders <paramref name="values"/> as a JSON object.
    /// </summary>
    /// <param name="values">Every column of the row by header name.</param>
    /// <returns>The row as JSON, with the keys in a stable order.</returns>
    public static string ToJson(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        using var buffer = new MemoryStream();

        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            writer.WriteStartObject();

            foreach (var column in FinanzguruColumns.All)
            {
                if (values.TryGetValue(column, out var known))
                {
                    writer.WriteString(column, known);
                }
            }

            var unknownColumns = values.Keys
                .Where(key => !FinanzguruColumns.All.Contains(key, StringComparer.Ordinal))
                .Order(StringComparer.Ordinal);

            foreach (var column in unknownColumns)
            {
                writer.WriteString(column, values[column]);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
