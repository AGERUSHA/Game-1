using Game1.Models;

namespace Game1.Services;

/// <summary>
/// Data contract used by all controllers.
/// Current implementation: <see cref="MockEduDataService"/> (static mock data).
/// Next stage: a SQL Server implementation registered in Program.cs instead of the mock.
/// </summary>
public interface IEduDataService
{
    // ---- Current user ----
    UserProfile GetStudent();
    UserProfile GetTeacher();
    List<Notification> GetNotifications();
    List<FeedItem> GetNotificationFeed();

    // ---- Student ----
    /// <summary>Published catalog, with the signed-in student's progress.</summary>
    List<Course> GetCourses();
    Course? GetCourse(int id);
    /// <summary>The student's most recently studied course (dashboard "Continue"), or null.</summary>
    Course? GetContinueCourse();
    LessonPage? GetLesson(int lessonId);
    MaterialPage? GetMaterial(int materialId);
    List<string> GetCategories();
    List<CourseModule> GetCurriculum(int courseId);
    /// <summary>Library of the courses the student is enrolled in.</summary>
    List<Material> GetMaterials();
    List<TestItem> GetTests();
    List<Question> GetQuestions(int testId);
    List<Assignment> GetAssignments();
    AssignmentDetail? GetAssignment(int id);
    List<Recommendation> GetRecommendations();
    List<Activity> GetRecentActivity();
    List<Achievement> GetAchievements();
    List<XpEvent> GetXpHistory();
    List<ChatMessage> GetChat();
    List<TopicScore> GetTopicScores();
    ChartSpec GetWeeklyActivity();
    ChartSpec GetXpByWeek();
    ChartSpec GetTestPerformance();
    int[] GetActivityHeatmap();
    List<LeagueRow> GetLeague();

    // ---- Study modules ----
    List<Deck> GetDecks();
    List<Flashcard> GetFlashcards(int deckId);
    List<CalendarEvent> GetCalendar();
    List<Quest> GetDailyQuests();
    List<ShopItem> GetShop();

    // ---- Teacher ----
    List<Submission> GetSubmissions();
    SubmissionDetail? GetSubmission(int id);
    List<Course> GetTeacherCourses();
    List<StudentRow> GetStudents();
    List<ReportItem> GetReports();
    ChartSpec GetStudentActivity();
    ChartSpec GetCourseCompletion();
    ChartSpec GetTeacherTestPerformance();
    ChartSpec GetScoreDistribution();
    ChartSpec GetXpEarned();

    // ---- Admin ----
    List<AdminUser> GetUsers();
    ChartSpec GetUserGrowth();
    ChartSpec GetRoleDistribution();
}
