namespace Whitebird.App.Features.Common.Import;

public class ImportResult
{
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int TotalRows { get; set; }
    public List<ImportError> Errors { get; set; } = new();
    public List<ImportWarning> Warnings { get; set; } = new();

    public bool HasErrors => ErrorCount > 0;
    public bool IsCompleteSuccess => SuccessCount > 0 && ErrorCount == 0;

    public static ImportResult Success(int count, int totalRows)
    {
        return new ImportResult
        {
            SuccessCount = count,
            ErrorCount = 0,
            TotalRows = totalRows
        };
    }

    public static ImportResult Failure(List<ImportError> errors, int totalRows)
    {
        return new ImportResult
        {
            SuccessCount = 0,
            ErrorCount = errors.Count,
            TotalRows = totalRows,
            Errors = errors
        };
    }

    public void AddError(int rowNumber, string column, string message, string? value = null)
    {
        Errors.Add(new ImportError
        {
            RowNumber = rowNumber,
            Column = column,
            Message = message,
            Value = value
        });
        ErrorCount++;
    }

    public void AddWarning(int rowNumber, string column, string message, string? value = null)
    {
        Warnings.Add(new ImportWarning
        {
            RowNumber = rowNumber,
            Column = column,
            Message = message,
            Value = value
        });
    }
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string Column { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Value { get; set; }
}

public class ImportWarning
{
    public int RowNumber { get; set; }
    public string Column { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Value { get; set; }
}