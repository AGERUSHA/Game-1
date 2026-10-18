using Game1.Models;

namespace Game1.Data;

/// <summary>
/// Demo catalog written to an empty database on first start (Program.cs), so the site is not empty.
/// Teachers add their own courses on top; these can be edited or archived like any other course.
/// Text is in Russian — the platform's main language; authors write courses in one language.
/// </summary>
public static class CourseSeed
{
    public static void Run(AppDbContext db)
    {
        if (db.Courses.Any()) return;

        db.Courses.AddRange(
            Course("PHP & Laravel", "Backend Development", "Программирование", "Айгерим Нурланова", "violet", "server", "Средний", 32,
                "Создавайте современные веб-приложения на Laravel: маршрутизация, MVC, Eloquent ORM и REST API.",
                "Создавать веб-приложения на Laravel\nПроектировать архитектуру MVC\nРаботать с базой данных через Eloquent\nРазрабатывать REST API",
                Module("Модуль 1. Введение в Laravel",
                    Video("Установка и структура проекта", "12:40", "Устанавливаем PHP, Composer и создаём первый проект командой composer create-project laravel/laravel.\n\nРазбираем папки: app — код приложения, routes — маршруты, resources/views — шаблоны, database — миграции и сидеры."),
                    Video("Маршрутизация и контроллеры", "18:05", "Маршрут связывает URL с кодом: Route::get('/posts', [PostController::class, 'index']).\n\nКонтроллер группирует обработчики одной сущности. Команда php artisan make:controller PostController создаёт заготовку."),
                    Article("Шаблоны Blade", "10 мин", "Blade — шаблонизатор Laravel. {{ $title }} выводит значение с экранированием, @if и @foreach управляют разметкой.\n\nМакеты собираются через @extends и @section, повторяющиеся куски — через компоненты <x-alert />."),
                    Test("Тест: основы Laravel", "10 вопр.")),
                Module("Модуль 2. Архитектура MVC",
                    Video("Модели, представления, контроллеры", "21:30", "Модель хранит данные и правила, представление показывает их, контроллер принимает запрос и связывает одно с другим.\n\nТакое разделение позволяет менять дизайн, не трогая логику, и тестировать логику без браузера."),
                    Video("Валидация форм", "15:12", "$request->validate(['email' => 'required|email']) проверяет поля и при ошибке возвращает пользователя на форму с сообщениями.\n\nДля сложных правил создайте Form Request: php artisan make:request StorePostRequest."),
                    Assignment("Практика: CRUD-приложение", "1 ч", "Сделайте приложение «Заметки»: список, создание, редактирование и удаление. Используйте ресурсный контроллер и валидацию.")),
                Module("Модуль 3. Базы данных и Eloquent ORM",
                    Video("Миграции и сидеры", "16:48", "Миграция описывает изменение схемы базы кодом и хранится в git — вся команда получает одинаковую базу.\n\nСидеры заполняют таблицы тестовыми данными: php artisan db:seed."),
                    Video("Eloquent: модели и запросы", "22:10", "Post::where('published', true)->latest()->get() — запрос без SQL.\n\nМассовое заполнение защищается свойством $fillable."),
                    Video("Eloquent Relationships", "19:35", "hasMany, belongsTo, belongsToMany описывают связи между моделями.\n\nЖадная загрузка Post::with('author') решает проблему N+1 запросов."),
                    Article("SQL JOIN на практике", "15 мин", "INNER JOIN возвращает только совпадающие строки, LEFT JOIN — все строки левой таблицы.\n\nEloquent строит JOIN за вас, но понимание SQL помогает находить медленные запросы."),
                    Test("Тест: Eloquent ORM", "12 вопр.")),
                Module("Модуль 4. REST API и аутентификация",
                    Video("Проектирование REST API", "20:00", "Ресурсы называются существительными (/api/posts), действие задаёт метод HTTP: GET, POST, PUT, DELETE.\n\nОтвечайте правильными кодами: 201 при создании, 404 если не найдено, 422 при ошибке валидации."),
                    Video("Laravel Sanctum", "17:25", "Sanctum выдаёт API-токены и защищает SPA cookie-сессией.\n\nМаршруты под middleware('auth:sanctum') доступны только с токеном."),
                    Assignment("Итоговый проект", "3 ч", "Соберите REST API блога: статьи, комментарии, регистрация и вход через Sanctum. Приложите ссылку на репозиторий."))),

            Course("SQL и базы данных", "SQL Server · PostgreSQL", "Базы данных", "Данияр Ахметов", "indigo", "database", "Начальный", 24,
                "Проектирование баз данных, сложные запросы, JOIN, индексы и оптимизация производительности.",
                "Проектировать таблицы и связи\nПисать запросы с JOIN и GROUP BY\nУскорять запросы индексами",
                Module("Модуль 1. Основы SQL",
                    Video("Таблицы, строки, столбцы", "11:20", "Таблица — это набор строк одной структуры. Столбец имеет тип: INT, NVARCHAR, DATETIME.\n\nПервичный ключ однозначно определяет строку."),
                    Article("SELECT, WHERE, ORDER BY", "12 мин", "SELECT name, score FROM Students WHERE score > 80 ORDER BY score DESC.\n\nWHERE фильтрует строки до группировки, ORDER BY сортирует результат."),
                    Test("Тест: основы SQL", "8 вопр.")),
                Module("Модуль 2. Связи и JOIN",
                    Video("Внешние ключи", "14:05", "Внешний ключ ссылается на строку другой таблицы и не даёт удалить то, на что ещё ссылаются."),
                    Video("INNER, LEFT, RIGHT JOIN", "19:40", "INNER JOIN — только пары, LEFT JOIN — все строки слева, даже без пары (с NULL справа)."),
                    Article("GROUP BY и HAVING", "10 мин", "GROUP BY собирает строки в группы, агрегаты COUNT, AVG, SUM считают по группе. HAVING фильтрует уже группы."),
                    Assignment("Практика: отчёт по успеваемости", "1 ч", "Напишите запрос: средний балл каждой группы и количество студентов с баллом ниже 60."))),

            Course("Modern JavaScript", "ES2024 · TypeScript", "Программирование", "Тимур Касымов", "fuchsia", "braces", "Средний", 40,
                "Асинхронность, модули, работа с DOM и основы TypeScript на практических проектах.",
                "Писать асинхронный код с async/await\nРаботать с DOM и событиями\nТипизировать код на TypeScript",
                Module("Модуль 1. Современный синтаксис",
                    Video("let, const и стрелочные функции", "13:10", "const — для значений, которые не переназначаются, let — для остальных. var больше не нужен.\n\nСтрелочная функция не имеет своего this — удобно в обработчиках."),
                    Article("Деструктуризация и spread", "9 мин", "const { name, age } = user; const copy = { ...user, age: 21 }; — короче и понятнее ручного копирования."),
                    Test("Тест: синтаксис ES2024", "10 вопр.")),
                Module("Модуль 2. Асинхронность",
                    Video("Promise и async/await", "24:10", "await приостанавливает функцию до выполнения промиса. Ошибки ловим обычным try/catch.\n\nPromise.all запускает запросы параллельно."),
                    Assignment("Практика: загрузка данных из API", "1 ч", "Загрузите список пользователей с публичного API через fetch и покажите его в таблице с поиском."))),

            Course("Python для анализа данных", "Pandas · NumPy · Matplotlib", "Data Science", "Мадина Омарова", "blue", "bar-chart-3", "Начальный", 28,
                "Очистка, анализ и визуализация данных с помощью Python и Jupyter Notebook.",
                "Загружать и очищать данные в Pandas\nСтроить графики в Matplotlib\nДелать выводы по данным",
                Module("Модуль 1. Pandas",
                    Video("DataFrame и Series", "17:30", "DataFrame — таблица в памяти. pd.read_csv('data.csv') загружает файл, df.head() показывает первые строки."),
                    Article("Очистка данных", "12 мин", "df.dropna() удаляет пропуски, df.fillna(0) заполняет их. df.duplicated() находит повторы."),
                    Test("Тест: Pandas", "8 вопр.")),
                Module("Модуль 2. Визуализация",
                    Video("Графики в Matplotlib", "15:45", "plt.plot для линий, plt.bar для столбцов, plt.hist для распределений. Подписывайте оси!"),
                    Assignment("Практика: анализ продаж", "1.5 ч", "Проанализируйте файл продаж: выручка по месяцам, топ-5 товаров, график динамики."))),

            Course("UI/UX дизайн", "Figma · Design Systems", "Дизайн", "Алия Жумабаева", "plum", "pen-tool", "Начальный", 20,
                "Исследование пользователей, прототипирование, дизайн-системы и юзабилити-тестирование.",
                "Проводить интервью с пользователями\nСобирать прототипы в Figma\nСтроить дизайн-систему",
                Module("Модуль 1. Исследование",
                    Video("Кто наш пользователь", "14:00", "Интервью с 5 пользователями находит большинство проблем. Спрашивайте о прошлом опыте, а не о желаниях."),
                    Article("Персоны и сценарии", "8 мин", "Персона — собирательный портрет пользователя. Сценарий — шаги, которыми он достигает цели.")),
                Module("Модуль 2. Прототипы",
                    Video("Auto Layout в Figma", "18:20", "Auto Layout выравнивает элементы и сам растягивает блоки под текст — макет не ломается при правках."),
                    Assignment("Практика: прототип приложения", "2 ч", "Соберите кликабельный прототип из 5 экранов мобильного приложения для записи к врачу."))),

            Course("Алгоритмы и структуры данных", "C# · Big O", "Программирование", "Ерлан Сейткали", "deep", "git-branch", "Продвинутый", 36,
                "Сортировки, деревья, графы и динамическое программирование для собеседований и олимпиад.",
                "Оценивать сложность алгоритма\nВыбирать структуру данных под задачу\nРешать задачи на графы и ДП",
                Module("Модуль 1. Сложность",
                    Video("Big O нотация", "14:52", "O(n) — время растёт линейно с размером входа, O(n²) — квадратично. Константы отбрасываем."),
                    Article("Массивы и списки", "10 мин", "Доступ по индексу в массиве — O(1), вставка в середину — O(n). У связного списка наоборот."),
                    Test("Тест: сложность алгоритмов", "10 вопр.")),
                Module("Модуль 2. Сортировки",
                    Video("Быстрая сортировка", "21:15", "Выбираем опорный элемент, раскладываем меньшие влево, большие вправо и повторяем для частей. В среднем O(n log n)."),
                    Assignment("Практика: сравнение сортировок", "1 ч", "Реализуйте пузырьковую и быструю сортировку и сравните время на массивах из 1 000, 10 000 и 100 000 чисел."))),

            Course("Основы машинного обучения", "scikit-learn · Models", "Искусственный интеллект", "Мадина Омарова", "violet", "brain-circuit", "Средний", 34,
                "Регрессия, классификация, оценка моделей и первые нейронные сети.",
                "Обучать модели в scikit-learn\nОценивать качество модели\nПонимать, как работает нейросеть",
                Module("Модуль 1. Первые модели",
                    Video("Линейная регрессия", "16:40", "Модель подбирает прямую, которая ближе всего к точкам. Ошибку меряем средним квадратом отклонений."),
                    Article("Обучающая и тестовая выборки", "9 мин", "Модель проверяют на данных, которых она не видела: train_test_split делит данные на две части."),
                    Test("Тест: основы ML", "10 вопр."))),

            Course("English for IT", "B1 → B2", "Языки", "Sarah Collins", "indigo", "languages", "B1–B2", 18,
                "Технический английский: документация, code review, митинги и собеседования.",
                "Читать техническую документацию\nПисать понятные комментарии в code review\nПроходить собеседования на английском",
                Module("Module 1. Reading docs",
                    Video("How documentation is structured", "12:00", "Most docs follow the same pattern: overview, quick start, guides, API reference. Start with the quick start."),
                    Article("Useful phrases for code review", "8 мин", "\"Could we extract this into a helper?\" sounds friendlier than \"Move this\". Ask, don't order."),
                    Test("Quiz: IT vocabulary", "10 вопр.")))
        );
        db.SaveChanges();

        // Library items, attached to courses by title.
        var byTitle = db.Courses.ToDictionary(c => c.Title, c => c.Id);
        void Mat(string course, string title, string type, string meta, string body = "") =>
            db.Materials.Add(new MaterialEntity { CourseId = byTitle[course], Title = title, Type = type, Meta = meta, Body = body });
        Mat("PHP & Laravel", "Eloquent Relationships: полное руководство", "video", "19:35", "Разбор всех видов связей Eloquent с примерами: один к одному, один ко многим, многие ко многим и полиморфные связи.");
        Mat("SQL и базы данных", "Шпаргалка по SQL JOIN", "pdf", "2.4 MB");
        Mat("PHP & Laravel", "Архитектура MVC простыми словами", "article", "8 мин чтения", "Представьте ресторан. Официант (контроллер) принимает заказ, повар (модель) готовит блюдо по рецепту, а тарелка с подачей (представление) — то, что видит гость.\n\nЕсли поменять тарелку, рецепт не изменится. Так и в MVC: дизайн меняется без переписывания логики.");
        Mat("PHP & Laravel", "Введение в REST API", "presentation", "24 слайда");
        Mat("Modern JavaScript", "Async/Await и Promise", "video", "24:10", "Как устроен цикл событий и почему await не блокирует страницу.");
        Mat("Python для анализа данных", "Pandas: работа с DataFrame", "article", "12 мин чтения", "Выбор столбцов: df['price']. Фильтр строк: df[df.price > 1000]. Группировка: df.groupby('city').price.mean().\n\nЦепочки методов читаются сверху вниз — так анализ легко повторить.");
        Mat("SQL и базы данных", "Нормализация баз данных", "pdf", "1.8 MB");
        Mat("UI/UX дизайн", "Дизайн-системы в Figma", "presentation", "36 слайдов");
        Mat("Алгоритмы и структуры данных", "Big O нотация", "video", "14:52", "Как оценить, во сколько раз замедлится программа при росте данных.");
        db.SaveChanges();
    }

    private static CourseEntity Course(string title, string subtitle, string category, string teacher, string cover, string icon,
        string level, int hours, string description, string outcomes, params ModuleEntity[] modules)
    {
        for (var i = 0; i < modules.Length; i++) modules[i].Order = i;
        return new CourseEntity
        {
            Title = title, Subtitle = subtitle, Category = category, TeacherName = teacher, Cover = cover, Icon = icon,
            Level = level, Hours = hours, Description = description, Outcomes = outcomes, Modules = [.. modules],
        };
    }

    private static ModuleEntity Module(string title, params LessonEntity[] lessons)
    {
        for (var i = 0; i < lessons.Length; i++) lessons[i].Order = i;
        return new ModuleEntity { Title = title, Lessons = [.. lessons] };
    }

    private static LessonEntity Video(string title, string duration, string body) => new() { Title = title, Type = "video", Duration = duration, Body = body };
    private static LessonEntity Article(string title, string duration, string body) => new() { Title = title, Type = "article", Duration = duration, Body = body };
    private static LessonEntity Test(string title, string duration) => new()
    {
        Title = title, Type = "test", Duration = duration,
        Body = "Проверьте себя: ответьте на вопросы по урокам модуля. Тест засчитывается при результате от 70% — тогда урок отмечается пройденным.",
    };
    private static LessonEntity Assignment(string title, string duration, string body) => new() { Title = title, Type = "assignment", Duration = duration, Body = body };
}
