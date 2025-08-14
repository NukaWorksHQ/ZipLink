using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Shared.Entities;

namespace Server.Contexts
{
    public class AppDbContext : DbContext
    {
        public required DbSet<Link> Links { get; set; }
        public required DbSet<User> Users { get; set; }
        public required DbSet<LinkStat> LinkStats { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Defining my relations
            modelBuilder.Entity<User>()
                .HasMany(m => m.Links)
                .WithOne(o => o.User)
                .HasPrincipalKey(p => p.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configuration des relations pour LinkStat
            modelBuilder.Entity<LinkStat>()
                .HasOne(ls => ls.Link)
                .WithMany(l => l.LinkStats)
                .HasForeignKey(ls => ls.LinkId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index pour optimiser les performances des requêtes de statistiques
            modelBuilder.Entity<LinkStat>()
                .HasIndex(ls => ls.LinkId);

            modelBuilder.Entity<LinkStat>()
                .HasIndex(ls => new { ls.LinkId, ls.AccessedAt });

            modelBuilder.Entity<LinkStat>()
                .HasIndex(ls => ls.IpAddress);

            // Apply configuration for all entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreatedAt")
                    .HasDefaultValueSql("GETUTCDATE()");
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("UpdatedAt")
                    .HasDefaultValueSql("GETUTCDATE()");
                modelBuilder.Entity(entityType.ClrType).Property<string>("Id").ValueGeneratedNever();
            }
        }
    }
}