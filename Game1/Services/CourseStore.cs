using Game1.Data;
using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Services;

/// <summary>Student actions on course content: enrol, complete lessons, rate, bookmark materials.</summary>
public class CourseStore(AppDbContext db)
{
    /// <summary>Joins a published course (no-op if already enrolled).</summary>
    public async Task<bool> EnrollAsync(int userId, int courseId)
    {
        if (!await db.Courses.AnyAsync(c => c.Id == courseId && c.Status == "published")) return false;
        if (!await db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId))
        {
            db.Enrollments.Add(new Enrollment { UserId = userId, CourseId = courseId });
            await db.SaveChangesAsync();
        }
        return true;
    }

    /// <summary>The student opened a lesson of this course — it becomes the dashboard's "Continue" course.</summary>
    public Task TouchAsync(int userId, int courseId) =>
        db.Enrollments.Where(e => e.UserId == userId && e.CourseId == courseId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.LastActivityAt, DateTime.UtcNow));

    /// <summary>Marks the lesson done. The caller has checked that the lesson is open for this student.</summary>
    public async Task CompleteLessonAsync(int userId, int lessonId, int courseId)
    {
        if (!await db.LessonCompletions.AnyAsync(c => c.UserId == userId && c.LessonId == lessonId))
        {
            db.LessonCompletions.Add(new LessonCompletion { UserId = userId, LessonId = lessonId });
            await db.SaveChangesAsync();
        }
        await TouchAsync(userId, courseId);
    }

    public Task RateAsync(int userId, int courseId, int stars) =>
        db.Enrollments.Where(e => e.UserId == userId && e.CourseId == courseId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Rating, Math.Clamp(stars, 1, 5)));

    /// <summary>Opening a material counts it as studied.</summary>
    public async Task MarkViewedAsync(int userId, int materialId)
    {
        var mark = await MarkAsync(userId, materialId);
        mark.ViewedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    /// <summary>Bookmark on / off; returns the new state.</summary>
    public async Task<bool> ToggleSavedAsync(int userId, int materialId)
    {
        var mark = await MarkAsync(userId, materialId);
        mark.Saved = !mark.Saved;
        await db.SaveChangesAsync();
        return mark.Saved;
    }

    private async Task<MaterialMark> MarkAsync(int userId, int materialId)
    {
        var mark = await db.MaterialMarks.SingleOrDefaultAsync(m => m.UserId == userId && m.MaterialId == materialId);
        if (mark == null) db.MaterialMarks.Add(mark = new MaterialMark { UserId = userId, MaterialId = materialId });
        return mark;
    }

    /// <summary>
    /// Deletes a course with its content and everyone's progress in it. Progress rows point at the
    /// course without a database cascade (SQL Server allows only one cascade path), so they go first.
    /// </summary>
    public async Task DeleteCourseAsync(int courseId)
    {
        await db.LessonCompletions.Where(c => c.Lesson.Module.CourseId == courseId).ExecuteDeleteAsync();
        await db.MaterialMarks.Where(m => m.Material.CourseId == courseId).ExecuteDeleteAsync();
        await db.Enrollments.Where(e => e.CourseId == courseId).ExecuteDeleteAsync();
        await db.Courses.Where(c => c.Id == courseId).ExecuteDeleteAsync();   // modules, lessons, materials cascade
    }
}
