using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Users;
using Whitebird.Infra.Features.Auth;
using Whitebird.Infra.Features.Common;
using Microsoft.Extensions.Logging;

namespace Whitebird.App.Features.Auth;

public class FirstTimeSetupService : BaseService
{
    private readonly IAuthReps _authReps;
    private readonly IGenericRepository<UsersEntity> _userRepository;

    public FirstTimeSetupService(
        IAuthReps authReps,
        IGenericRepository<UsersEntity> userRepository,
        ILogger<FirstTimeSetupService> logger) : base(logger)
    {
        _authReps = authReps;
        _userRepository = userRepository;
    }

    public async Task<bool> IsFirstTimeSetupRequiredAsync()
    {
        var adminUser = await _authReps.GetUserByUsernameAsync("admin");
        return adminUser == null || string.IsNullOrEmpty(adminUser.PasswordHash);
    }

    public async Task<ServiceResult> SetupAdminAsync(string username, string email, string fullName, string password)
    {
        var existingUser = await _authReps.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            return ServiceResult.Conflict($"User '{username}' already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new UsersEntity
        {
            Username = username,
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
            IsActive = true,
            IsLocked = false,
            CreatedDate = DateTime.Now,
            CreatedBy = "System"
        };

        await _userRepository.InsertAsync(user);

        return ServiceResult.Success("Admin user created successfully");
    }
}