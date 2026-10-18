using Game1.Data;
using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Services;

/// <summary>
/// Hand-in of assignments. Files go to App_Data/submissions (outside wwwroot, so they are
/// never public) and are served by controllers that check who is asking.
/// </summary>
public class AssignmentStore(AppDbContext db, CourseStore courses, IWebHostEnvironment env)
{
    public const int MaxFiles = 10;

    private string Root => Path.Combine(env.ContentRootPath, "App_Data", "submissions");

    public string PathOf(SubmissionFile f) => Path.Combine(Root, f.SubmissionId.ToString(), f.StoredName);

    /// <summary>Removes a user's uploaded files from disk (their rows go with the user by cascade).</summary>
    public async Task DeleteUserFilesAsync(int userId)
    {
        var ids = await db.Submissions.Where(s => s.UserId == userId).Select(s => s.Id).ToListAsync();
        foreach (var dir in ids.Select(id => Path.Combine(Root, id.ToString())).Where(Directory.Exists))
            Directory.Delete(dir, recursive: true);
    }

    public record SubmitResult(bool Ok, string? Error = null);

    /// <summary>
    /// First hand-in, or a new version after the teacher returned the work (old files are replaced).
    /// A submission waiting for review or already graded can't be changed.
    /// </summary>
    public async Task<SubmitResult> SubmitAsync(int userId, AssignmentEntity a, string? comment, IReadOnlyList<IFormFile> files, DateTime? due)
    {
        var sub = await db.Submissions.Include(s => s.Files).FirstOrDefaultAsync(s => s.AssignmentId == a.Id && s.UserId == userId);
        if (sub is { Status: "submitted" or "graded" }) return new(false, "locked");
        if (files.Count == 0) return new(false, "no-files");
        if (files.Count > MaxFiles) return new(false, "too-many");

        var allowed = a.Formats.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(f => f.ToLowerInvariant()).ToHashSet();
        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (allowed.Count > 0 && !allowed.Contains(ext)) return new(false, "bad-type");
            if (f.Length > a.MaxMb * 1024L * 1024) return new(false, "too-big");
            if (f.Length == 0) return new(false, "empty");
        }

        if (sub == null)
        {
            db.Submissions.Add(sub = new SubmissionEntity { AssignmentId = a.Id, UserId = userId });
            await db.SaveChangesAsync();   // need the id for the folder
        }
        else
        {
            foreach (var old in sub.Files) File.Delete(PathOf(old));
            db.SubmissionFiles.RemoveRange(sub.Files);
            sub.Files.Clear();
        }

        var dir = Path.Combine(Root, sub.Id.ToString());
        Directory.CreateDirectory(dir);
        foreach (var f in files)
        {
            var stored = $"{Guid.NewGuid():N}{Path.GetExtension(f.FileName).ToLowerInvariant()}";
            await using (var s = File.Create(Path.Combine(dir, stored))) await f.CopyToAsync(s);
            sub.Files.Add(new SubmissionFile { FileName = Path.GetFileName(f.FileName), StoredName = stored, Size = f.Length });
        }

        sub.Comment = (comment ?? "").Trim() is { Length: > 1000 } c ? c[..1000] : (comment ?? "").Trim();
        sub.Status = "submitted";
        sub.SubmittedAt = DateTime.UtcNow;
        sub.Late = due < DateTime.UtcNow;
        sub.Score = null;
        sub.GradedAt = null;
        await db.SaveChangesAsync();

        // Handing the work in is what the assignment lesson asks for.
        if (a.LessonId is int lessonId) await courses.CompleteLessonAsync(userId, lessonId, a.CourseId);
        return new(true);
    }
}
