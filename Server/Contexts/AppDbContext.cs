using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Entities;

namespace Server.Contexts
{
    public class AppDbContext : DbContext
    {
        public required DbSet<Link> Links { get; set; }
        public required DbSet<User> Users { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Defining my relations
            modelBuilder.Entity<User>()
                .HasMany(m => m.Links)
                .WithOne(o => o.User)
                .HasForeignKey(f => f.UserId)
                .HasPrincipalKey(p => p.Id);

            // Apply configuration for all entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreatedAt").HasDefaultValueSql("GETUTCDATE()");
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("UpdatedAt").HasDefaultValueSql("GETUTCDATE()");
                modelBuilder.Entity(entityType.ClrType).Property<string>("Id").ValueGeneratedNever();
            }
        }

    }
}
