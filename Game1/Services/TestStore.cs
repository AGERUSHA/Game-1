using Game1.Data;
using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Services;

/// <summary>
/// Runs test attempts on the server: answers are stored one at a time and checked here,
/// the time limit is enforced here, and the score is computed here — the browser never
/// receives the right answers before choosing.
/// </summary>
public class TestStore(AppDbContext db, CourseStore courses)
{
    /// <summary>Network delay allowance on top of the time limit.</summary>
    private static readonly TimeSpan Grace = TimeSpan.FromSeconds(30);

    public static List<int> ParseAnswers(string answers, int count)
    {
        var list = answers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.TryParse(x, out var n) ? n : -1).ToList();
        while (list.Count < count) list.Add(-1);
        return list.Take(count).ToList();
    }

    private static DateTime Deadline(TestAttempt a, TestEntity t) => a.StartedAt.AddMinutes(t.Minutes) + Grace;

    /// <summary>The running attempt, or a new one. An attempt whose time ran out is finished first.</summary>
    public async Task<TestAttempt> StartOrResumeAsync(int userId, TestEntity test)
    {
        var open = await db.TestAttempts.Where(a => a.UserId == userId && a.TestId == test.Id && a.FinishedAt == null)
            .OrderByDescending(a => a.StartedAt).FirstOrDefaultAsync();
        if (open != null && DateTime.UtcNow <= Deadline(open, test)) return open;
        if (open != null) await FinishAsync(open, test);

        var total = await db.Questions.CountAsync(q => q.TestId == test.Id);
        var attempt = new TestAttempt { UserId = userId, TestId = test.Id, Total = total, Answers = string.Join(',', Enumerable.Repeat(-1, total)) };
        db.TestAttempts.Add(attempt);
        await db.SaveChangesAsync();
        return attempt;
    }

    public record AnswerResult(bool Ok, int Correct, string? Explanation, string? Error = null);

    /// <summary>Records the choice for question <paramref name="index"/> (first answer is final) and reveals the right option.</summary>
    public async Task<AnswerResult> AnswerAsync(int userId, int attemptId, int index, int option)
    {
        var a = await db.TestAttempts.Include(x => x.Test).FirstOrDefaultAsync(x => x.Id == attemptId && x.UserId == userId);
        if (a == null) return new(false, -1, null, "not-found");
        if (a.FinishedAt != null || DateTime.UtcNow > Deadline(a, a.Test)) return new(false, -1, null, "finished");

        var question = await db.Questions.Where(q => q.TestId == a.TestId).OrderBy(q => q.Order).Skip(index).FirstOrDefaultAsync();
        if (question == null) return new(false, -1, null, "bad-question");
        var options = question.Options.Split('\n').Length;
        if (option < 0 || option >= options) return new(false, -1, null, "bad-option");

        var answers = ParseAnswers(a.Answers, a.Total);
        if (answers[index] < 0)
        {
            answers[index] = option;
            a.Answers = string.Join(',', answers);
            await db.SaveChangesAsync();
        }
        return new(answers[index] == question.Correct, question.Correct, question.Explanation);
    }

    /// <summary>Scores the attempt; a pass (≥ PassScore) completes the test's lesson.</summary>
    public async Task<TestAttempt> FinishAsync(TestAttempt a, TestEntity test)
    {
        if (a.FinishedAt != null) return a;
        var correct = await db.Questions.Where(q => q.TestId == a.TestId).OrderBy(q => q.Order).Select(q => q.Correct).ToListAsync();
        var answers = ParseAnswers(a.Answers, correct.Count);
        a.Total = correct.Count;
        a.Correct = correct.Where((c, i) => answers[i] == c).Count();
        a.Score = a.Total == 0 ? 0 : (int)Math.Round(a.Correct * 100.0 / a.Total);
        var deadline = Deadline(a, test) - Grace;
        a.FinishedAt = DateTime.UtcNow < deadline ? DateTime.UtcNow : deadline;
        await db.SaveChangesAsync();

        if (a.Score >= test.PassScore && test.LessonId is int lessonId)
            await courses.CompleteLessonAsync(a.UserId, lessonId, test.CourseId);
        return a;
    }
}
