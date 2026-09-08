namespace SupportPulse.Api.Models;

public enum UserRole
{
    CUSTOMER,
    AGENT,
    ADMIN
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.CUSTOMER;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}