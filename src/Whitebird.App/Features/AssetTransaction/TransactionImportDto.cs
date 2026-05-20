namespace Whitebird.App.Features.AssetTransaction;

public class TransactionImportDto
{
    public string AssetCode { get; set; } = default!;
    public string TransactionTypeName { get; set; } = default!;
    public string? FromEmployeeCode { get; set; }
    public string? ToEmployeeCode { get; set; }
    public string? ToOfficeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
    public string? ConditionBeforeName { get; set; }
    public string? ConditionAfterName { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public int? FromAssetTransactionId { get; set; }
}