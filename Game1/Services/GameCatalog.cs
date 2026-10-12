namespace Game1.Services;

/// <summary>
/// One game in the "Game zone".
/// Category drives the accent color: code | data | web | algo | sec | quiz.
/// Playable = false → shown as a "coming soon" concept card.
/// </summary>
public record GameInfo(
    string Id, string Title, string Tagline, string Category, string Icon,
    int Minutes, int Difficulty, string Topic, string Skill, int MaxXp, bool Playable = true, string Mode = "solo");

/// <summary>Catalog of games shown in the hub. Game content lives in wwwroot/js/games/*.js and changes every round.</summary>
public class GameCatalog(LangService l)
{
    private string T(string ru, string kk, string en) => l.T(ru, kk, en);

    public List<GameInfo> All() =>
    [
        new("kodl", T("Кодл", "Кодл", "Codle"),
            T("Угадай IT-слово за 6 попыток. Новое слово каждый день.", "IT-сөзді 6 әрекетте тап. Күн сайын жаңа сөз.", "Guess the IT word in 6 tries. A new word every day."),
            "quiz", "spell-check", 3, 1, T("Термины", "Терминдер", "Terms"), T("Словарь разработчика", "Әзірлеуші сөздігі", "Dev vocabulary"), 40),
        new("bughunt", T("Баг-хантер", "Баг-хантер", "Bug hunter"),
            T("Найди строку с ошибкой быстрее таймера. Комбо за серию.", "Қате жолды таймерден жылдам тап.", "Spot the buggy line before the timer runs out."),
            "code", "bug", 4, 2, "PHP · JS · Python · SQL", T("Код-ревью", "Код-ревью", "Code review"), 120),
        new("output", T("Что выведет?", "Не шығарады?", "What's the output?"),
            T("Прочитай код и предскажи результат. Разбор после каждого ответа.", "Кодты оқып, нәтижені болжа.", "Read the code and predict what it prints."),
            "code", "terminal", 4, 2, "JS · Python · PHP", T("Трассировка кода", "Кодты трассировкалау", "Code tracing"), 100),
        new("puzzle", T("Код-пазл", "Код-пазл", "Code puzzle"),
            T("Собери программу из перемешанных строк. Осторожно: есть лишние!", "Араласқан жолдардан бағдарлама құрастыр.", "Rebuild the program from shuffled lines. Watch out for decoys!"),
            "code", "puzzle", 5, 2, "PHP · JS · Python", T("Структура программ", "Бағдарлама құрылымы", "Program structure"), 90),
        new("sqldet", T("SQL-детектив", "SQL-детектив", "SQL detective"),
            T("В EDU-Сити украли Золотую сову. Найди вора настоящими SQL-запросами.", "EDU-Сити-де Алтын үкі ұрланды. Ұрыны SQL арқылы тап.", "The Golden Owl was stolen in EDU City. Catch the thief with real SQL."),
            "data", "search", 12, 3, "SQL: SELECT · JOIN · WHERE", T("Реальный SQL", "Нақты SQL", "Real SQL"), 200),
        new("sort", T("Будь сортировкой", "Сұрыптау бол", "Be the sort"),
            T("Сортируй кристаллы шаг за шагом, как настоящий алгоритм.", "Кристалдарды алгоритм сияқты сұрыпта.", "Sort the crystals step by step, exactly like the algorithm."),
            "algo", "bar-chart-3", 4, 2, T("Сортировки", "Сұрыптаулар", "Sorting"), T("Алгоритмы", "Алгоритмдер", "Algorithms"), 90),
        new("robot", T("Робот Ух", "Робот Ух", "Robo-Hoot"),
            T("Запрограммируй робота-сову: шаги, повороты и функции.", "Робот-үкіні бағдарламала: қадамдар, бұрылыстар, функциялар.", "Program the robot owl with moves, turns and functions."),
            "algo", "bot", 6, 2, T("Циклы и функции", "Циклдер мен функциялар", "Loops & functions"), T("Алгоритмическое мышление", "Алгоритмдік ойлау", "Algorithmic thinking"), 120),
        new("regex", T("Регулярка-ниндзя", "Регекс-ниндзя", "Regex ninja"),
            T("Напиши регулярку, которая ловит всё слева и ничего справа.", "Сол жақтағының бәрін ұстайтын регекс жаз.", "Write a regex that matches everything left and nothing right."),
            "web", "regex", 5, 3, "RegExp", T("Регулярные выражения", "Тұрақты өрнектер", "Regular expressions"), 100),
        new("blitz", T("Блиц-викторина", "Блиц-викторина", "Blitz quiz"),
            T("60 секунд, вопросы по всем курсам. Быстрее ответ — больше очков.", "60 секунд, барлық курстар бойынша сұрақтар.", "60 seconds of questions across all courses. Faster is better."),
            "quiz", "zap", 1, 1, T("Все курсы", "Барлық курстар", "All courses"), T("Быстрые знания", "Жылдам білім", "Quick recall"), 60),

        // ---- Concepts (coming soon) ----
        new("fortress", T("Кибер-крепость", "Кибер-қамал", "Cyber fortress"),
            T("Tower defense: защищай сервер от SQL-инъекций и XSS правильным кодом.", "Серверді SQL-инъекция мен XSS-тен қорға.", "Tower defense: protect the server from SQLi and XSS with the right fix."),
            "sec", "castle", 8, 3, "OWASP Top 10", T("Безопасность", "Қауіпсіздік", "Security"), 150, Playable: false),
        new("garden", T("CSS-оранжерея", "CSS-жылыжай", "CSS greenhouse"),
            T("Расставь горшки Уха с помощью flexbox и grid.", "Ух құмыраларын flexbox пен grid арқылы орналастыр.", "Arrange Hoot's plants with flexbox and grid."),
            "web", "flower-2", 5, 1, "Flexbox · Grid", T("Вёрстка", "Беттеу", "Layout"), 90, Playable: false),
        new("duel", T("Дуэль 1×1", "Дуэль 1×1", "Duel 1×1"),
            T("Вызови одногруппника: одинаковые задачи, кто быстрее и точнее.", "Топтасыңды шақыр: кім жылдам әрі дәл.", "Challenge a classmate: same tasks, fastest and most accurate wins."),
            "quiz", "swords", 5, 2, T("Все курсы", "Барлық курстар", "All courses"), T("Соревнование", "Жарыс", "Competition"), 40, Playable: false, Mode: "duel"),
        new("senior", T("Кто хочет стать сеньором?", "Кім сеньор болғысы келеді?", "Who wants to be a senior?"),
            T("15 вопросов от джуна до сеньора с подсказками.", "Джуннан сеньорға дейін 15 сұрақ.", "15 questions from junior to senior with lifelines."),
            "quiz", "crown", 10, 3, T("Все курсы", "Барлық курстар", "All courses"), T("Глубокие знания", "Терең білім", "Deep knowledge"), 150, Playable: false),
        new("git", T("Git-ветки", "Git-тармақтар", "Git branches"),
            T("Командами git приведи дерево коммитов к цели.", "git командаларымен коммит ағашын мақсатқа жеткіз.", "Use git commands to shape the commit tree."),
            "sec", "git-branch", 6, 2, "Git", T("Контроль версий", "Нұсқаларды басқару", "Version control"), 100, Playable: false),
    ];

    public GameInfo? Find(string id) => All().FirstOrDefault(g => g.Id == id);

    /// <summary>Same "game of the day" for everyone, changes at midnight.</summary>
    public GameInfo GameOfDay()
    {
        var playable = All().Where(g => g.Playable && g.Id != "kodl").ToList();
        return playable[DateTime.Today.DayOfYear % playable.Count];
    }
}
