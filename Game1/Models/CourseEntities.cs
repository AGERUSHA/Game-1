using System.ComponentModel.DataAnnotations;

namespace Game1.Models;

// ------------------------------------------------------------------
// Course content stored in SQL Server. Views keep using the records in
// EduModels.cs (Course, CourseModule, Lesson, Material); SqlEduDataService
// maps these entities to them, adding the signed-in student's progress.
// ------------------------------------------------------------------

/// <summary>A course: title, cover, author. Status: draft | review | published | archived.</summary>
public class CourseEntity
{
    public int Id { get; set; }
    [MaxLength(200)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Subtitle { get; set; } = "";
    [MaxLength(100)] public string Category { get; set; } = "";
    [MaxLength(2000)] public string Description { get; set; } = "";

    /// <summary>"What you'll learn" — one item per line.</summary>
    [MaxLength(2000)] public string Outcomes { get; set; } = "";

    /// <summary>Author account; null for the demo courses seeded on first start.</summary>
    public int? TeacherId { get; set; }
    public User? Teacher { get; set; }
    /// <summary>Shown on the card (kept even if the author account is deleted).</summary>
    [MaxLength(200)] public string TeacherName { get; set; } = "";

    /// <summary>Cover gradient (violet | indigo | fuchsia | blue | plum | deep) and Lucide icon.</summary>
    [MaxLength(20)] public string Cover { get; set; } = "violet";
    [MaxLength(50)] public string Icon { get; set; } = "book-open";
    [MaxLength(50)] public string Level { get; set; } = "";
    public int Hours { get; set; }

    [MaxLength(20)] public string Status { get; set; } = "published";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ModuleEntity> Modules { get; set; } = [];
    public List<MaterialEntity> Materials { get; set; } = [];
}

public class ModuleEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = null!;
    [MaxLength(200)] public string Title { get; set; } = "";
    public int Order { get; set; }
    public List<LessonEntity> Lessons { get; set; } = [];
}

/// <summary>Type: video | article | test | assignment.</summary>
public class LessonEntity
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public ModuleEntity Module { get; set; } = null!;
    [MaxLength(200)] public string Title { get; set; } = "";
    [MaxLength(20)] public string Type { get; set; } = "article";
    /// <summary>"12:40", "10 мин", "1 ч" — as the author wrote it.</summary>
    [MaxLength(30)] public string Duration { get; set; } = "";
    public int Order { get; set; }
    /// <summary>Lesson text (paragraphs separated by a blank line).</summary>
    public string Body { get; set; } = "";
    /// <summary>YouTube / video link for video lessons.</summary>
    [MaxLength(500)] public string? VideoUrl { get; set; }
}

/// <summary>Library item of a course. Type: video | pdf | article | presentation.</summary>
public class MaterialEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = null!;
    [MaxLength(200)] public string Title { get; set; } = "";
    [MaxLength(20)] public string Type { get; set; } = "article";
    /// <summary>"19:35", "2.4 MB", "8 мин чтения", "24 слайда".</summary>
    [MaxLength(50)] public string Meta { get; set; } = "";
    public string Body { get; set; } = "";
    [MaxLength(500)] public string? VideoUrl { get; set; }
    /// <summary>Uploaded file under wwwroot/uploads/materials (null until the teacher uploads one).</summary>
    [MaxLength(300)] public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>A student joined a course (and maybe rated it 1–5).</summary>
public class Enrollment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = null!;
    public int? Rating { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    /// <summary>Last lesson opened or completed — "Continue" on the dashboard goes to the most recent course.</summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
}

public class LessonCompletion
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int LessonId { get; set; }
    public LessonEntity Lesson { get; set; } = null!;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Student × material: opened at least once ("studied") and / or bookmarked.</summary>
public class MaterialMark
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int MaterialId { get; set; }
    public MaterialEntity Material { get; set; } = null!;
    public bool Saved { get; set; }
    public DateTime? ViewedAt { get; set; }
}

// ------------------------------------------------------------------ tests

/// <summary>A quiz of a course; LessonId links it to a "test" lesson (passing it completes the lesson).</summary>
public class TestEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = null!;
    public int? LessonId { get; set; }
    [MaxLength(200)] public string Title { get; set; } = "";
    public int Minutes { get; set; } = 15;
    /// <summary>easy | medium | hard</summary>
    [MaxLength(20)] public string Difficulty { get; set; } = "medium";
    /// <summary>Percent needed to pass.</summary>
    public int PassScore { get; set; } = 70;
    public List<QuestionEntity> Questions { get; set; } = [];
}

public class QuestionEntity
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public TestEntity Test { get; set; } = null!;
    public int Order { get; set; }
    [MaxLength(1000)] public string Text { get; set; } = "";
    /// <summary>Answer options, one per line (2–6).</summary>
    [MaxLength(2000)] public string Options { get; set; } = "";
    /// <summary>Index of the right option.</summary>
    public int Correct { get; set; }
    [MaxLength(100)] public string Topic { get; set; } = "";
    [MaxLength(2000)] public string? Code { get; set; }
    /// <summary>Why the right answer is right — shown in the review after the test.</summary>
    [MaxLength(2000)] public string? Explanation { get; set; }
}

/// <summary>
/// One run of a test. Answers are recorded one by one on the server (the browser only learns
/// the right option after answering), so the score can't be faked.
/// </summary>
public class TestAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int TestId { get; set; }
    public TestEntity Test { get; set; } = null!;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    /// <summary>Chosen option per question in test order, comma-separated; -1 = not answered.</summary>
    [MaxLength(500)] public string Answers { get; set; } = "";
    public int Correct { get; set; }
    public int Total { get; set; }
    /// <summary>Percent, set when finished.</summary>
    public int Score { get; set; }
}

// ------------------------------------------------------------------ assignments

/// <summary>Practical work. Deadline: DueAt (fixed date) or DueDays after the student enrolled.</summary>
public class AssignmentEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = null!;
    public int? LessonId { get; set; }
    [MaxLength(200)] public string Title { get; set; } = "";
    [MaxLength(4000)] public string Description { get; set; } = "";
    public int MaxScore { get; set; } = 100;
    public int Xp { get; set; } = 100;
    public DateTime? DueAt { get; set; }
    public int? DueDays { get; set; }
    /// <summary>Allowed extensions, comma-separated: ".zip,.pdf".</summary>
    [MaxLength(200)] public string Formats { get; set; } = ".zip,.pdf,.docx";
    public int MaxMb { get; set; } = 25;
}

/// <summary>Status: submitted | returned (needs rework) | graded.</summary>
public class SubmissionEntity
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public AssignmentEntity Assignment { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    [MaxLength(1000)] public string Comment { get; set; } = "";
    [MaxLength(20)] public string Status { get; set; } = "submitted";
    public int? Score { get; set; }
    [MaxLength(4000)] public string? Feedback { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GradedAt { get; set; }
    public int? GraderId { get; set; }
    [MaxLength(200)] public string? GraderName { get; set; }
    public bool Late { get; set; }
    public List<SubmissionFile> Files { get; set; } = [];
}

/// <summary>Uploaded file, stored outside wwwroot (App_Data/submissions) and served only to the student and the course's teacher.</summary>
public class SubmissionFile
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public SubmissionEntity Submission { get; set; } = null!;
    [MaxLength(260)] public string FileName { get; set; } = "";
    [MaxLength(100)] public string StoredName { get; set; } = "";
    public long Size { get; set; }
}
