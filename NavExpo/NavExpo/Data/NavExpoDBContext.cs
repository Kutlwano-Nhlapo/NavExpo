using Microsoft.EntityFrameworkCore;
using NavExpo.Models;

namespace NavExpo.Data
{
    public class NavExpoDBContext : DbContext
    {
        public NavExpoDBContext(DbContextOptions<NavExpoDBContext> options) : base(options)
        {
        }
        public DbSet<EventForm> Expos { get; set; }
        public DbSet<Stall> Stalls { get; set; }

    }
}
