using Microsoft.EntityFrameworkCore;
using PFM.API.Entities;

namespace PFM.API.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Transactions> Transactions { get; set; } = null!;

       

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
     
    }
}
