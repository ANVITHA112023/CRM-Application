using System.ComponentModel.DataAnnotations;

namespace CRM.API.DTOs;

// Sent by frontend when logging in
public class LoginDto
{
    [Required][EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required][MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

// Sent by frontend when registering
public class RegisterDto
{
    [Required][MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required][EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required][MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }
}