using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Data;

/// <summary>
/// Demo quizzes and assignments for the seeded courses: every "test" lesson gets a quiz,
/// every "assignment" lesson gets an assignment (deadline counted from the student's enrolment).
/// Runs once, when the Tests table is empty.
/// </summary>
public static class TestSeed
{
    private record Q(string Text, string Topic, int Correct, string Explanation, string[] Options, string? Code = null);

    private static readonly Dictionary<string, (string Difficulty, int Minutes, Q[] Questions)> Quizzes = new()
    {
        ["Тест: основы Laravel"] = ("easy", 10,
        [
            new("Что означает буква «C» в аббревиатуре MVC?", "MVC", 1, "MVC = Model–View–Controller. Контроллер принимает запрос и связывает модель с представлением.", ["Component", "Controller", "Container", "Collection"]),
            new("Какой слой отвечает за бизнес-логику и работу с данными?", "MVC", 1, "Модель хранит данные и правила работы с ними; представление только показывает.", ["View", "Model", "Router", "Middleware"]),
            new("Где в Laravel по умолчанию хранятся шаблоны представлений?", "Laravel", 1, "Blade-шаблоны лежат в resources/views, например resources/views/posts/index.blade.php.", ["app/Views", "resources/views", "public/views", "routes/views"]),
            new("Какая команда создаёт новый контроллер?", "Laravel", 0, "Artisan — консоль Laravel; make:controller создаёт заготовку в app/Http/Controllers.", ["php artisan make:controller", "php artisan new:controller", "composer controller", "laravel make controller"]),
            new("Какой файл описывает веб-маршруты?", "Laravel", 0, "routes/web.php — маршруты для браузера (с сессиями и CSRF), routes/api.php — для API.", ["routes/web.php", "app/routes.php", "config/routes.php", "public/index.php"]),
            new("Для чего нужен Middleware?", "Laravel", 1, "Middleware стоит между запросом и контроллером: проверяет вход, CSRF, роли и т. д.", ["Для стилизации страниц", "Для фильтрации HTTP-запросов", "Для миграций", "Для тестов"]),
        ]),
        ["Тест: Eloquent ORM"] = ("hard", 15,
        [
            new("Какой метод Eloquent описывает связь «один ко многим»?", "Eloquent", 2, "hasMany() — у одной записи много связанных (у автора много постов). Обратная сторона — belongsTo().", ["belongsTo()", "hasOne()", "hasMany()", "morphTo()"],
                "public function posts()\n{\n    return $this->______(Post::class);\n}"),
            new("Что делает метод with() в Eloquent?", "Eloquent", 0, "with() загружает связи заранее одним запросом и убирает проблему N+1.", ["Жадная загрузка связей", "Удаляет записи", "Создаёт транзакцию", "Сортирует результат"]),
            new("Что вернёт INNER JOIN, если в правой таблице нет совпадений?", "SQL JOIN", 1, "INNER JOIN оставляет только пары. Чтобы сохранить строку без пары, нужен LEFT JOIN.", ["Строку с NULL", "Строка не попадёт в результат", "Ошибку", "Все строки правой таблицы"]),
            new("Зачем модели свойство $fillable?", "Eloquent", 2, "$fillable перечисляет поля, которые можно заполнять массово через create() — защита от подмены полей вроде is_admin.", ["Для сортировки", "Для связей", "Для защиты от массового присвоения", "Для кеша"]),
            new("Какая связь нужна для «студенты ↔ курсы»?", "Eloquent", 3, "Много студентов на много курсов — belongsToMany через промежуточную таблицу.", ["hasOne", "hasMany", "belongsTo", "belongsToMany"]),
        ]),
        ["Тест: основы SQL"] = ("easy", 10,
        [
            new("Какой оператор выбирает данные из таблицы?", "SELECT", 0, "SELECT читает строки; INSERT, UPDATE и DELETE их меняют.", ["SELECT", "INSERT", "UPDATE", "CREATE"]),
            new("Что делает WHERE?", "SELECT", 1, "WHERE отбирает строки по условию до группировки и сортировки.", ["Сортирует строки", "Фильтрует строки", "Группирует строки", "Создаёт индекс"]),
            new("Как отсортировать по убыванию?", "SELECT", 2, "ORDER BY столбец DESC — по убыванию, ASC (по умолчанию) — по возрастанию.", ["ORDER BY score UP", "SORT BY score", "ORDER BY score DESC", "GROUP BY score DESC"]),
            new("Что такое первичный ключ?", "Таблицы", 1, "Первичный ключ уникален и не бывает NULL — по нему находят ровно одну строку.", ["Самый большой столбец", "Столбец, однозначно определяющий строку", "Столбец с паролем", "Любой текстовый столбец"]),
            new("Какой тип подойдёт для даты и времени в SQL Server?", "Таблицы", 3, "DATETIME2 хранит дату и время с высокой точностью.", ["INT", "BIT", "NVARCHAR(10)", "DATETIME2"]),
        ]),
        ["Тест: синтаксис ES2024"] = ("medium", 10,
        [
            new("Чем const отличается от let?", "Переменные", 0, "const нельзя переназначить; но содержимое объекта в const менять можно.", ["const нельзя переназначить", "const работает быстрее", "let нельзя менять", "Ничем"]),
            new("Что выведет код?", "Деструктуризация", 2, "Деструктуризация берёт свойство name из объекта user.", ["undefined", "user", "Аня", "Ошибку"], "const user = { name: 'Аня', age: 20 };\nconst { name } = user;\nconsole.log(name);"),
            new("Что делает оператор ... в { ...user, age: 21 }?", "Spread", 1, "Spread копирует свойства в новый объект; age затем перезаписывается.", ["Удаляет user", "Копирует свойства user в новый объект", "Создаёт массив", "Ничего"]),
            new("Есть ли у стрелочной функции свой this?", "Функции", 1, "Стрелочная функция берёт this из окружающего кода — поэтому удобна в обработчиках.", ["Да, всегда", "Нет, она берёт this снаружи", "Только в классах", "Только в модулях"]),
            new("Что вернёт [1, 2, 3].map(x => x * 2)?", "Массивы", 3, "map создаёт новый массив, применяя функцию к каждому элементу.", ["6", "[1, 2, 3]", "[2, 3, 4]", "[2, 4, 6]"]),
        ]),
        ["Тест: Pandas"] = ("easy", 10,
        [
            new("Как загрузить CSV в Pandas?", "Pandas", 0, "pd.read_csv читает файл в DataFrame.", ["pd.read_csv('data.csv')", "pd.open('data.csv')", "pd.load('data.csv')", "csv.read('data.csv')"]),
            new("Что показывает df.head()?", "Pandas", 1, "head() возвращает первые 5 строк (или сколько укажете).", ["Названия столбцов", "Первые строки таблицы", "Последние строки", "Типы данных"]),
            new("Как удалить строки с пропусками?", "Очистка", 2, "dropna() удаляет строки, где есть NaN; fillna() заполняет пропуски.", ["df.clean()", "df.fillna()", "df.dropna()", "df.remove()"]),
            new("Как посчитать среднюю цену по городам?", "Группировка", 3, "groupby группирует строки, затем mean() считает среднее в каждой группе.", ["df.price.mean('city')", "df.mean().city", "df.sort('city').price", "df.groupby('city').price.mean()"]),
            new("Какой график лучше показывает распределение значений?", "Визуализация", 1, "Гистограмма показывает, как часто встречаются значения в разных диапазонах.", ["Круговая диаграмма", "Гистограмма", "Линейный график", "Таблица"]),
        ]),
        ["Тест: сложность алгоритмов"] = ("medium", 15,
        [
            new("Какова сложность поиска элемента в несортированном массиве?", "Big O", 1, "В худшем случае нужно проверить все n элементов — O(n).", ["O(1)", "O(n)", "O(log n)", "O(n²)"]),
            new("Какова сложность бинарного поиска?", "Big O", 2, "Каждый шаг делит диапазон пополам — O(log n). Работает только на отсортированных данных.", ["O(n)", "O(1)", "O(log n)", "O(n log n)"]),
            new("Сколько в среднем работает быстрая сортировка?", "Сортировки", 3, "Quicksort в среднем O(n log n), в худшем (плохой опорный элемент) — O(n²).", ["O(n)", "O(n²)", "O(log n)", "O(n log n)"]),
            new("Доступ по индексу в массиве — это…", "Структуры данных", 0, "Адрес элемента вычисляется сразу, поэтому доступ мгновенный — O(1).", ["O(1)", "O(n)", "O(log n)", "O(n²)"]),
            new("Сложность двух вложенных циклов по n?", "Big O", 3, "Внутренний цикл выполняется n раз для каждой из n итераций внешнего — n·n = O(n²).", ["O(n)", "O(2n)", "O(log n)", "O(n²)"], "for (int i = 0; i < n; i++)\n    for (int j = 0; j < n; j++)\n        sum += a[i] * a[j];"),
        ]),
        ["Тест: основы ML"] = ("medium", 10,
        [
            new("Зачем делить данные на обучающую и тестовую выборки?", "Оценка", 2, "Тест на невиденных данных показывает, как модель будет работать в реальности, а не как она запомнила примеры.", ["Чтобы обучение шло быстрее", "Чтобы данных было больше", "Чтобы честно проверить модель на новых данных", "Так требует Python"]),
            new("Какая задача — регрессия?", "Задачи", 0, "Регрессия предсказывает число (цену, температуру); классификация — класс.", ["Предсказать цену квартиры", "Отличить кошку от собаки", "Найти спам", "Сгруппировать клиентов"]),
            new("Что такое переобучение?", "Оценка", 1, "Переобученная модель запомнила обучающие примеры и плохо работает на новых.", ["Модель обучалась слишком долго по времени", "Модель отлично работает на обучении и плохо на новых данных", "Модель не обучилась", "Слишком мало признаков"]),
            new("Какая метрика подходит для регрессии?", "Метрики", 3, "Средняя квадратичная ошибка (MSE) меряет, насколько предсказания отличаются от реальных чисел.", ["Accuracy", "Precision", "Recall", "MSE"]),
            new("Какой метод в scikit-learn обучает модель?", "scikit-learn", 1, "fit(X, y) обучает модель, predict(X) — предсказывает.", ["train()", "fit()", "learn()", "run()"]),
        ]),
        ["Quiz: IT vocabulary"] = ("easy", 10,
        [
            new("What does “deploy” mean?", "Vocabulary", 2, "To deploy = to release code to a server so users can use it.", ["To delete code", "To write tests", "To release code to a server", "To rename a file"]),
            new("A “pull request” is…", "Git", 0, "A pull request asks teammates to review and merge your branch.", ["A request to review and merge changes", "A download of the repository", "A bug report", "A server error"]),
            new("Which phrase is the most polite in a code review?", "Code review", 3, "Questions and suggestions sound friendlier than orders.", ["Fix this.", "This is wrong.", "Why did you do this?!", "Could we extract this into a helper?"]),
            new("“Deadline” means…", "Vocabulary", 1, "A deadline is the latest time something must be done.", ["The first day of a project", "The latest time to finish", "A broken line of code", "A meeting"]),
            new("What is “documentation”?", "Vocabulary", 0, "Documentation explains how to use and maintain software.", ["Written explanation of how software works", "A list of bugs", "A type of database", "A programming language"]),
        ]),
    };

    public static void Run(AppDbContext db)
    {
        if (db.Tests.Any()) return;

        var lessons = db.Lessons.Include(l => l.Module).Where(l => l.Type == "test" || l.Type == "assignment").ToList();
        foreach (var l in lessons.Where(l => l.Type == "test" && Quizzes.ContainsKey(l.Title)))
        {
            var (difficulty, minutes, qs) = Quizzes[l.Title];
            db.Tests.Add(new TestEntity
            {
                CourseId = l.Module.CourseId, LessonId = l.Id, Title = l.Title.Replace("Тест: ", "").Replace("Quiz: ", ""),
                Difficulty = difficulty, Minutes = minutes,
                Questions = qs.Select((q, i) => new QuestionEntity
                {
                    Order = i, Text = q.Text, Topic = q.Topic, Correct = q.Correct, Explanation = q.Explanation,
                    Options = string.Join('\n', q.Options), Code = q.Code,
                }).ToList(),
            });
            l.Duration = $"{qs.Length} вопр.";
            l.Body = "Проверьте себя: ответьте на вопросы по урокам модуля. Тест засчитывается при результате от 70% — тогда урок отмечается пройденным.";
        }

        var due = 7;
        foreach (var l in lessons.Where(l => l.Type == "assignment"))
        {
            db.Assignments.Add(new AssignmentEntity
            {
                CourseId = l.Module.CourseId, LessonId = l.Id, Title = l.Title.Replace("Практика: ", ""),
                Description = l.Body, MaxScore = 100, Xp = 100, DueDays = due, Formats = ".zip,.pdf,.docx,.sql,.php,.js,.py,.ipynb,.fig,.png", MaxMb = 25,
            });
            due += 3;
        }
        db.SaveChanges();
    }
}
