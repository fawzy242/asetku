namespace Whitebird.App.Features.Common.Service;

public interface ICurrentUserService
{
    int? UserId { get; }
    string Username { get; }
    string FullName { get; }
    string Email { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
    string GetDisplayName();
}