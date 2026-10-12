/* Blitz quiz — 60 seconds, mixed true/false and multiple choice. Questions reshuffle every round. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.BZ_T, $ = (s) => document.querySelector(s);
  const tf = (topic, q, a, ex) => ({ topic, q, tf: true, a });
  const mc = (topic, q, a, w) => ({ topic, q, a, w });

  // q: { ru, kk, en } — answers are language-neutral
  const BANK = [
    tf("web", { ru: "HTTP-код 404 означает «Не найдено».", kk: "HTTP 404 коды «Табылмады» дегенді білдіреді.", en: "HTTP 404 means “Not Found”." }, true),
    tf("web", { ru: "Метод GET должен изменять данные на сервере.", kk: "GET әдісі сервердегі деректерді өзгертуі керек.", en: "GET requests should change data on the server." }, false),
    tf("web", { ru: "В CSS flexbox по умолчанию направление — row.", kk: "CSS flexbox-та әдепкі бағыт — row.", en: "In CSS flexbox the default direction is row." }, true),
    tf("web", { ru: "localStorage очищается при закрытии вкладки.", kk: "localStorage қойынды жабылғанда тазаланады.", en: "localStorage is cleared when the tab closes." }, false),
    tf("web", { ru: "=== в JavaScript не приводит типы.", kk: "JavaScript-тегі === типтерді түрлендірмейді.", en: "=== in JavaScript doesn't coerce types." }, true),
    tf("web", { ru: "Promise.all ждёт выполнения всех промисов.", kk: "Promise.all барлық промистерді күтеді.", en: "Promise.all waits for every promise." }, true),
    tf("web", { ru: "HTML-атрибут id может повторяться на странице.", kk: "HTML id атрибуты бетте қайталана алады.", en: "An HTML id may repeat on a page." }, false),
    tf("web", { ru: "Код 500 — ошибка на стороне клиента.", kk: "500 коды — клиент жағындағы қате.", en: "Status 500 is a client-side error." }, false),
    mc("web", { ru: "Какой HTTP-метод частично обновляет ресурс?", kk: "Қай HTTP әдісі ресурсты ішінара жаңартады?", en: "Which HTTP method partially updates a resource?" }, "PATCH", ["PUT", "POST", "GET"]),
    mc("web", { ru: "Какой код ответа означает «Создано»?", kk: "Қай жауап коды «Жасалды» дегенді білдіреді?", en: "Which status code means “Created”?" }, "201", ["200", "204", "302"]),
    mc("web", { ru: "Какое свойство CSS делает сетку?", kk: "Қай CSS қасиеті тор жасайды?", en: "Which CSS value creates a grid?" }, "display: grid", ["display: table", "float: grid", "position: grid"]),
    mc("web", { ru: "Что вернёт typeof [] ?", kk: "typeof [] не қайтарады?", en: "What does typeof [] return?" }, "object", ["array", "list", "undefined"]),
    mc("web", { ru: "Каким тегом подключают JS-файл?", kk: "JS файлы қай тегпен қосылады?", en: "Which tag loads a JS file?" }, "<script>", ["<js>", "<link>", "<code>"]),
    mc("web", { ru: "Где в Laravel описывают маршруты сайта?", kk: "Laravel-де сайт маршруттары қайда жазылады?", en: "Where are web routes defined in Laravel?" }, "routes/web.php", ["app/routes.php", "config/web.php", "public/index.php"]),
    tf("data", { ru: "PRIMARY KEY может содержать NULL.", kk: "PRIMARY KEY NULL қамти алады.", en: "A PRIMARY KEY may contain NULL." }, false),
    tf("data", { ru: "LEFT JOIN сохраняет все строки левой таблицы.", kk: "LEFT JOIN сол кестенің барлық жолын сақтайды.", en: "LEFT JOIN keeps every row of the left table." }, true),
    tf("data", { ru: "HAVING фильтрует строки до группировки.", kk: "HAVING топтауға дейін жолдарды сүзеді.", en: "HAVING filters rows before grouping." }, false),
    tf("data", { ru: "Индекс ускоряет поиск, но замедляет вставку.", kk: "Индекс іздеуді жылдамдатады, бірақ кірістіруді баяулатады.", en: "An index speeds up reads but slows inserts." }, true),
    tf("data", { ru: "COUNT(*) не считает строки с NULL.", kk: "COUNT(*) NULL жолдарын санамайды.", en: "COUNT(*) skips rows with NULL." }, false),
    mc("data", { ru: "Какой оператор убирает дубликаты?", kk: "Қай оператор қайталауларды алып тастайды?", en: "Which keyword removes duplicates?" }, "DISTINCT", ["UNIQUE", "ONLY", "SINGLE"]),
    mc("data", { ru: "Как выбрать первые 10 строк в SQLite/MySQL?", kk: "SQLite/MySQL-де алғашқы 10 жолды қалай таңдау керек?", en: "How do you take the first 10 rows in SQLite/MySQL?" }, "LIMIT 10", ["TOP 10", "FIRST 10", "ROWS 10"]),
    mc("data", { ru: "Какая функция считает среднее?", kk: "Қай функция орташаны есептейді?", en: "Which function computes an average?" }, "AVG()", ["MEAN()", "MID()", "AVERAGE()"]),
    mc("data", { ru: "Какая нормальная форма убирает транзитивные зависимости?", kk: "Қай қалыпты форма транзитивті тәуелділіктерді жояды?", en: "Which normal form removes transitive dependencies?" }, "3NF", ["1NF", "2NF", "BCNF"]),
    mc("data", { ru: "Какая команда отменяет транзакцию?", kk: "Қай команда транзакцияны болдырмайды?", en: "Which command undoes a transaction?" }, "ROLLBACK", ["UNDO", "REVERT", "CANCEL"]),
    tf("cs", { ru: "Двоичный поиск работает только на отсортированных данных.", kk: "Екілік іздеу тек сұрыпталған деректерде жұмыс істейді.", en: "Binary search only works on sorted data." }, true),
    tf("cs", { ru: "Стек работает по принципу FIFO.", kk: "Стек FIFO принципімен жұмыс істейді.", en: "A stack is FIFO." }, false),
    tf("cs", { ru: "Сложность пузырьковой сортировки — O(n²).", kk: "Көпіршікті сұрыптау күрделілігі — O(n²).", en: "Bubble sort is O(n²)." }, true),
    tf("cs", { ru: "1 байт = 8 бит.", kk: "1 байт = 8 бит.", en: "1 byte = 8 bits." }, true),
    tf("cs", { ru: "Рекурсия без базового случая завершится сама.", kk: "Базалық жағдайсыз рекурсия өздігінен аяқталады.", en: "Recursion without a base case stops on its own." }, false),
    tf("cs", { ru: "Git rebase переписывает историю коммитов.", kk: "Git rebase коммит тарихын қайта жазады.", en: "git rebase rewrites commit history." }, true),
    mc("cs", { ru: "Сколько будет 1010 в двоичной системе?", kk: "Екілік жүйедегі 1010 нешеге тең?", en: "What is binary 1010 in decimal?" }, "10", ["5", "12", "1010"]),
    mc("cs", { ru: "Сложность поиска в хеш-таблице в среднем?", kk: "Хеш-кестеде іздеудің орташа күрделілігі?", en: "Average lookup time in a hash table?" }, "O(1)", ["O(n)", "O(log n)", "O(n²)"]),
    mc("cs", { ru: "Какая команда создаёт новую ветку и переключается на неё?", kk: "Қай команда жаңа тармақ жасап, оған ауысады?", en: "Which command creates and switches to a new branch?" }, "git switch -c", ["git branch -m", "git commit -b", "git merge -n"]),
    mc("cs", { ru: "Как называется структура «первым пришёл — первым ушёл»?", kk: "«Бірінші келген — бірінші кетеді» құрылымы қалай аталады?", en: "What is the first-in, first-out structure called?" }, "Queue", ["Stack", "Tree", "Heap"]),
    mc("cs", { ru: "Какая команда Linux показывает текущую папку?", kk: "Қай Linux командасы ағымдағы қалтаны көрсетеді?", en: "Which Linux command prints the current folder?" }, "pwd", ["cd", "ls", "dir -c"]),
    mc("cs", { ru: "Какой алгоритм ищет кратчайший путь во взвешенном графе?", kk: "Қай алгоритм салмақты графта ең қысқа жолды табады?", en: "Which algorithm finds shortest paths in a weighted graph?" }, "Dijkstra", ["DFS", "Bubble", "Kruskal"]),
    tf("cs", { ru: "HTTPS шифрует передаваемые данные.", kk: "HTTPS берілетін деректерді шифрлайды.", en: "HTTPS encrypts data in transit." }, true),
    tf("cs", { ru: "Хранить пароли в открытом виде — нормально, если база закрыта.", kk: "База жабық болса, құпиясөздерді ашық сақтау қалыпты.", en: "Plain-text passwords are fine if the database is private." }, false),
    mc("cs", { ru: "Что из этого — атака на веб-приложение?", kk: "Мыналардың қайсысы веб-қосымшаға шабуыл?", en: "Which of these is a web attack?" }, "XSS", ["CSS", "DOM", "JSON"]),
    mc("cs", { ru: "Сколько бит в IPv4-адресе?", kk: "IPv4 мекенжайында неше бит?", en: "How many bits are in an IPv4 address?" }, "32", ["64", "128", "16"]),
  ];

  const SECONDS = 60;
  let queue, q, score, combo, answered, correct, shownAt, clock, review, over;

  function start() {
    const topic = G.option("topic") || "all";
    queue = G.shuffle(topic === "all" ? BANK : BANK.filter(b => b.topic === topic));
    score = 0; combo = 0; answered = 0; correct = 0; review = []; over = false;
    G.hud.score(0); G.hud.combo(0);
    clock?.stop(); clock = G.timer(SECONDS, (l, t) => G.hud.timer(l, t), finish).start();
    next();
  }

  function next() {
    if (over) return;
    if (!queue.length) queue = G.shuffle(BANK);
    q = queue.pop(); shownAt = Date.now();
    $("[data-bz-topic]").textContent = T.topics[q.topic];
    $("[data-bz-q]").textContent = G.tr(q.q);
    const card = $("[data-bz-card]"); card.classList.remove("in"); void card.offsetWidth; card.classList.add("in");
    const opts = q.tf ? [[true, T.yes], [false, T.no]] : G.shuffle([q.a, ...q.w]).map(o => [o, o]);
    const box = $("[data-bz-choices]");
    box.classList.toggle("tf", !!q.tf);
    box.innerHTML = opts.map(([v, label], k) => `<button class="g-choice ${q.tf ? (v ? "tf-yes" : "tf-no") : ""}" type="button" data-v="${G.esc(String(v))}"><span class="key">${q.tf ? (v ? "←" : "→") : k + 1}</span><span>${G.esc(label)}</span></button>`).join("");
  }

  function answer(btn) {
    if (over || !btn) return;
    const ok = btn.dataset.v === String(q.a);
    answered++;
    btn.classList.add(ok ? "ok" : "no");
    if (ok) {
      correct++; combo++;
      const fast = Date.now() - shownAt < 3000;
      const pts = (10 + (fast ? 5 : 0)) * Math.min(combo, 5);
      score += pts; G.hud.score(score); G.hud.combo(combo); G.floatText(btn, "+" + pts, fast ? "gold" : "");
      combo % 5 === 0 ? G.sfx.combo() : G.sfx.ok();
      if (combo % 5 === 0) G.owl.good();
    } else {
      combo = 0; G.hud.combo(0); clock.add(-3); G.floatText(btn, "−3s", "bad"); G.sfx.bad(); G.shake($("[data-bz-card]"));
      review.push(`${G.esc(G.tr(q.q))} → <b>${G.esc(q.tf ? (q.a ? T.yes : T.no) : q.a)}</b>`);
    }
    document.querySelectorAll("[data-v]").forEach(b => b.disabled = true);
    setTimeout(next, ok ? 260 : 650);
  }

  function finish() {
    if (over) return; over = true; clock.stop();
    const acc = answered ? Math.round(correct / answered * 100) : 0;
    G.finish("blitz", {
      win: correct >= 5, score, base: Math.min(60, correct * 3), perfect: correct >= 10 && correct === answered,
      stars: correct >= 20 ? 3 : correct >= 12 ? 2 : correct >= 5 ? 1 : 0,
      title: T.done, text: `${correct} ✓ · ${acc}%`,
      stats: [{ label: T.answered, value: answered }, { label: T.correct, value: correct }, { label: T.score, value: score }],
      review: review.slice(0, 6),
    });
  }

  document.addEventListener("click", e => { const b = e.target.closest("[data-v]"); if (b && !b.disabled) answer(b); });
  document.addEventListener("keydown", e => {
    if ($("[data-game-stage]").hidden || over) return;
    const btns = document.querySelectorAll("[data-v]:not(:disabled)");
    if (q?.tf && (e.key === "ArrowLeft" || e.key === "ArrowRight")) answer(btns[e.key === "ArrowLeft" ? 0 : 1]);
    const k = "1234".indexOf(e.key); if (k >= 0) answer(btns[k]);
  });
  G.intro(start);
})();
