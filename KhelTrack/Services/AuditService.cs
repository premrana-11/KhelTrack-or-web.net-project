using System.Security.Claims;
using KhelTrack.Data;
using KhelTrack.Models;

namespace KhelTrack.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(
            string action,
            string entityType,
            string entityId,
            string details,
            string? userId = null,
            string? userEmail = null,
            string? ipAddress = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (string.IsNullOrEmpty(userId) && httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                if (string.IsNullOrEmpty(userEmail) && httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    userEmail = httpContext.User.FindFirstValue(ClaimTypes.Email) ?? httpContext.User.Identity?.Name;
                }

                if (string.IsNullOrEmpty(ipAddress) && httpContext != null)
                {
                    ipAddress = httpContext.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                }

                var audit = new AuditLog
                {
                    UserId = userId ?? "SYSTEM",
                    UserEmail = userEmail ?? "system@kheltrack.local",
                    Action = action.ToUpperInvariant(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    TimestampUtc = DateTime.UtcNow,
                    IpAddress = ipAddress ?? "127.0.0.1"
                };

                _context.AuditLogs.Add(audit);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Never let audit log failure crash the user workflow, but log error
                _logger.LogError(ex, "Failed to persist audit log for Action: {Action}, Entity: {EntityType}:{EntityId}", action, entityType, entityId);
            }
        }
    }
}
