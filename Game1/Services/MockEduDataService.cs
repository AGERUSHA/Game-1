using Game1.Models;

namespace Game1.Services;

/// <summary>
/// Static mock data for the UI prototype (no database).
/// All user-facing strings are localized through <see cref="LangService.T"/>.
/// </summary>
public class MockEduDataService(LangService l) : IEduDataService
{
    protected string T(string ru, string kk, string en) => l.T(ru, kk, en);

    private string[] WeekDays => l.Current switch
    {
        "kk" => ["Дс", "Сс", "Ср", "Бс", "Жм", "Сб", "Жс"],
        "en" => ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        _ => ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"]
    };

    private string[] Months => l.Current switch
    {
        "kk" => ["Мам", "Мау", "Шіл", "Там", "Қыр", "Қаз"],
        "en" => ["May", "Jun", "Jul", "Aug", "Sep", "Oct"],
        _ => ["Май", "Июн", "Июл", "Авг", "Сен", "Окт"]
    };

    private string[] Weeks(int n) => Enumerable.Range(1, n).Select(i => T($"Нед {i}", $"{i}-апта", $"Wk {i}")).ToArray();

    // ------------------------------------------------------------------ user

    public virtual UserProfile GetStudent() => new(
        "Акерке", "Серикова", "akerke@edu.ai", "student", T("Студент", "Студент", "Student"),
        Xp: 1250, Level: 8, LevelStartXp: 1000, NextLevelXp: 1500,
        Courses: 12, AverageScore: 87, Streak: 7, CompletedCourses: 4, Certificates: 3);

    public virtual UserProfile GetTeacher() => new(
        "Айгерим", "Нурланова", "a.nurlanova@edu.ai", "teacher", T("Преподаватель", "Оқытушы", "Teacher"),
        Xp: 0, Level: 0, LevelStartXp: 0, NextLevelXp: 1,
        Courses: 6, AverageScore: 82, Streak: 0, CompletedCourses: 0, Certificates: 0);

    public List<Notification> GetNotifications() =>
    [
        new("sparkles", T("AI подготовил новые рекомендации", "AI жаңа ұсыныстар дайындады", "AI prepared new recommendations"), T("5 мин назад", "5 мин бұрын", "5 min ago"), true, "primary"),
        new("clipboard-check", T("Задание «REST API» проверено: 92/100", "«REST API» тапсырмасы тексерілді: 92/100", "Assignment “REST API” graded: 92/100"), T("1 час назад", "1 сағат бұрын", "1 hour ago"), true, "success"),
        new("trophy", T("Новое достижение: 7 дней подряд", "Жаңа жетістік: қатарынан 7 күн", "New achievement: 7-day streak"), T("Вчера", "Кеше", "Yesterday"), false, "warning"),
        new("calendar-clock", T("Дедлайн по «Eloquent ORM» через 2 дня", "«Eloquent ORM» мерзіміне 2 күн қалды", "“Eloquent ORM” is due in 2 days"), T("Вчера", "Кеше", "Yesterday"), false, "info"),
    ];

    public List<FeedItem> GetNotificationFeed() =>
    [
        new("grades", "clipboard-check", T("Оценка работы", "Жұмыс бағасы", "Grade"),
            T("Преподаватель проверил «REST API: CRUD» — 92/100", "Оқытушы «REST API: CRUD» тексерді — 92/100", "Your teacher graded “REST API: CRUD” — 92/100"),
            T("Александр Волков оставил рецензию и начислил XP.", "Александр Волков рецензия қалдырып, XP берді.", "Alexander Volkov left a review and awarded XP."),
            T("15 мин назад", "15 мин бұрын", "15 min ago"), true, "today", T("Смотреть", "Қарау", "View"), "/Student/Assignment/4", 100),
        new("deadlines", "alarm-clock", T("Горящий дедлайн", "Жанып тұрған мерзім", "Deadline soon"),
            T("До сдачи «Связи моделей в Eloquent» осталось 2 дня", "«Eloquent-тегі модель байланыстары» тапсыруға 2 күн қалды", "2 days left for “Eloquent model relationships”"),
            T("Загрузите файлы заранее — после дедлайна минус 10% за день.", "Файлдарды алдын ала жүктеңіз — мерзімнен кейін күніне 10% кем.", "Upload early — after the deadline it's −10% per day."),
            T("1 час назад", "1 сағат бұрын", "1 hour ago"), true, "today", T("Сдать", "Тапсыру", "Submit"), "/Student/Assignment/1"),
        new("ai", "sparkles", T("AI-наставник", "AI-тәлімгер", "AI mentor"),
            T("AI нашёл пробелы в теме SQL JOIN", "AI SQL JOIN тақырыбында олқылықтар тапты", "AI found gaps in SQL JOIN"),
            T("Собран набор из 5 карточек для повторения — займёт около 7 минут.", "Қайталауға 5 карточка жиналды — шамамен 7 минут.", "A set of 5 review cards is ready — about 7 minutes."),
            T("3 часа назад", "3 сағат бұрын", "3 hours ago"), true, "today", T("Тренироваться", "Жаттығу", "Practice"), "/Student/AiAnalysis"),
        new("achievements", "trophy", T("Достижение", "Жетістік", "Achievement"),
            T("Новый бейдж «Недельный спринт»", "Жаңа «Апталық спринт» белгісі", "New badge: “Weekly sprint”"),
            T("Вы занимались 7 дней подряд — серия защищена.", "Қатарынан 7 күн оқыдыңыз — серия қорғалды.", "You studied 7 days in a row — your streak is safe."),
            T("Вчера, 19:20", "Кеше, 19:20", "Yesterday, 19:20"), false, "yesterday", null, null, 50),
        new("grades", "message-square-text", T("Комментарий", "Пікір", "Comment"),
            T("Новый комментарий к «Прототипу мобильного приложения»", "«Мобильді қосымша прототипіне» жаңа пікір", "New comment on “Mobile app prototype”"),
            T("Айгерим Нурланова: «Отличная навигация, поправьте контраст кнопок».", "Айгерим Нурланова: «Навигация тамаша, түймелер контрастын түзетіңіз».", "Aigerim Nurlanova: “Great navigation, fix the button contrast.”"),
            T("Вчера, 11:05", "Кеше, 11:05", "Yesterday, 11:05"), false, "yesterday", T("Ответить", "Жауап беру", "Reply"), "/Student/Assignment/6"),
        new("deadlines", "calendar-check", T("Расписание", "Кесте", "Schedule"),
            T("Открыт тест «SQL: продвинутые запросы»", "«SQL: күрделі сұраулар» тесті ашылды", "Test “SQL: advanced queries” is open"),
            T("20 вопросов, 30 минут. Доступен до пятницы.", "20 сұрақ, 30 минут. Жұмаға дейін қолжетімді.", "20 questions, 30 minutes. Open until Friday."),
            T("3 дня назад", "3 күн бұрын", "3 days ago"), false, "earlier", T("К тесту", "Тестке", "Open test"), "/Student/Tests"),
        new("achievements", "zap", T("Уровень", "Деңгей", "Level"),
            T("Вы достигли 8 уровня!", "Сіз 8-деңгейге жеттіңіз!", "You reached level 8!"),
            T("Ух получил медаль. Следующий аксессуар — на 12 уровне.", "Ух медаль алды. Келесі аксессуар — 12-деңгейде.", "Hoot earned a medal. Next accessory unlocks at level 12."),
            T("5 дней назад", "5 күн бұрын", "5 days ago"), false, "earlier", T("Смотреть", "Қарау", "View"), "/Student/Xp"),
    ];

    // ------------------------------------------------------------------ courses

    public List<string> GetCategories() =>
    [
        T("Все", "Барлығы", "All"),
        T("Программирование", "Бағдарламалау", "Programming"),
        T("Базы данных", "Деректер қоры", "Databases"),
        "Data Science",
        T("Дизайн", "Дизайн", "Design"),
        T("Искусственный интеллект", "Жасанды интеллект", "AI"),
        T("Языки", "Тілдер", "Languages")
    ];

    public List<Course> GetCourses() =>
    [
        new(1, "PHP & Laravel", "Backend Development", T("Программирование", "Бағдарламалау", "Programming"),
            T("Создавайте современные веб-приложения на Laravel: маршрутизация, MVC, Eloquent ORM и REST API.",
              "Laravel-де заманауи веб-қосымшалар жасаңыз: маршруттау, MVC, Eloquent ORM және REST API.",
              "Build modern web applications with Laravel: routing, MVC, Eloquent ORM and REST APIs."),
            "Айгерим Нурланова", 24, 72, 4.9, 1240, "violet", "server", T("32 ч", "32 сағ", "32 h"), T("Средний", "Орташа", "Intermediate")),
        new(2, T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), "SQL Server · PostgreSQL", T("Базы данных", "Деректер қоры", "Databases"),
            T("Проектирование баз данных, сложные запросы, JOIN, индексы и оптимизация производительности.",
              "Деректер қорын жобалау, күрделі сұраулар, JOIN, индекстер және өнімділікті оңтайландыру.",
              "Database design, complex queries, JOINs, indexes and performance tuning."),
            "Данияр Ахметов", 18, 45, 4.8, 980, "indigo", "database", T("24 ч", "24 сағ", "24 h"), T("Начальный", "Бастапқы", "Beginner")),
        new(3, "Modern JavaScript", "ES2024 · TypeScript", T("Программирование", "Бағдарламалау", "Programming"),
            T("Асинхронность, модули, работа с DOM и основы TypeScript на практических проектах.",
              "Асинхрондылық, модульдер, DOM-мен жұмыс және TypeScript негіздері практикалық жобаларда.",
              "Async code, modules, the DOM and TypeScript basics through hands-on projects."),
            "Тимур Касымов", 30, 60, 4.7, 2310, "fuchsia", "braces", T("40 ч", "40 сағ", "40 h"), T("Средний", "Орташа", "Intermediate")),
        new(4, T("Python для анализа данных", "Деректерді талдауға арналған Python", "Python for Data Analysis"), "Pandas · NumPy · Matplotlib", "Data Science",
            T("Очистка, анализ и визуализация данных с помощью Python и Jupyter Notebook.",
              "Python және Jupyter Notebook көмегімен деректерді тазалау, талдау және визуализациялау.",
              "Clean, analyze and visualize data with Python and Jupyter Notebook."),
            "Мадина Омарова", 22, 30, 4.9, 1650, "blue", "bar-chart-3", T("28 ч", "28 сағ", "28 h"), T("Начальный", "Бастапқы", "Beginner")),
        new(5, T("UI/UX дизайн", "UI/UX дизайн", "UI/UX Design"), "Figma · Design Systems", T("Дизайн", "Дизайн", "Design"),
            T("Исследование пользователей, прототипирование, дизайн-системы и юзабилити-тестирование.",
              "Пайдаланушыларды зерттеу, прототиптеу, дизайн-жүйелер және юзабилити-тестілеу.",
              "User research, prototyping, design systems and usability testing."),
            "Алия Жумабаева", 16, 100, 4.8, 870, "plum", "pen-tool", T("20 ч", "20 сағ", "20 h"), T("Начальный", "Бастапқы", "Beginner")),
        new(6, T("Алгоритмы и структуры данных", "Алгоритмдер және деректер құрылымы", "Algorithms & Data Structures"), "C# · Big O", T("Программирование", "Бағдарламалау", "Programming"),
            T("Сортировки, деревья, графы и динамическое программирование для собеседований и олимпиад.",
              "Сұрыптау, ағаштар, графтар және динамикалық бағдарламалау — сұхбаттар мен олимпиадаларға.",
              "Sorting, trees, graphs and dynamic programming for interviews and contests."),
            "Ерлан Сейткали", 28, 15, 4.6, 640, "deep", "git-branch", T("36 ч", "36 сағ", "36 h"), T("Продвинутый", "Жоғары", "Advanced")),
        new(7, T("Основы машинного обучения", "Машиналық оқыту негіздері", "Machine Learning Basics"), "scikit-learn · Models", T("Искусственный интеллект", "Жасанды интеллект", "AI"),
            T("Регрессия, классификация, оценка моделей и первые нейронные сети.",
              "Регрессия, классификация, модельдерді бағалау және алғашқы нейрондық желілер.",
              "Regression, classification, model evaluation and your first neural networks."),
            "Мадина Омарова", 26, 0, 4.9, 1120, "violet", "brain-circuit", T("34 ч", "34 сағ", "34 h"), T("Средний", "Орташа", "Intermediate")),
        new(8, "English for IT", "B1 → B2", T("Языки", "Тілдер", "Languages"),
            T("Технический английский: документация, code review, митинги и собеседования.",
              "Техникалық ағылшын: құжаттама, code review, кездесулер және сұхбаттар.",
              "Technical English: documentation, code reviews, meetings and interviews."),
            "Sarah Collins", 20, 88, 4.7, 1890, "indigo", "languages", T("18 ч", "18 сағ", "18 h"), "B1–B2"),
    ];

    public Course? GetCourse(int id) => GetCourses().FirstOrDefault(c => c.Id == id);

    public List<CourseModule> GetCurriculum(int courseId) =>
    [
        new(T("Модуль 1. Введение в Laravel", "1-модуль. Laravel-ге кіріспе", "Module 1. Introduction to Laravel"),
        [
            new(T("Установка и структура проекта", "Орнату және жоба құрылымы", "Installation & project structure"), "video", "12:40", "completed"),
            new(T("Маршрутизация и контроллеры", "Маршруттау және контроллерлер", "Routing & controllers"), "video", "18:05", "completed"),
            new(T("Шаблоны Blade", "Blade шаблондары", "Blade templates"), "article", "10 min", "completed"),
            new(T("Тест: основы Laravel", "Тест: Laravel негіздері", "Quiz: Laravel basics"), "test", "10 q", "completed"),
        ]),
        new(T("Модуль 2. Архитектура MVC", "2-модуль. MVC архитектурасы", "Module 2. MVC architecture"),
        [
            new(T("Модели, представления, контроллеры", "Модельдер, көріністер, контроллерлер", "Models, views, controllers"), "video", "21:30", "completed"),
            new(T("Валидация форм", "Пішіндерді валидациялау", "Form validation"), "video", "15:12", "completed"),
            new(T("Практика: CRUD-приложение", "Практика: CRUD-қосымша", "Practice: CRUD app"), "assignment", "1 h", "completed"),
        ]),
        new(T("Модуль 3. Базы данных и Eloquent ORM", "3-модуль. Деректер қоры және Eloquent ORM", "Module 3. Databases & Eloquent ORM"),
        [
            new(T("Миграции и сидеры", "Миграциялар және сидерлер", "Migrations & seeders"), "video", "16:48", "completed"),
            new(T("Eloquent: модели и запросы", "Eloquent: модельдер мен сұраулар", "Eloquent: models & queries"), "video", "22:10", "completed"),
            new(T("Eloquent Relationships", "Eloquent Relationships", "Eloquent Relationships"), "video", "19:35", "current"),
            new(T("SQL JOIN на практике", "SQL JOIN практикада", "SQL JOIN in practice"), "article", "15 min", "locked"),
            new(T("Тест: Eloquent ORM", "Тест: Eloquent ORM", "Quiz: Eloquent ORM"), "test", "12 q", "locked"),
        ]),
        new(T("Модуль 4. REST API и аутентификация", "4-модуль. REST API және аутентификация", "Module 4. REST API & authentication"),
        [
            new(T("Проектирование REST API", "REST API жобалау", "Designing a REST API"), "video", "20:00", "locked"),
            new("Laravel Sanctum", "video", "17:25", "locked"),
            new(T("Итоговый проект", "Қорытынды жоба", "Final project"), "assignment", "3 h", "locked"),
        ]),
    ];

    // ------------------------------------------------------------------ materials / tests / assignments

    public List<Material> GetMaterials() =>
    [
        new(T("Eloquent Relationships: полное руководство", "Eloquent Relationships: толық нұсқаулық", "Eloquent Relationships: complete guide"), "video", "PHP & Laravel", "19:35", T("2 дня назад", "2 күн бұрын", "2 days ago")),
        new(T("Шпаргалка по SQL JOIN", "SQL JOIN бойынша анықтамалық", "SQL JOIN cheat sheet"), "pdf", T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), "2.4 MB", T("3 дня назад", "3 күн бұрын", "3 days ago")),
        new(T("Архитектура MVC простыми словами", "MVC архитектурасы қарапайым тілмен", "MVC architecture explained simply"), "article", "PHP & Laravel", T("8 мин чтения", "8 мин оқу", "8 min read"), T("Неделю назад", "Апта бұрын", "1 week ago")),
        new(T("Введение в REST API", "REST API-ге кіріспе", "Introduction to REST APIs"), "presentation", "PHP & Laravel", T("24 слайда", "24 слайд", "24 slides"), T("Неделю назад", "Апта бұрын", "1 week ago")),
        new(T("Async/Await и Promise", "Async/Await және Promise", "Async/Await & Promises"), "video", "Modern JavaScript", "24:10", T("2 недели назад", "2 апта бұрын", "2 weeks ago")),
        new(T("Pandas: работа с DataFrame", "Pandas: DataFrame-мен жұмыс", "Pandas: working with DataFrames"), "article", T("Python для анализа данных", "Деректерді талдауға арналған Python", "Python for Data Analysis"), T("12 мин чтения", "12 мин оқу", "12 min read"), T("2 недели назад", "2 апта бұрын", "2 weeks ago")),
        new(T("Нормализация баз данных", "Деректер қорын қалыпқа келтіру", "Database normalization"), "pdf", T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), "1.8 MB", T("3 недели назад", "3 апта бұрын", "3 weeks ago")),
        new(T("Дизайн-системы в Figma", "Figma-дағы дизайн-жүйелер", "Design systems in Figma"), "presentation", T("UI/UX дизайн", "UI/UX дизайн", "UI/UX Design"), T("36 слайдов", "36 слайд", "36 slides"), T("Месяц назад", "Ай бұрын", "1 month ago")),
        new(T("Big O нотация", "Big O нотациясы", "Big O notation"), "video", T("Алгоритмы и структуры данных", "Алгоритмдер және деректер құрылымы", "Algorithms & Data Structures"), "14:52", T("Месяц назад", "Ай бұрын", "1 month ago")),
    ];

    public List<TestItem> GetTests() =>
    [
        new(1, T("Архитектура MVC", "MVC архитектурасы", "MVC architecture"), "PHP & Laravel", 10, 15, null, "available", 0, T("Средний", "Орташа", "Medium")),
        new(2, "SQL JOIN", T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), 12, 20, 58, "retake", 2, T("Средний", "Орташа", "Medium")),
        new(3, T("Основы Laravel", "Laravel негіздері", "Laravel basics"), "PHP & Laravel", 10, 15, 94, "passed", 1, T("Лёгкий", "Жеңіл", "Easy")),
        new(4, "Eloquent ORM", "PHP & Laravel", 12, 20, null, "locked", 0, T("Сложный", "Күрделі", "Hard")),
        new(5, T("Асинхронный JavaScript", "Асинхронды JavaScript", "Async JavaScript"), "Modern JavaScript", 15, 25, 81, "passed", 1, T("Средний", "Орташа", "Medium")),
        new(6, T("Основы Pandas", "Pandas негіздері", "Pandas basics"), T("Python для анализа данных", "Деректерді талдауға арналған Python", "Python for Data Analysis"), 10, 15, null, "available", 0, T("Лёгкий", "Жеңіл", "Easy")),
    ];

    public List<Question> GetQuestions(int testId) =>
    [
        new(T("Что означает буква «C» в аббревиатуре MVC?", "MVC қысқартпасындағы «C» әрпі нені білдіреді?", "What does the letter “C” stand for in MVC?"),
            ["Component", "Controller", "Container", "Collection"], 1, "MVC"),
        new(T("Какой слой отвечает за бизнес-логику и работу с данными?", "Бизнес-логика мен деректермен жұмысқа қай қабат жауап береді?", "Which layer is responsible for business logic and data?"),
            ["View", "Model", "Router", "Middleware"], 1, "MVC"),
        new(T("Где в Laravel по умолчанию хранятся шаблоны представлений?", "Laravel-де көрініс шаблондары әдепкі бойынша қайда сақталады?", "Where are view templates stored in Laravel by default?"),
            ["app/Views", "resources/views", "public/views", "routes/views"], 1, "Laravel"),
        new(T("Какой метод Eloquent описывает связь «один ко многим»?", "Eloquent-тің қай әдісі «біреуі көпке» байланысын сипаттайды?", "Which Eloquent method defines a one-to-many relationship?"),
            ["belongsTo()", "hasOne()", "hasMany()", "morphTo()"], 2, "Eloquent",
            "public function posts()\n{\n    return $this->______(Post::class);\n}"),
        new(T("Что вернёт INNER JOIN, если в правой таблице нет совпадений?", "Оң жақ кестеде сәйкестік болмаса, INNER JOIN не қайтарады?", "What does INNER JOIN return when the right table has no match?"),
            [T("Строку с NULL", "NULL бар жол", "A row with NULLs"), T("Строка не попадёт в результат", "Жол нәтижеге кірмейді", "The row is excluded"), T("Ошибку", "Қате", "An error"), T("Все строки правой таблицы", "Оң кестенің барлық жолы", "All rows from the right table")], 1, "SQL JOIN"),
        new(T("Какой HTTP-метод используется для обновления ресурса в REST?", "REST-те ресурсты жаңарту үшін қай HTTP-әдіс қолданылады?", "Which HTTP method updates a resource in REST?"),
            ["GET", "POST", "PUT", "DELETE"], 2, "REST"),
        new(T("Для чего нужен Middleware?", "Middleware не үшін керек?", "What is middleware used for?"),
            [T("Для стилизации страниц", "Беттерді безендіру үшін", "Styling pages"), T("Для фильтрации HTTP-запросов", "HTTP-сұрауларды сүзу үшін", "Filtering HTTP requests"), T("Для миграций", "Миграциялар үшін", "Running migrations"), T("Для тестов", "Тесттер үшін", "Testing")], 1, "Laravel"),
        new(T("Какая команда создаёт новый контроллер?", "Жаңа контроллерді қай команда жасайды?", "Which command creates a new controller?"),
            ["php artisan make:controller", "php artisan new:controller", "composer controller", "laravel make controller"], 0, "Laravel"),
        new(T("Что делает метод with() в Eloquent?", "Eloquent-тегі with() әдісі не істейді?", "What does with() do in Eloquent?"),
            [T("Жадная загрузка связей", "Байланыстарды алдын ала жүктеу", "Eager-loads relationships"), T("Удаляет записи", "Жазбаларды жояды", "Deletes records"), T("Создаёт транзакцию", "Транзакция жасайды", "Starts a transaction"), T("Сортирует результат", "Нәтижені сұрыптайды", "Sorts results")], 0, "Eloquent"),
        new(T("Какой файл описывает веб-маршруты?", "Веб-маршруттар қай файлда сипатталады?", "Which file defines web routes?"),
            ["routes/web.php", "app/routes.php", "config/routes.php", "public/index.php"], 0, "Laravel"),
    ];

    public List<Assignment> GetAssignments() =>
    [
        new(T("Связи моделей в Eloquent", "Eloquent-тегі модель байланыстары", "Eloquent model relationships"), "PHP & Laravel", T("8 окт, 23:59", "8 қаз, 23:59", "Oct 8, 23:59"), "in-progress", null, 100, 2, Id: 1),
        new(T("Проектирование схемы БД", "ДҚ сұлбасын жобалау", "Database schema design"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), T("10 окт, 18:00", "10 қаз, 18:00", "Oct 10, 18:00"), "todo", null, 100, 4, Id: 2),
        new(T("Todo-приложение на Fetch API", "Fetch API-дегі Todo-қосымша", "Todo app with Fetch API"), "Modern JavaScript", T("14 окт, 23:59", "14 қаз, 23:59", "Oct 14, 23:59"), "todo", null, 50, 8, Id: 3),
        new("REST API: CRUD", "PHP & Laravel", T("1 окт, 23:59", "1 қаз, 23:59", "Oct 1, 23:59"), "graded", 92, 100, -4, Id: 4),
        new(T("Анализ датасета продаж", "Сату датасетін талдау", "Sales dataset analysis"), T("Python для анализа данных", "Деректерді талдауға арналған Python", "Python for Data Analysis"), T("3 окт, 12:00", "3 қаз, 12:00", "Oct 3, 12:00"), "submitted", null, 100, -2, Id: 5),
        new(T("Прототип мобильного приложения", "Мобильді қосымша прототипі", "Mobile app prototype"), T("UI/UX дизайн", "UI/UX дизайн", "UI/UX Design"), T("25 сен, 23:59", "25 қыр, 23:59", "Sep 25, 23:59"), "graded", 48, 50, -10, Id: 6),
        new(T("Эссе: мой путь в IT", "Эссе: IT-дегі жолым", "Essay: my path into IT"), "English for IT", T("28 сен, 23:59", "28 қыр, 23:59", "Sep 28, 23:59"), "overdue", null, 20, -7, Id: 7),
    ];


    public AssignmentDetail? GetAssignment(int id)
    {
        var a = GetAssignments().FirstOrDefault(x => x.Id == id);
        if (a is null) return null;
        var graded = a.Status == "graded";
        var sent = a.Status is "submitted" or "graded";
        return id switch
        {
            1 => new(a, T("Модуль 3: Eloquent ORM и связи", "3-модуль: Eloquent ORM және байланыстар", "Module 3: Eloquent ORM & relations"),
                T("Реализуйте связи «один ко многим» и «многие ко многим» для постов, комментариев и тегов. Устраните проблему N+1 с помощью eager loading и приложите EXPLAIN-дамп запросов до и после.",
                  "Посттар, пікірлер және тегтер үшін «бірге-көп» және «көпке-көп» байланыстарын іске асырыңыз. N+1 мәселесін eager loading арқылы шешіп, сұраулардың EXPLAIN-дампын тіркеңіз.",
                  "Implement one-to-many and many-to-many relations for posts, comments and tags. Fix the N+1 problem with eager loading and attach an EXPLAIN dump before and after."),
                120, [".zip", ".php", ".sql"], 25, "Александр Волков", T("Куратор курса", "Курс кураторы", "Course curator"), null, null,
                T("Использовал with(['comments.author', 'tags']) — запросов стало 3 вместо 142.", "with(['comments.author', 'tags']) қолдандым — сұраулар 142 орнына 3 болды.", "Used with(['comments.author', 'tags']) — 3 queries instead of 142."),
                [new("eloquent_relations.zip", "14.2 MB", "file-archive"), new("explain_before_after.sql", "3.8 MB", "database")], null),
            4 => new(a, T("Модуль 2: REST API", "2-модуль: REST API", "Module 2: REST API"),
                T("Создайте CRUD API для ресурса «Курсы» с валидацией, пагинацией и ресурсными классами. Покройте эндпоинты тестами.",
                  "«Курстар» ресурсы үшін валидациясы, пагинациясы бар CRUD API жасаңыз. Эндпоинттерді тесттермен жабыңыз.",
                  "Build a CRUD API for the Courses resource with validation, pagination and API resources. Cover the endpoints with tests."),
                100, [".zip", ".php"], 25, "Александр Волков", T("Куратор курса", "Курс кураторы", "Course curator"),
                T("Отличная работа: валидация вынесена в FormRequest, ответы через API Resources, тесты покрывают все статусы. Время ответа эндпоинта сократилось с 840 мс до 62 мс.",
                  "Тамаша жұмыс: валидация FormRequest-ке шығарылған, жауаптар API Resources арқылы, тесттер барлық мәртебелерді қамтиды.",
                  "Great work: validation lives in FormRequests, responses use API Resources and tests cover every status code. Endpoint time dropped from 840 ms to 62 ms."),
                T("Для вложенных списков изучите withCount — так не придётся загружать сущности целиком.", "Кірістірілген тізімдер үшін withCount-ты қараңыз.", "For nested lists look at withCount so you don't load whole entities."),
                T("Добавил тесты на 422 и 404, пагинация по 15 записей.", "422 және 404 үшін тесттер қостым.", "Added tests for 422 and 404, pagination by 15."),
                [new("rest_api_crud.zip", "9.6 MB", "file-archive")], T("2 ч назад", "2 сағ бұрын", "2 h ago")),
            _ => new(a, T("Практическая работа", "Практикалық жұмыс", "Practical work"),
                T("Выполните задание по описанию в материалах курса и прикрепите результат. Перед отправкой проверьте, что все файлы открываются.",
                  "Тапсырманы курс материалдарындағы сипаттама бойынша орындап, нәтижені тіркеңіз.",
                  "Complete the task described in the course materials and attach your result. Check that all files open before submitting."),
                a.MaxScore + 20, [".zip", ".pdf", ".docx"], 25, "Айгерим Нурланова", T("Преподаватель", "Оқытушы", "Teacher"),
                graded ? T("Работа выполнена аккуратно, все требования соблюдены. Хорошая структура и понятные выводы.", "Жұмыс ұқыпты орындалған, барлық талаптар сақталған.", "Neat work that meets every requirement. Good structure and clear conclusions.") : null,
                graded ? T("В следующий раз добавьте краткое резюме в начале.", "Келесіде басында қысқаша түйін қосыңыз.", "Next time add a short summary at the top.") : null,
                sent ? T("Готово, жду проверки.", "Дайын, тексеруді күтемін.", "Done, waiting for review.") : null,
                sent ? [new("work.zip", "4.1 MB", "file-archive")] : [], graded ? T("вчера", "кеше", "yesterday") : null),
        };
    }

    // ------------------------------------------------------------------ grading queue

    public List<Submission> GetSubmissions() =>
    [
        new(1, "Акерке Нургали", "АН", "IT-2101", T("Лаб. №4: оптимизация Eloquent и N+1", "№4 зертх.: Eloquent және N+1 оңтайландыру", "Lab 4: Eloquent & N+1 optimization"), "PHP & Laravel", "lab", T("сегодня, 14:20", "бүгін, 14:20", "today, 14:20"), 94, "new", false),
        new(2, "Данияр Сериков", "ДС", "IT-2102", T("Курсовая: REST API и JWT", "Курстық: REST API және JWT", "Coursework: REST API & JWT"), "PHP & Laravel", "course", T("вчера, 19:10", "кеше, 19:10", "yesterday, 19:10"), 89, "new", false),
        new(3, "Алина Ким", "АК", "IT-2101", T("Лаб. №5: Docker и Nginx", "№5 зертх.: Docker және Nginx", "Lab 5: Docker & Nginx"), "DevOps", "lab", T("2 дня назад", "2 күн бұрын", "2 days ago"), 98, "new", false),
        new(4, "Бауыржан Тохтаров", "БТ", "IT-2101", T("Тест модуля 3: индексы", "3-модуль тесті: индекстер", "Module 3 test: indexes"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), "test", T("2 дня назад", "2 күн бұрын", "2 days ago"), 61, "review", true),
        new(5, "Нурай Абенова", "НА", "IT-2102", T("Лаб. №4: оптимизация Eloquent и N+1", "№4 зертх.: Eloquent және N+1 оңтайландыру", "Lab 4: Eloquent & N+1 optimization"), "PHP & Laravel", "lab", T("3 дня назад", "3 күн бұрын", "3 days ago"), 76, "returned", false),
        new(6, "Тимур Ибраев", "ТИ", "IT-2101", T("Лаб. №3: связи моделей", "№3 зертх.: модель байланыстары", "Lab 3: model relationships"), "PHP & Laravel", "lab", T("неделю назад", "бір апта бұрын", "a week ago"), 91, "graded", false),
    ];

    public SubmissionDetail? GetSubmission(int id)
    {
        var s = GetSubmissions().FirstOrDefault(x => x.Id == id);
        if (s is null) return null;
        const string code = """
            <?php

            namespace App\Services;

            class PostFeedService
            {
                public function feed(int $userId)
                {
                    // Before: 142 queries — Post::all() + lazy relations in the loop
                    // After: eager loading with only the columns we need
                    return Post::query()
                        ->select(['id', 'title', 'author_id', 'created_at'])
                        ->with(['author:id,name', 'comments' => fn ($q) => $q->latest()->limit(3)])
                        ->withCount('comments')
                        ->whereBelongsTo(User::find($userId), 'followers')
                        ->latest()
                        ->cursorPaginate(15);
                }
            }
            """;
        return new(s, "app/Services/PostFeedService.php", "PHP 8.3 · Laravel 11", code,
            T("Синтаксических ошибок нет. Количество SQL-запросов снизилось со 142 до 3 благодаря with() и withCount(). Пагинация курсором экономит память.",
              "Синтаксистік қате жоқ. with() және withCount() арқасында SQL-сұраулар 142-ден 3-ке азайды.",
              "No syntax errors. SQL queries dropped from 142 to 3 thanks to with() and withCount(). Cursor pagination saves memory."),
            2, ["eloquent_optimization.zip · 14.2 MB", "explain_before_after.sql · 3.8 MB"],
            T("Использовала eager loading и составной индекс — время ответа 840 → 62 мс.", "Eager loading және құрама индекс қолдандым — жауап уақыты 840 → 62 мс.", "Used eager loading and a composite index — response time 840 → 62 ms."));
    }

    // ------------------------------------------------------------------ study modules

    public List<Deck> GetDecks() =>
    [
        new(1, T("SQL: индексы и JOIN", "SQL: индекстер және JOIN", "SQL: indexes & JOINs"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), 32, 18, 5, "database"),
        new(2, T("Laravel: Eloquent", "Laravel: Eloquent", "Laravel: Eloquent"), "PHP & Laravel", 24, 20, 2, "layers"),
        new(3, T("HTTP и REST", "HTTP және REST", "HTTP & REST"), "PHP & Laravel", 18, 6, 9, "globe"),
        new(4, T("Алгоритмы: сложность", "Алгоритмдер: күрделілік", "Algorithms: complexity"), T("Алгоритмы", "Алгоритмдер", "Algorithms"), 20, 20, 0, "binary"),
    ];

    public List<Flashcard> GetFlashcards(int deckId) =>
    [
        new(T("Чем B-Tree индекс отличается от Hash индекса?", "B-Tree индексі Hash индексінен немен ерекшеленеді?", "How does a B-Tree index differ from a Hash index?"),
            T("B-Tree хранит ключи упорядоченно: подходит для диапазонов (<, >, BETWEEN) и сортировки. Hash — только для точного равенства (=), но очень быстрый.", "B-Tree кілттерді реттеп сақтайды: аралықтар мен сұрыптауға жарайды. Hash — тек дәл теңдікке (=).", "B-Tree keeps keys ordered — good for ranges (<, >, BETWEEN) and sorting. Hash only supports exact equality (=) but is very fast."),
            T("Индексы", "Индекстер", "Indexes")),
        new(T("Что вернёт LEFT JOIN, если справа нет совпадения?", "Оң жақта сәйкестік болмаса, LEFT JOIN не қайтарады?", "What does LEFT JOIN return when there is no match on the right?"),
            T("Строку из левой таблицы, а поля правой таблицы будут NULL.", "Сол кестеден жолды, ал оң кесте өрістері NULL болады.", "The left row, with the right table's columns set to NULL."),
            "JOIN", "SELECT u.name, o.id\nFROM users u\nLEFT JOIN orders o ON o.user_id = u.id;"),
        new(T("Что такое покрывающий индекс?", "Жабатын индекс дегеніміз не?", "What is a covering index?"),
            T("Индекс, который содержит все колонки запроса — СУБД отвечает, не читая саму таблицу.", "Сұраудың барлық бағандарын қамтитын индекс — ДҚБЖ кестені оқымай жауап береді.", "An index that contains every column the query needs, so the table itself is never read."),
            T("Индексы", "Индекстер", "Indexes")),
        new(T("Зачем нужен EXPLAIN?", "EXPLAIN не үшін керек?", "What is EXPLAIN for?"),
            T("Показывает план выполнения запроса: какие индексы используются, сколько строк читается, где полный перебор.", "Сұрауды орындау жоспарын көрсетеді: қандай индекстер, қанша жол оқылады.", "It shows the query plan: which indexes are used, how many rows are read, where full scans happen."),
            T("Оптимизация", "Оңтайландыру", "Optimization")),
        new(T("Чем WHERE отличается от HAVING?", "WHERE мен HAVING айырмашылығы неде?", "WHERE vs HAVING?"),
            T("WHERE фильтрует строки до группировки, HAVING — группы после GROUP BY.", "WHERE топтауға дейін жолдарды, HAVING — GROUP BY-дан кейін топтарды сүзеді.", "WHERE filters rows before grouping; HAVING filters groups after GROUP BY."),
            T("Агрегация", "Агрегация", "Aggregation")),
    ];

    public List<CalendarEvent> GetCalendar() =>
    [
        new(6, "18:00", "lesson", T("Лекция: индексы в PostgreSQL", "Дәріс: PostgreSQL индекстері", "Lecture: PostgreSQL indexes"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases")),
        new(8, "23:59", "assignment", T("Связи моделей в Eloquent", "Eloquent-тегі модель байланыстары", "Eloquent model relationships"), "PHP & Laravel", "/Student/Assignment/1"),
        new(10, "18:00", "assignment", T("Проектирование схемы БД", "ДҚ сұлбасын жобалау", "Database schema design"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), "/Student/Assignment/2"),
        new(13, "10:00", "test", T("Тест: архитектура MVC", "Тест: MVC архитектурасы", "Test: MVC architecture"), "PHP & Laravel", "/Student/TakeTest/1"),
        new(14, "23:59", "assignment", T("Todo на Fetch API", "Fetch API-дегі Todo", "Todo with Fetch API"), "Modern JavaScript", "/Student/Assignment/3"),
        new(15, "15:00", "consult", T("Консультация с А. Волковым", "А. Волковпен кеңес", "Consultation with A. Volkov"), "PHP & Laravel"),
        new(17, "12:00", "test", T("Тест: SQL JOIN", "Тест: SQL JOIN", "Test: SQL JOIN"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases"), "/Student/Tests"),
        new(20, "18:00", "lesson", T("Практика: REST API", "Практика: REST API", "Practice: REST API"), "PHP & Laravel"),
        new(22, "18:00", "lesson", T("Лекция: очереди и Redis", "Дәріс: кезектер және Redis", "Lecture: queues & Redis"), "PHP & Laravel"),
        new(24, "23:59", "assignment", T("Итоговый проект: API курсов", "Қорытынды жоба: курстар API", "Final project: courses API"), "PHP & Laravel"),
        new(28, "11:00", "test", T("Экзамен: базы данных", "Емтихан: деректер қоры", "Exam: databases"), T("SQL и базы данных", "SQL және деректер қоры", "SQL & Databases")),
    ];

    public List<Quest> GetDailyQuests() =>
    [
        new("clipboard-check", T("Пройди 1 тест", "1 тест тапсыр", "Pass 1 test"), T("Любой тест по вашим курсам", "Курстарыңыз бойынша кез келген тест", "Any test in your courses"), 1, 1, 30),
        new("layers", T("Повтори 10 карточек", "10 карточканы қайтала", "Review 10 flashcards"), T("Интервальное повторение", "Аралықты қайталау", "Spaced repetition"), 6, 10, 20),
        new("timer", T("Учись 25 минут в фокусе", "Фокуста 25 минут оқы", "Study 25 minutes in focus"), T("Одна полная сессия Pomodoro", "Бір толық Pomodoro сессиясы", "One full Pomodoro session"), 18, 25, 50),
    ];

    public List<ShopItem> GetShop() =>
    [
        new("snowflake", "boost", T("Заморозка серии", "Серияны мұздату", "Streak freeze"), T("Сохраняет серию, если пропустите день", "Күн өткізіп алсаңыз, серияны сақтайды", "Keeps your streak if you miss a day"), 200, "owned", "info"),
        new("rocket", "boost", T("Бустер ×2 XP", "×2 XP бустер", "×2 XP booster"), T("Двойной XP на 1 час", "1 сағатқа екі есе XP", "Double XP for 1 hour"), 300, "buy", "warning"),
        new("glasses", "owl", T("Очки отличника", "Үздік көзілдірігі", "Honor-roll glasses"), T("Аксессуар для Ух", "Ух үшін аксессуар", "An accessory for Hoot"), 150, "equipped", "primary"),
        new("crown", "owl", T("Золотая мантия", "Алтын мантия", "Golden robe"), T("Наряд магистра для Ух", "Ух үшін магистр киімі", "A master's robe for Hoot"), 500, "buy", "warning"),
        new("headphones", "owl", T("Наушники", "Құлаққап", "Headphones"), T("Ух слушает подкасты в фокусе", "Ух фокуста подкаст тыңдайды", "Hoot listens to podcasts while you focus"), 250, "owned", "primary"),
        new("moon-star", "theme", T("Тема «Ночное небо»", "«Түнгі аспан» тақырыбы", "“Night sky” theme"), T("Звёздный фон в тёмной теме", "Қараңғы тақырыпта жұлдызды фон", "A starry background in dark mode"), 350, "buy", "info"),
        new("palette", "theme", T("Тема «Аврора»", "«Аврора» тақырыбы", "“Aurora” theme"), T("Бирюзово-фиолетовые акценты", "Көкшіл-күлгін акценттер", "Teal and violet accents"), 1500, "buy", "success"),
    ];

    // ------------------------------------------------------------------ AI / activity

    public List<Recommendation> GetRecommendations() =>
    [
        new("database", T("Повторите SQL JOIN", "SQL JOIN тақырыбын қайталаңыз", "Review SQL JOIN"),
            T("В последнем тесте 3 из 5 ошибок связаны с JOIN.", "Соңғы тестте 5 қатенің 3-еуі JOIN-ге қатысты.", "3 of 5 mistakes in your last test were about JOINs."),
            T("Слабая тема", "Әлсіз тақырып", "Weak topic"), T("Повторить", "Қайталау", "Review"), "/Student/Materials"),
        new("book-open", T("Изучите Eloquent Relationships", "Eloquent Relationships оқып шығыңыз", "Study Eloquent Relationships"),
            T("Следующий урок курса — займёт около 20 минут.", "Курстың келесі сабағы — шамамен 20 минут.", "Your next lesson — takes about 20 minutes."),
            T("Следующий шаг", "Келесі қадам", "Next step"), T("Начать", "Бастау", "Start"), "/Student/CourseDetails/1"),
        new("clipboard-check", T("Пройдите тест по MVC", "MVC бойынша тест тапсырыңыз", "Take the MVC quiz"),
            T("Вы готовы: 92% материала модуля изучено.", "Сіз дайынсыз: модуль материалының 92%-ы оқылды.", "You're ready: 92% of the module is complete."),
            "+120 XP", T("Пройти", "Тапсыру", "Take quiz"), "/Student/TakeTest/1"),
    ];

    public List<Activity> GetRecentActivity() =>
    [
        new("check-circle-2", T("Завершён урок «Eloquent: модели и запросы»", "«Eloquent: модельдер мен сұраулар» сабағы аяқталды", "Completed lesson “Eloquent: models & queries”"), T("2 часа назад", "2 сағат бұрын", "2 hours ago"), 40, "success"),
        new("clipboard-check", T("Тест «Основы Laravel» — 94%", "«Laravel негіздері» тесті — 94%", "Quiz “Laravel basics” — 94%"), T("Вчера, 18:20", "Кеше, 18:20", "Yesterday, 18:20"), 120, "primary"),
        new("trophy", T("Получено достижение «7 дней подряд»", "«Қатарынан 7 күн» жетістігі алынды", "Unlocked “7-day streak” achievement"), T("Вчера, 09:05", "Кеше, 09:05", "Yesterday, 09:05"), 50, "warning"),
        new("upload", T("Отправлено задание «Анализ датасета продаж»", "«Сату датасетін талдау» тапсырмасы жіберілді", "Submitted “Sales dataset analysis”"), T("3 окт", "3 қаз", "Oct 3"), null, "info"),
        new("sparkles", T("AI-разбор теста «SQL JOIN»", "«SQL JOIN» тестіне AI-талдау", "AI review of “SQL JOIN” quiz"), T("2 окт", "2 қаз", "Oct 2"), null, "primary"),
    ];

    public List<ChatMessage> GetChat() =>
    [
        new("ai", T("Привет, Акерке! 👋 Я проанализировал ваши последние результаты. В тесте «SQL JOIN» вы набрали 58% — большинство ошибок связаны с LEFT JOIN и условием ON.",
                    "Сәлем, Ақерке! 👋 Соңғы нәтижелеріңізді талдадым. «SQL JOIN» тестінде 58% жинадыңыз — қателердің көбі LEFT JOIN мен ON шартына қатысты.",
                    "Hi Akerke! 👋 I analyzed your latest results. You scored 58% on the “SQL JOIN” quiz — most mistakes were about LEFT JOIN and the ON condition."), "10:24"),
        new("user", T("Объясни, чем LEFT JOIN отличается от INNER JOIN?", "LEFT JOIN-нің INNER JOIN-нен айырмашылығын түсіндірші?", "Can you explain how LEFT JOIN differs from INNER JOIN?"), "10:25"),
        new("ai", T("Конечно! Представьте две таблицы: студенты и курсы.\n• INNER JOIN вернёт только студентов, у которых есть курс.\n• LEFT JOIN вернёт всех студентов, а у тех, у кого курса нет, поля курса будут NULL.",
                    "Әрине! Екі кестені елестетіңіз: студенттер және курстар.\n• INNER JOIN тек курсы бар студенттерді қайтарады.\n• LEFT JOIN барлық студентті қайтарады, курсы жоқтарда курс өрістері NULL болады.",
                    "Sure! Imagine two tables: students and courses.\n• INNER JOIN returns only students who have a course.\n• LEFT JOIN returns every student; for those without a course, the course columns are NULL."), "10:25"),
    ];

    public List<TopicScore> GetTopicScores() =>
    [
        new("PHP Basics", 95), new(T("Функции", "Функциялар", "Functions"), 91), new("MVC", 84),
        new("Blade", 78), new("Eloquent", 54), new("SQL JOIN", 42),
    ];

    // ------------------------------------------------------------------ gamification

    public List<Achievement> GetAchievements() =>
    [
        new("🏆", T("Первый курс", "Алғашқы курс", "First Course"), T("Завершите свой первый курс", "Алғашқы курсыңызды аяқтаңыз", "Complete your first course"), true, 1, 1, T("12 сен", "12 қыр", "Sep 12"), "common"),
        new("🔥", T("7 дней подряд", "Қатарынан 7 күн", "7 Day Streak"), T("Занимайтесь 7 дней без перерыва", "7 күн үзіліссіз оқыңыз", "Study 7 days in a row"), true, 7, 7, T("4 окт", "4 қаз", "Oct 4"), "rare"),
        new("🧠", T("Отличник", "Үздік студент", "Excellent Student"), T("Средний балл выше 85%", "Орташа балл 85%-дан жоғары", "Average score above 85%"), true, 87, 85, T("1 окт", "1 қаз", "Oct 1"), "epic"),
        new("🚀", T("Быстрый старт", "Жылдам бастау", "Fast Start"), T("Пройдите 5 уроков в первый день", "Бірінші күні 5 сабақ өтіңіз", "Finish 5 lessons on day one"), true, 5, 5, T("2 сен", "2 қыр", "Sep 2"), "common"),
        new("🎯", T("Снайпер", "Мерген", "Sharpshooter"), T("Наберите 100% в любом тесте", "Кез келген тестте 100% жинаңыз", "Score 100% on any quiz"), false, 94, 100, null, "rare"),
        new("📚", T("Книжный червь", "Кітапқұмар", "Bookworm"), T("Изучите 50 материалов", "50 материалды оқыңыз", "Study 50 materials"), false, 34, 50, null, "common"),
        new("⚡", T("Марафонец", "Марафоншы", "Marathoner"), T("30 дней подряд", "Қатарынан 30 күн", "30-day streak"), false, 7, 30, null, "epic"),
        new("🤖", T("Друг AI", "AI досы", "AI Buddy"), T("Задайте AI 25 вопросов", "AI-ға 25 сұрақ қойыңыз", "Ask the AI 25 questions"), false, 18, 25, null, "common"),
        new("👑", T("Легенда", "Аңыз", "Legend"), T("Достигните 20 уровня", "20-деңгейге жетіңіз", "Reach level 20"), false, 8, 20, null, "legendary"),
        new("💎", T("Мастер курсов", "Курс шебері", "Course Master"), T("Завершите 10 курсов", "10 курсты аяқтаңыз", "Complete 10 courses"), false, 4, 10, null, "epic"),
        new("🌙", T("Ночная сова", "Түнгі үкі", "Night Owl"), T("Занимайтесь после 23:00", "23:00-ден кейін оқыңыз", "Study after 11 PM"), true, 1, 1, T("20 сен", "20 қыр", "Sep 20"), "common"),
        new("🤝", T("Командный игрок", "Командалық ойыншы", "Team Player"), T("Помогите 10 одногруппникам", "10 топтасқа көмектесіңіз", "Help 10 classmates"), false, 3, 10, null, "rare"),
    ];

    public List<XpEvent> GetXpHistory() =>
    [
        new(T("Урок «Eloquent: модели и запросы»", "«Eloquent: модельдер мен сұраулар» сабағы", "Lesson “Eloquent: models & queries”"), T("Урок", "Сабақ", "Lesson"), 40, T("Сегодня", "Бүгін", "Today"), "book-open"),
        new(T("Тест «Основы Laravel» — 94%", "«Laravel негіздері» тесті — 94%", "Quiz “Laravel basics” — 94%"), T("Тест", "Тест", "Quiz"), 120, T("Вчера", "Кеше", "Yesterday"), "clipboard-check"),
        new(T("Достижение «7 дней подряд»", "«Қатарынан 7 күн» жетістігі", "Achievement “7-day streak”"), T("Достижение", "Жетістік", "Achievement"), 50, T("Вчера", "Кеше", "Yesterday"), "trophy"),
        new(T("Задание «REST API: CRUD» — 92/100", "«REST API: CRUD» тапсырмасы — 92/100", "Assignment “REST API: CRUD” — 92/100"), T("Задание", "Тапсырма", "Assignment"), 150, T("1 окт", "1 қаз", "Oct 1"), "file-check-2"),
        new(T("Ежедневная серия", "Күнделікті серия", "Daily streak"), T("Бонус", "Бонус", "Bonus"), 20, T("30 сен", "30 қыр", "Sep 30"), "flame"),
        new(T("Тест «Асинхронный JavaScript» — 81%", "«Асинхронды JavaScript» тесті — 81%", "Quiz “Async JavaScript” — 81%"), T("Тест", "Тест", "Quiz"), 100, T("28 сен", "28 қыр", "Sep 28"), "clipboard-check"),
    ];

    // ------------------------------------------------------------------ student charts

    public ChartSpec GetWeeklyActivity() => new("bar", WeekDays,
    [
        new(T("Минуты обучения", "Оқу минуттары", "Study minutes"), [45, 80, 62, 95, 70, 120, 55])
    ], Suffix: T(" мин", " мин", " min"));

    public ChartSpec GetXpByWeek() => new("line", Weeks(8),
    [
        new("XP", [80, 140, 120, 210, 180, 260, 230, 310])
    ]);

    public ChartSpec GetTestPerformance() => new("line", Months,
    [
        new(T("Мой балл", "Менің балым", "My score"), [72, 78, 75, 83, 86, 87]),
        new(T("Средний по группе", "Топ бойынша орташа", "Group average"), [70, 71, 73, 74, 76, 77], "teal")
    ], Suffix: "%");

    public int[] GetActivityHeatmap()
    {
        // 18 weeks × 7 days, deterministic pseudo-random levels 0..4
        var rnd = new Random(42);
        return Enumerable.Range(0, 18 * 7).Select(i => i > 121 ? 0 : Math.Min(4, Math.Max(0, rnd.Next(-1, 5) + (i > 90 ? 1 : 0)))).ToArray();
    }

    // Weekly XP league (top 3 move up, bottom 3 move down on Sunday night)
    public List<LeagueRow> GetLeague() =>
    [
        new(1, "Сабина Ким", "IT-2102", 640, 0),
        new(2, "Нурсултан Абенов", "IT-2101", 455, 1),
        new(3, "Жанель Оспанова", "IT-2103", 350, -1),
        new(4, "Акерке Серикова", "IT-2101", 310, 2, IsMe: true),
        new(5, "Аружан Есенова", "IT-2103", 290, -1),
        new(6, "Дана Каирова", "IT-2102", 245, 1),
        new(7, "Мирас Толеуов", "IT-2104", 210, -2),
        new(8, "Даурен Нуров", "IT-2103", 180, 0),
        new(9, "Ербол Жаксылыков", "IT-2101", 120, 1),
        new(10, "Камила Ахметова", "IT-2104", 95, -1),
        new(11, "Алихан Мусин", "IT-2101", 40, 0),
        new(12, "Тимур Ибраев", "IT-2102", 15, 0),
    ];

    // ------------------------------------------------------------------ teacher

    public List<Course> GetTeacherCourses() =>
    [
        GetCourses()[0] with { Completion = 68, Students = 124 },
        new(9, T("Laravel: продвинутый уровень", "Laravel: жоғары деңгей", "Advanced Laravel"), "Queues · Events · Testing", T("Программирование", "Бағдарламалау", "Programming"),
            T("Очереди, события, тестирование и деплой Laravel-приложений.", "Laravel-қосымшалардың кезектері, оқиғалары, тестілеуі және деплойы.", "Queues, events, testing and deploying Laravel apps."),
            "Айгерим Нурланова", 20, 0, 4.8, 86, "deep", "layers", T("26 ч", "26 сағ", "26 h"), T("Продвинутый", "Жоғары", "Advanced"), "published", 41),
        new(10, T("Веб-разработка для начинающих", "Бастаушыларға арналған веб-әзірлеу", "Web Development for Beginners"), "HTML · CSS · JS", T("Программирование", "Бағдарламалау", "Programming"),
            T("Первые шаги в веб-разработке: вёрстка, адаптивность и основы JavaScript.", "Веб-әзірлеудегі алғашқы қадамдар: беттеу, бейімделу және JavaScript негіздері.", "First steps in web development: layout, responsiveness and JavaScript basics."),
            "Айгерим Нурланова", 32, 0, 4.9, 212, "fuchsia", "code-2", T("30 ч", "30 сағ", "30 h"), T("Начальный", "Бастапқы", "Beginner"), "published", 79),
        new(11, "REST API Design", "OpenAPI · Best practices", T("Программирование", "Бағдарламалау", "Programming"),
            T("Проектирование понятных и масштабируемых API.", "Түсінікті әрі масштабталатын API жобалау.", "Designing clear and scalable APIs."),
            "Айгерим Нурланова", 14, 0, 0, 0, "blue", "plug", T("14 ч", "14 сағ", "14 h"), T("Средний", "Орташа", "Intermediate"), "draft", 0),
    ];

    public List<StudentRow> GetStudents() =>
    [
        new(1, "Акерке Серикова", "akerke@edu.ai", "IT-2101", 4, 72, 87, 1250, "active", T("Сейчас онлайн", "Қазір онлайн", "Online now")),
        new(2, "Нурсултан Абенов", "nursultan@edu.ai", "IT-2101", 3, 91, 94, 2140, "active", T("10 мин назад", "10 мин бұрын", "10 min ago")),
        new(3, "Дана Каирова", "dana.k@edu.ai", "IT-2102", 5, 64, 79, 980, "active", T("1 час назад", "1 сағат бұрын", "1 hour ago")),
        new(4, "Алихан Мусин", "alikhan@edu.ai", "IT-2101", 2, 38, 61, 420, "risk", T("5 дней назад", "5 күн бұрын", "5 days ago")),
        new(5, "Жанель Оспанова", "zhanel@edu.ai", "IT-2103", 4, 85, 90, 1780, "active", T("Вчера", "Кеше", "Yesterday")),
        new(6, "Тимур Ибраев", "timur.i@edu.ai", "IT-2102", 3, 22, 54, 260, "risk", T("2 недели назад", "2 апта бұрын", "2 weeks ago")),
        new(7, "Аружан Есенова", "aruzhan@edu.ai", "IT-2103", 4, 77, 83, 1320, "active", T("3 часа назад", "3 сағат бұрын", "3 hours ago")),
        new(8, "Ербол Жаксылыков", "erbol@edu.ai", "IT-2101", 3, 55, 72, 760, "inactive", T("9 дней назад", "9 күн бұрын", "9 days ago")),
        new(9, "Сабина Ким", "sabina.kim@edu.ai", "IT-2102", 5, 96, 97, 2560, "active", T("Сейчас онлайн", "Қазір онлайн", "Online now")),
        new(10, "Даурен Нуров", "dauren@edu.ai", "IT-2103", 2, 47, 68, 610, "active", T("Вчера", "Кеше", "Yesterday")),
    ];

    public List<ReportItem> GetReports() =>
    [
        new("progress", "trending-up", T("Прогресс курса", "Курс прогресі", "Course progress"),
            T("Прохождение уроков и модулей по каждому студенту.", "Әр студенттің сабақтар мен модульдерді өтуі.", "Lesson and module completion for each student."), T("Сегодня, 09:00", "Бүгін, 09:00", "Today, 09:00"), ["PDF", "XLSX"]),
        new("tests", "clipboard-check", T("Результаты тестов", "Тест нәтижелері", "Test results"),
            T("Баллы, попытки и сложные вопросы по всем тестам.", "Барлық тест бойынша балдар, әрекеттер және күрделі сұрақтар.", "Scores, attempts and hardest questions across all tests."), T("Вчера", "Кеше", "Yesterday"), ["PDF", "XLSX", "CSV"]),
        new("activity", "activity", T("Активность студентов", "Студенттер белсенділігі", "Student activity"),
            T("Время на платформе, входы и вовлечённость по дням.", "Платформадағы уақыт, кірулер және күндер бойынша белсенділік.", "Time on platform, sign-ins and engagement by day."), T("2 окт", "2 қаз", "Oct 2"), ["PDF", "CSV"]),
        new("xp", "zap", T("XP и достижения", "XP және жетістіктер", "XP & achievements"),
            T("Рейтинг студентов, начисление XP и полученные награды.", "Студенттер рейтингі, XP есептеу және алынған марапаттар.", "Student ranking, XP earned and unlocked badges."), T("1 окт", "1 қаз", "Oct 1"), ["PDF", "XLSX"]),
        new("summary", "file-bar-chart", T("Сводный отчёт преподавателя", "Оқытушының жиынтық есебі", "Teacher summary report"),
            T("Ключевые показатели по всем курсам за выбранный период.", "Таңдалған кезеңдегі барлық курс бойынша негізгі көрсеткіштер.", "Key metrics across all courses for the selected period."), T("30 сен", "30 қыр", "Sep 30"), ["PDF"]),
    ];

    public ChartSpec GetStudentActivity() => new("line", WeekDays,
    [
        new(T("Эта неделя", "Осы апта", "This week"), [142, 168, 155, 190, 176, 98, 84]),
        new(T("Прошлая неделя", "Өткен апта", "Last week"), [120, 135, 150, 148, 160, 90, 70], "teal")
    ]);

    public ChartSpec GetCourseCompletion() => new("doughnut",
        [T("Завершили", "Аяқтағандар", "Completed"), T("В процессе", "Үдерісте", "In progress"), T("Не начали", "Бастамағандар", "Not started")],
        [new(T("Студенты", "Студенттер", "Students"), [58, 31, 11])], Suffix: "%");

    public ChartSpec GetTeacherTestPerformance() => new("bar",
        ["MVC", "Laravel", "Eloquent", "SQL JOIN", "REST", "Blade"],
        [new(T("Средний балл", "Орташа балл", "Average score"), [84, 88, 67, 61, 79, 90])], Suffix: "%");

    public ChartSpec GetScoreDistribution() => new("bar",
        ["0–49", "50–59", "60–69", "70–79", "80–89", "90–100"],
        [new(T("Студенты", "Студенттер", "Students"), [6, 11, 24, 48, 63, 39])]);

    public ChartSpec GetXpEarned() => new("line", Weeks(8),
    [
        new("XP", [4200, 5100, 4800, 6300, 7100, 6900, 8200, 9100])
    ]);

    // ------------------------------------------------------------------ admin

    public virtual List<AdminUser> GetUsers() =>
    [
        new("Акерке Серикова", "akerke@edu.ai", "student", "active", "02.09.2026", T("Сейчас", "Қазір", "Now")),
        new("Айгерим Нурланова", "a.nurlanova@edu.ai", "teacher", "active", "15.01.2025", T("5 мин назад", "5 мин бұрын", "5 min ago")),
        new("Данияр Ахметов", "d.akhmetov@edu.ai", "teacher", "active", "20.03.2025", T("1 час назад", "1 сағат бұрын", "1 hour ago")),
        new("Нурсултан Абенов", "nursultan@edu.ai", "student", "active", "02.09.2026", T("10 мин назад", "10 мин бұрын", "10 min ago")),
        new("Ольга Петрова", "admin@edu.ai", "admin", "active", "01.12.2024", T("Сейчас", "Қазір", "Now")),
        new("Тимур Ибраев", "timur.i@edu.ai", "student", "blocked", "05.09.2026", T("2 недели назад", "2 апта бұрын", "2 weeks ago")),
        new("Мадина Омарова", "m.omarova@edu.ai", "teacher", "pending", "01.10.2026", T("Вчера", "Кеше", "Yesterday")),
        new("Сабина Ким", "sabina.kim@edu.ai", "student", "active", "03.09.2026", T("Сейчас", "Қазір", "Now")),
    ];

    public ChartSpec GetUserGrowth() => new("bar", Months,
    [
        new(T("Студенты", "Студенттер", "Students"), [820, 940, 1010, 1260, 1840, 2130]),
        new(T("Преподаватели", "Оқытушылар", "Teachers"), [42, 45, 51, 58, 71, 78], "teal")
    ]);

    public virtual ChartSpec GetRoleDistribution() => new("doughnut",
        [T("Студенты", "Студенттер", "Students"), T("Преподаватели", "Оқытушылар", "Teachers"), T("Администраторы", "Әкімшілер", "Admins")],
        [new(T("Пользователи", "Пайдаланушылар", "Users"), [2130, 78, 6])]);
}
