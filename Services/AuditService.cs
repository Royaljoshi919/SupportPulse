using SupportPulse.Api.Data;
using SupportPulse.Api.Models;

namespace SupportPulse.Api.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? userId, string action, string entity, string? entityId = null, string? oldValue = null, string? newValue = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}