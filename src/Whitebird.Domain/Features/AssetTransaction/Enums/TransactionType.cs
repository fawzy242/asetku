namespace Whitebird.Domain.Features.AssetTransaction.Enums;

/// <summary>
/// Defines all supported transaction types in the asset management system.
/// Used for type-safe transaction handling instead of magic strings.
/// </summary>
public static class TransactionTypeConstants
{
    /// <summary>Company → Employee (permanent assignment)</summary>
    public const string Handover = "HANDOVER";

    /// <summary>Employee A → Employee B (transfer ownership/holder)</summary>
    public const string Transfer = "TRANSFER";

    /// <summary>Company → Employee (temporary loan)</summary>
    public const string Loan = "LOAN";

    /// <summary>Employee → Company (general return)</summary>
    public const string Return = "RETURN";

    /// <summary>Return from a Loan transaction (must be paired)</summary>
    public const string LoanReturn = "LOAN_RETURN";

    /// <summary>Asset sent to maintenance</summary>
    public const string Maintenance = "MAINTENANCE";

    /// <summary>Return from maintenance (must be paired)</summary>
    public const string PostMaintenance = "POST_MAINTENANCE";

    /// <summary>Asset disposal</summary>
    public const string Disposal = "DISPOSAL";

    /// <summary>All valid transaction types</summary>
    public static readonly string[] All = { Handover, Transfer, Loan, Return, LoanReturn, Maintenance, PostMaintenance, Disposal };

    /// <summary>Transaction types that require a paired transaction</summary>
    public static readonly string[] RequiresPairing = { LoanReturn, PostMaintenance };

    /// <summary>Transaction types that create a pairing</summary>
    public static readonly string[] CreatesPairing = { Loan, Maintenance };

    /// <summary>Transaction types that change asset holder</summary>
    public static readonly string[] ChangesHolder = { Handover, Transfer, Loan, Return, LoanReturn };
}