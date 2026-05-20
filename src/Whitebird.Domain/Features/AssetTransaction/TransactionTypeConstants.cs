namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// Transaction type codes based on MasterData table
/// ReferenceName = "TransactionType"
/// </summary>
public static class TransactionTypeConstants
{
    public const int HANDOVER = 1;
    public const int TRANSFER = 2;
    public const int LOAN = 3;
    public const int RETURN = 4;
    public const int LOAN_RETURN = 5;
    public const int MAINTENANCE = 6;
    public const int POST_MAINTENANCE = 7;
    public const int DISPOSAL = 8;

    public static readonly int[] All = { HANDOVER, TRANSFER, LOAN, RETURN, LOAN_RETURN, MAINTENANCE, POST_MAINTENANCE, DISPOSAL };
    public static readonly int[] RequiresPairing = { LOAN_RETURN, POST_MAINTENANCE };
    public static readonly int[] CreatesPairing = { LOAN, MAINTENANCE };
    public static readonly int[] ChangesHolder = { HANDOVER, TRANSFER, LOAN, RETURN, LOAN_RETURN };
}