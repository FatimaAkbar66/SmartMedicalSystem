using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.Entities;

namespace SmartMedicalSystem.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(
            int userId,
            string action,
            string entity,
            string? details = null,
            string? ipAddress = null)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserID = userId,
                Action = action,
                Entity = entity,
                Details = details,
                IPAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}