namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Transaction type constants yang mereferensikan MasterData table.
/// ReferenceName = "TransactionType"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// Jika ada perubahan di MasterData, constants ini harus diupdate.
/// </summary>
public static class TransactionTypeConstants
{
    // Transaction types berdasarkan MasterData
    public const int HANDOVER = 1;           // HANDOVER (Company → Employee)
    public const int TRANSFER = 2;           // TRANSFER (Employee → Employee)
    public const int LOAN = 3;               // LOAN (Company → Employee)
    public const int RETURN = 4;             // RETURN (Employee → Company)
    public const int LOAN_RETURN = 5;        // LOAN RETURN (Paired with LOAN)
    public const int MAINTENANCE = 6;        // MAINTENANCE
    public const int POST_MAINTENANCE = 7;   // POST-MAINTENANCE (Paired with MAINTENANCE)
    public const int DISPOSAL = 8;           // DISPOSAL

    /// <summary>
    /// Transaction types that require a paired transaction (fromAssetTransactionId)
    /// </summary>
    public static readonly int[] RequiresPairedTransaction = { LOAN_RETURN, POST_MAINTENANCE };

    /// <summary>
    /// Transaction types that create a pairing relationship (can be used as pair source)
    /// </summary>
    public static readonly int[] CreatesPairing = { LOAN, MAINTENANCE };

    /// <summary>
    /// Transaction types that require ToEmployeeId (receiver)
    /// </summary>
    public static readonly int[] RequiresToEmployee = { HANDOVER, TRANSFER, LOAN };

    /// <summary>
    /// Transaction types that require FromEmployeeId (sender)
    /// </summary>
    public static readonly int[] RequiresFromEmployee = { TRANSFER, RETURN, LOAN_RETURN };

    /// <summary>
    /// Transaction types that are considered "active" (asset is currently held/used)
    /// </summary>
    public static readonly int[] ActiveTransactionTypes = { HANDOVER, TRANSFER, LOAN, MAINTENANCE };

    /// <summary>
    /// Check if a transaction type requires a paired transaction
    /// </summary>
    public static bool RequiresPaired(int transactionType) 
        => Array.IndexOf(RequiresPairedTransaction, transactionType) >= 0;

    /// <summary>
    /// Check if a transaction type creates a pairing relationship
    /// </summary>
    public static bool CreatesPairingRelationship(int transactionType) 
        => Array.IndexOf(CreatesPairing, transactionType) >= 0;
}