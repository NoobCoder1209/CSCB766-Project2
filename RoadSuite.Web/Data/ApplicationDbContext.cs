using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoadSuite.Web.Models;

namespace RoadSuite.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<DealerProfile> DealerProfiles => Set<DealerProfile>();
    public DbSet<ModerationFeedback> ModerationFeedback => Set<ModerationFeedback>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DealerProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        builder.Entity<DealerProfile>()
            .HasOne(p => p.User)
            .WithOne(u => u.DealerProfile)
            .HasForeignKey<DealerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Car>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Cars)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Car>()
            .HasOne(c => c.DealerProfile)
            .WithMany(p => p.Cars)
            .HasForeignKey(c => c.DealerProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ModerationFeedback>()
            .HasOne(m => m.Car)
            .WithMany(c => c.ModerationFeedback)
            .HasForeignKey(m => m.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ModerationFeedback>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.ModeratorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasOne(n => n.Car)
            .WithMany(c => c.Notifications)
            .HasForeignKey(n => n.CarId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
