namespace Whitebird.Domain.Features.Common;

/// <summary>
/// Password complexity rules applied to new-password fields
/// (<see cref="ChangePasswordRequest"/>, <see cref="ResetPasswordRequest"/>,
/// <see cref="ResetPasswordWithTokenRequest"/>).
///
/// Existing passwords continue to verify against their BCrypt hash unchanged;
/// login does not re-check complexity. The rule applies only when the user sets
/// a new password.
///
/// Kept in Domain (not App) because it is consumed by DataAnnotation attributes
/// on Domain view models. Moving it to App would introduce an App → Domain → App
/// dependency cycle through the annotated classes.
/// </summary>
public static class PasswordRules
{
    /// <summary>
    /// Regex enforcing: at least 8 characters, at least one letter, at least one digit.
    /// Symbols are allowed but not required.
    /// </summary>
    public const string ComplexityPattern = @"^(?=.*[A-Za-z])(?=.*\d).{8,}$";

    /// <summary>
    /// Human-readable description of <see cref="ComplexityPattern"/>. Used verbatim
    /// by DataAnnotation ErrorMessages so the frontend can display it directly.
    /// </summary>
    public const string ComplexityMessage = "Password must be at least 8 characters and contain at least one letter and one digit";

    /// <summary>
    /// Runtime validator with the same semantics as the DataAnnotation regex.
    /// Useful for services that receive passwords as plain strings (e.g. a future
    /// admin-driven user-creation flow) without going through ModelState.
    /// </summary>
    public static bool IsValid(string? password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(password, ComplexityPattern);
    }
}