using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Dtos;
using Application.Services;
using Domain.Entities;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class EfAuthService(ApplicationDbContext db, IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private const long DefaultCompanyId = 1;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var exists = await db.Users.AnyAsync(u => u.CompanyId == DefaultCompanyId && u.Email == req.Email, ct);
        if (exists) throw new InvalidOperationException("Email ya registrado");

        var user = new AppUser
        {
            CompanyId = DefaultCompanyId,
            Email = req.Email.Trim().ToLowerInvariant(),
            FullName = req.FullName.Trim(),
            Role = string.IsNullOrWhiteSpace(req.Role) ? "admin" : req.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = SimplePasswordHasher.HashPassword(req.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = GenerateToken(user);
        return new AuthResponse(token, new AuthUserDto(user.Id, user.Email, user.Role, user.FullName ?? ""));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.CompanyId == DefaultCompanyId && x.Email == req.Email.ToLower(), ct);
        if (u is null || !u.IsActive)
            throw new InvalidOperationException("Credenciales inválidas");

        var verify = SimplePasswordHasher.Verify(req.Password, u.PasswordHash);
        if (!verify)
            throw new InvalidOperationException("Credenciales inválidas");

        var token = GenerateToken(u);
        return new AuthResponse(token, new AuthUserDto(u.Id, u.Email, u.Role, u.FullName ?? ""));
    }

    private string GenerateToken(AppUser u)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Email, u.Email),
            new(ClaimTypes.Role, u.Role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
