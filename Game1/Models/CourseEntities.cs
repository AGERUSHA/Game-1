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
