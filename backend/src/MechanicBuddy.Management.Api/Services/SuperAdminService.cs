using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using BCrypt.Net;

namespace MechanicBuddy.Management.Api.Services;

public class SuperAdminService
{
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly ILogger<SuperAdminService> _logger;

    public SuperAdminService(
        ISuperAdminRepository superAdminRepository,
        ILogger<SuperAdminService> logger)
    {
        _superAdminRepository = superAdminRepository;
        _logger = logger;
    }

    public async Task<SuperAdmin?> AuthenticateAsync(string email, string password)
    {
        var admin = await _superAdminRepository.GetByEmailAsync(email);
        if (admin == null || !admin.IsActive)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            return null;
        }

        // Update last login
        await _superAdminRepository.UpdateLastLoginAsync(admin.Id);
        admin.LastLoginAt = DateTime.UtcNow;

        _logger.LogInformation("Successful login for {Email}", email);

        return admin;
    }

    public async Task<SuperAdmin> CreateAdminAsync(string email, string password, string name, string role = "admin")
    {
        var existingAdmin = await _superAdminRepository.GetByEmailAsync(email);
        if (existingAdmin != null)
        {
            throw new InvalidOperationException("Admin with this email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var admin = new SuperAdmin
        {
            Email = email,
            PasswordHash = passwordHash,
            Name = name,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _superAdminRepository.CreateAsync(admin);
        admin.Id = id;

        _logger.LogInformation("Created new super admin: {Email}", email);

        return admin;
    }

    public async Task<SuperAdmin?> GetByIdAsync(int id)
    {
        return await _superAdminRepository.GetByIdAsync(id);
    }

    public async Task<SuperAdmin?> GetByEmailAsync(string email)
    {
        return await _superAdminRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<SuperAdmin>> GetAllAsync()
    {
        return await _superAdminRepository.GetAllAsync();
    }

    public async Task<bool> UpdatePasswordAsync(int id, string currentPassword, string newPassword)
    {
        var admin = await _superAdminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, admin.PasswordHash))
        {
            return false;
        }

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return await _superAdminRepository.UpdateAsync(admin);
    }

    public async Task<bool> DeactivateAdminAsync(int id)
    {
        var admin = await _superAdminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            return false;
        }

        admin.IsActive = false;
        return await _superAdminRepository.UpdateAsync(admin);
    }

    public async Task<bool> ActivateAdminAsync(int id)
    {
        var admin = await _superAdminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            return false;
        }

        admin.IsActive = true;
        return await _superAdminRepository.UpdateAsync(admin);
    }

    public async Task<bool> DeleteAdminAsync(int id)
    {
        return await _superAdminRepository.DeleteAsync(id);
    }
}
