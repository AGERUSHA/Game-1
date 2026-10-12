using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();

        b.Entity<ExternalLogin>().HasIndex(l => new { l.Provider, l.ProviderKey }).IsUnique();
        b.Entity<ExternalLogin>().HasIndex(l => new { l.UserId, l.Provider }).IsUnique();
        b.Entity<ExternalLogin>().HasOne(l => l.User).WithMany().OnDelete(DeleteBehavior.Cascade);
    }
}
