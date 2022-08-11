using AltDict.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AltDict.Data.DbContexts
{
    public class AltDictDbContext : DbContext
    {
        public DbSet<Connection> Connections { get; set; }

        public AltDictDbContext(DbContextOptions<AltDictDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
