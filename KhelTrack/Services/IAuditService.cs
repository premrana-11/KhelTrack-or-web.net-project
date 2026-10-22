namespace KhelTrack.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string entityType,
            string entityId,
            string details,
            string? userId = null,
            string? userEmail = null,
            string? ipAddress = null);
    }
}
