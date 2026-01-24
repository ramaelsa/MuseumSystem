using System.ComponentModel.DataAnnotations;

namespace MuseumSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string? Details { get; set; }
    }
}