namespace Application.Dtos;

public record AuthUserDto(long Id, string Email, string Role, string FullName);

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "admin"; // admin|operador
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, AuthUserDto User);

