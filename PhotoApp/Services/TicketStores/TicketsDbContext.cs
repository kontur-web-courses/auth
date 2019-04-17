using Microsoft.EntityFrameworkCore;

namespace PhotoApp.Services.TicketStores
{
    public class TicketsDbContext : DbContext
    {
        public TicketsDbContext(DbContextOptions<TicketsDbContext> options)
            : base(options)
        {
        }

        public DbSet<TicketEntity> Tickets { get; set; }
    }
}