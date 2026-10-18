namespace Game1.Models;

// ------------------------------------------------------------------
// Domain models for the UI prototype.
// They intentionally mirror future SQL Server tables, so the mock data
// service can later be replaced with a real repository without touching views.
// ------------------------------------------------------------------

public record UserProfile(
    string FirstName, string LastName, string Email, string Role, string RoleTitle,
    int Xp, int Level, int LevelStartXp, int NextLevelXp,
    int Courses, int AverageScore, int Streak, int CompletedCourses, int Certificates)
{
    public string? AvatarUrl { get; init; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => string.Concat(new[] { FirstName, LastName }.Where(s => s.Length > 0).Select(s => char.ToUpper(s[0])));
    public int LevelPercent => (int)Math.Round((Xp - LevelStartXp) * 100.0 / (NextLevelXp - LevelStartXp));
}

public record Course(
    int Id, string Title, string Subtitle, string Category, string Description, string Teacher,
    int Lessons, int Progress, double Rating, int Students, string Cover, string Icon,
    string Duration, string Level, string Status = "published", int Completion = 0, bool Enrolled = true)
{
    /// <summary>"What you'll learn" items.</summary>
    public string[] Outcomes { get; init; } = [];
    /// <summary>The signed-in student's own rating (1–5), if given.</summary>
    public int? MyRating { get; init; }
    public int CompletedLessons => (int)Math.Round(Lessons * Progress / 100.0);
}

public record Lesson(string Title, string Type, string Duration, string Status, int Id = 0); // completed | current | locked
public record CourseModule(string Title, List<Lesson> Lessons)
{
    public int Done => Lessons.Count(l => l.Status == "completed");
}

public record Material(string Title, string Type, string Course, string Meta, string Date, int Id = 0, bool Saved = false, bool Viewed = false);

/// <summary>Lesson page: the lesson, where it sits in the course, and its neighbours.</summary>
public record LessonPage(Course Course, string Module, Lesson Lesson, string Body, string? VideoEmbed,
    int? PrevId, int? NextId, int Number, int Total);

/// <summary>Material page: text / video / file of one library item.</summary>
public record MaterialPage(Material Material, int CourseId, string Body, string? VideoEmbed, string? FileUrl);

public record TestItem(int Id, string Title, string Course, int Questions, int Minutes, int? BestScore, string Status, int Attempts, string Difficulty);

public record Question(string Text, string[] Options, int Correct, string Topic, string? Code = null);

public record Assignment(string Title, string Course, string Deadline, string Status, int? Score, int MaxScore, int DaysLeft, int Id = 0);

/// <summary>Assignment page: brief, files, review (Views/Student/Assignment.cshtml).</summary>
public record AssignmentDetail(
    Assignment Info, string Module, string Description, int Xp, string[] Formats, int MaxMb,
    string Teacher, string TeacherTitle, string? Feedback, string? Tip, string? Comment,
    List<SubmittedFile> Files, string? ReviewedAgo);

public record SubmittedFile(string Name, string Size, string Icon);

public record Recommendation(string Icon, string Title, string Text, string Tag, string Action, string Href);

public record Activity(string Icon, string Text, string Time, int? Xp, string Kind);

public record Achievement(string Emoji, string Title, string Description, bool Unlocked, int Progress, int Target, string? Date, string Rarity);

public record XpEvent(string Title, string Source, int Amount, string Date, string Icon);

public record StudentRow(int Id, string Name, string Email, string Group, int Courses, int Progress, int AvgScore, int Xp, string Status, string LastActive);

public record ReportItem(string Key, string Icon, string Title, string Description, string Updated, string[] Formats);

public record AdminUser(string Name, string Email, string Role, string Status, string Joined, string LastSeen, int Id = 0, bool EmailConfirmed = true);

public record Notification(string Icon, string Title, string Time, bool Unread, string Kind);

/// <summary>Notifications page item. Category: grades | deadlines | ai | achievements. Day: today | yesterday | earlier.</summary>
public record FeedItem(string Category, string Icon, string Label, string Title, string Text, string Time, bool Unread,
    string Day, string? Action = null, string? Href = null, int Xp = 0);

public record ChatMessage(string Author, string Text, string Time); // "ai" | "user"

public record TopicScore(string Topic, int Score);

/// <summary>Weekly league table row. Change = places gained (+) or lost (−) since yesterday.</summary>
public record LeagueRow(int Rank, string Name, string Group, int WeekXp, int Change, bool IsMe = false)
{
    public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0]));
}

// ---- Study modules: flashcards, calendar, quests, shop ----
public record Deck(int Id, string Title, string Course, int Total, int Learned, int Due, string Icon);
public record Flashcard(string Front, string Back, string Topic, string? Code = null);
/// <summary>Kind: assignment | test | lesson | consult. Day = day of the current month.</summary>
public record CalendarEvent(int Day, string Time, string Kind, string Title, string Course, string? Href = null);
public record Quest(string Icon, string Title, string Text, int Done, int Goal, int Xp);
/// <summary>Kind: boost | owl | theme. State: owned | equipped | buy.</summary>
public record ShopItem(string Icon, string Kind, string Title, string Text, int Price, string State, string Tone);

// ---- Teacher: grading queue ----
/// <summary>Kind: lab | course | test. Status: new | review | returned | graded.</summary>
public record Submission(int Id, string Student, string Initials, string Group, string Title, string Course, string Kind,
    string SubmittedAgo, int AiMatch, string Status, bool Late, int MaxScore = 100);
public record SubmissionDetail(Submission Info, string FileName, string Language, string Code, string AiSummary,
    int Plagiarism, string[] Files, string? StudentComment);

// ---- Chart payload consumed by wwwroot/js/edu.js ----
public record ChartSeries(string Label, int[] Data, string Color = "primary");
public record ChartSpec(string Type, string[] Labels, ChartSeries[] Series, bool Stacked = false, bool Horizontal = false, string? Suffix = null);

/// <summary>
/// Empty / no-results block (Views/Shared/_EmptyState.cshtml).
/// For = "#listId" → hidden until a search/filter hides every item of that list (edu.js filterItems).
/// </summary>
public record EmptyState(string Icon, string Title, string Text, string? For = null,
    string? ActionText = null, string? ActionHref = null, string Mood = "is-thinking");
