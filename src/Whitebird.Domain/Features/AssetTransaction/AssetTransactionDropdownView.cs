using System;

namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model untuk AssetTransaction dropdown (minimal data untuk pairing)
/// </summary>
public class AssetTransactionDropdownView
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string? AssetCode { get; set; }
    public int TransactionType { get; set; }
    public string? TransactionTypeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool? Approved { get; set; }
    public bool IsPaired { get; set; }
}