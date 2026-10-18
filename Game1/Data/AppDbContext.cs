using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<EmailCode> EmailCodes => Set<EmailCode>();

    // Course content and student progress.
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<ModuleEntity> Modules => Set<ModuleEntity>();
    public DbSet<LessonEntity> Lessons => Set<LessonEntity>();
    public DbSet<MaterialEntity> Materials => Set<MaterialEntity>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonCompletion> LessonCompletions => Set<LessonCompletion>();
    public DbSet<MaterialMark> MaterialMarks => Set<MaterialMark>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();

        b.Entity<ExternalLogin>().HasIndex(l => new { l.Provider, l.ProviderKey }).IsUnique();
        b.Entity<ExternalLogin>().HasIndex(l => new { l.UserId, l.Provider }).IsUnique();
        b.Entity<ExternalLogin>().HasOne(l => l.User).WithMany().OnDelete(DeleteBehavior.Cascade);

        b.Entity<EmailCode>().HasIndex(c => new { c.UserId, c.Purpose }).IsUnique();
        b.Entity<EmailCode>().HasOne(c => c.User).WithMany().OnDelete(DeleteBehavior.Cascade);

        b.Entity<CourseEntity>().ToTable("Courses");
        b.Entity<CourseEntity>().HasOne(c => c.Teacher).WithMany().OnDelete(DeleteBehavior.SetNull);
        b.Entity<ModuleEntity>().ToTable("Modules");
        b.Entity<LessonEntity>().ToTable("Lessons");
        b.Entity<MaterialEntity>().ToTable("Materials");

        // Deleting a user removes their progress; deleting a course removes its content.
        // (SQL Server forbids two cascade paths to one table, so the course side of
        // progress rows uses NoAction and the app deletes them first — see DeleteCourseAsync.)
        b.Entity<Enrollment>().HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        b.Entity<Enrollment>().HasOne(e => e.User).WithMany().OnDelete(DeleteBehavior.Cascade);
        b.Entity<Enrollment>().HasOne(e => e.Course).WithMany().OnDelete(DeleteBehavior.NoAction);
        b.Entity<LessonCompletion>().HasIndex(c => new { c.UserId, c.LessonId }).IsUnique();
        b.Entity<LessonCompletion>().HasOne(c => c.User).WithMany().OnDelete(DeleteBehavior.Cascade);
        b.Entity<LessonCompletion>().HasOne(c => c.Lesson).WithMany().OnDelete(DeleteBehavior.NoAction);
        b.Entity<MaterialMark>().HasIndex(m => new { m.UserId, m.MaterialId }).IsUnique();
        b.Entity<MaterialMark>().HasOne(m => m.User).WithMany().OnDelete(DeleteBehavior.Cascade);
        b.Entity<MaterialMark>().HasOne(m => m.Material).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}
