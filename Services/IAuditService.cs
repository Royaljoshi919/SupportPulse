namespace SupportPulse.Api.Services
{
    public interface IAuditService
    {
        Task LogAsync(int? userId, string action, string entity, string? entityId = null, string? oldValue = null, string? newValue = null);
    }
}