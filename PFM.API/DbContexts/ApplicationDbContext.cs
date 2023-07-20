using Microsoft.EntityFrameworkCore;
using PFM.API.Entities;

namespace PFM.API.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {

        }
        public DbSet<Transactions> Transactions { get; set; } = null!;
        public DbSet< Categories>  Categories { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CatCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
