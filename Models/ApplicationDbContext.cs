using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MuseumSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Exhibit> Exhibits { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}