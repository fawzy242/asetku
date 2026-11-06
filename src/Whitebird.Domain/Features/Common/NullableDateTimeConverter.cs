using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Whitebird.Domain.Features.Common;

/// <summary>
/// System.Text.Json converter for <see cref="DateTime"/>? that treats empty
/// strings and whitespace as <c>null</c>.
///
/// Motivation
/// ----------
/// The transaction DTOs (<c>AssetTransactionCreateViewModel</c>,
/// <c>AssetTransactionUpdateFormDto</c>,
/// <c>AssetTransactionUpdateApprovalFormDto</c>) are union types: one DTO
/// covers every transaction type. <c>ExpectedLoanReturnDate</c> is required
/// only for LOAN; for HANDOVER / TRANSFER / MAINTENANCE / DISPOSAL /
/// OPERATIONAL_OFFICE it is unused. An HTML date input on those forms
/// serializes to <c>""</c> when left blank — and System.Text.Json rejects
/// <c>""</c> for <c>DateTime?</c> by default, producing a 400 before the
/// business logic ever runs.
///
/// This converter is applied per-field (via <c>[JsonConverter]</c>) on the
/// affected optional dates. It is NOT registered globally. Non-nullable
/// <see cref="DateTime"/> properties remain strict (a required field
/// must carry a real value).
///
/// Behavior
/// --------
/// Input        → Result
/// null         → null
/// "" / "   "   → null
/// "2026-11-05" → 2026-11-05 00:00:00
/// "2026-11-05T14:30:00" → 2026-11-05 14:30:00 (local kind)
/// "garbage"    → JsonException with a clear message
/// </summary>
public sealed class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.String:
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    // "" / "   " → null. Matches the behavior of an empty
                    // HTML date input on an optional field.
                    return null;
                }

                if (DateTime.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out var parsed))
                {
                    return parsed;
                }

                throw new JsonException(
                    $"Unable to parse '{raw}' as a DateTime. Expected ISO-8601 " +
                    $"(e.g. '2026-11-05' or '2026-11-05T14:30:00Z') or null/empty.");

            default:
                throw new JsonException(
                    $"Unexpected token {reader.TokenType} when parsing nullable DateTime. " +
                    $"Expected String or Null.");
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("O", CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}