using System.Data;

namespace Whitebird.App.Features.Common.Import;

public static class ExcelDataReader
{
    public static string? GetString(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static int GetInt(DataRow row, string columnName, int defaultValue = 0)
    {
        if (!row.Table.Columns.Contains(columnName))
            return defaultValue;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public static int? GetNullableInt(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return int.TryParse(value, out var result) ? result : null;
    }

    public static decimal? GetNullableDecimal(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return decimal.TryParse(value, out var result) ? result : null;
    }

    public static DateTime? GetNullableDateTime(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    public static DateTime GetDateTime(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return DateTime.MinValue;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.MinValue;
        return DateTime.TryParse(value, out var result) ? result : DateTime.MinValue;
    }

    public static bool? GetNullableBool(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (bool.TryParse(value, out var boolResult))
            return boolResult;
        if (value.Equals("1") || value.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
            return true;
        if (value.Equals("0") || value.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
            return false;

        return null;
    }

    public static T? GetEnum<T>(DataRow row, string columnName, T? defaultValue = null) where T : struct
    {
        var value = GetString(row, columnName);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse<T>(value, true, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}