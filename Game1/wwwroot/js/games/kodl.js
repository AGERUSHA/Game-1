/* Codle — Wordle for developers. Daily word (same for everyone) or random practice word. */
(function () {
  "use strict";
  const G = window.EduGames, K = window.KODL_T, $ = (s) => document.querySelector(s);

  // 5-letter IT terms with a short meaning (ru / kk / en)
  const WORDS = [
    ["ARRAY", "Упорядоченный набор элементов с индексами", "Индекстері бар реттелген элементтер жиыны", "An ordered collection of elements with indexes"],
    ["CLASS", "Шаблон объектов в ООП", "ООП-тағы объектілер үлгісі", "A blueprint for objects in OOP"],
    ["CONST", "Объявление неизменяемой переменной", "Өзгермейтін айнымалыны жариялау", "Declares a variable that can't be reassigned"],
    ["WHILE", "Цикл, пока условие истинно", "Шарт ақиқат болғанша цикл", "A loop that runs while a condition holds"],
    ["BREAK", "Досрочный выход из цикла", "Циклден мерзімінен бұрын шығу", "Exits a loop early"],
    ["FETCH", "Функция JS для HTTP-запросов", "HTTP-сұрауларға арналған JS функциясы", "The JS function for HTTP requests"],
    ["QUERY", "Запрос к базе данных", "Деректер қорына сұрау", "A request to a database"],
    ["INDEX", "Структура для быстрого поиска в таблице", "Кестеде жылдам іздеу құрылымы", "A structure that speeds up table lookups"],
    ["MERGE", "Слияние веток в Git", "Git-те тармақтарды біріктіру", "Joining branches in Git"],
    ["STACK", "Структура «последним пришёл — первым ушёл»", "«Соңғы келген — бірінші кетеді» құрылымы", "Last-in, first-out structure"],
    ["QUEUE", "Структура «первым пришёл — первым ушёл»", "«Бірінші келген — бірінші кетеді» құрылымы", "First-in, first-out structure"],
    ["TUPLE", "Неизменяемая последовательность в Python", "Python-дағы өзгермейтін тізбек", "An immutable sequence in Python"],
    ["FLOAT", "Число с плавающей точкой", "Жылжымалы нүктелі сан", "A floating-point number"],
    ["PRINT", "Вывод в консоль в Python", "Python-да консольге шығару", "Prints to the console in Python"],
    ["YIELD", "Возврат значения из генератора", "Генератордан мән қайтару", "Returns a value from a generator"],
    ["ASYNC", "Асинхронная функция", "Асинхронды функция", "An asynchronous function"],
    ["AWAIT", "Ожидание результата промиса", "Промис нәтижесін күту", "Waits for a promise to resolve"],
    ["CACHE", "Быстрое хранилище готовых результатов", "Дайын нәтижелердің жылдам қоймасы", "Fast storage for ready results"],
    ["CLONE", "Скопировать репозиторий себе", "Репозиторийді өзіңізге көшіру", "Copy a repository locally"],
    ["DEBUG", "Поиск и исправление ошибок", "Қателерді іздеу және түзету", "Finding and fixing bugs"],
    ["ERROR", "Ошибка при выполнении", "Орындау кезіндегі қате", "Something went wrong at runtime"],
    ["EVENT", "Событие: клик, ввод, загрузка", "Оқиға: басу, енгізу, жүктеу", "Something that happens: click, input, load"],
    ["FALSE", "Логическая «ложь»", "Логикалық «жалған»", "Boolean false"],
    ["FIELD", "Поле класса или таблицы", "Класс немесе кесте өрісі", "A class or table field"],
    ["GRAPH", "Вершины и рёбра между ними", "Төбелер мен олардың арасындағы қырлар", "Vertices connected by edges"],
    ["LINUX", "Популярная ОС для серверов", "Серверлерге арналған танымал ОЖ", "A popular server operating system"],
    ["LOGIN", "Вход в систему", "Жүйеге кіру", "Signing in"],
    ["MODEL", "M в MVC: данные и логика", "MVC-дағы M: деректер мен логика", "The M in MVC: data and logic"],
    ["MUTEX", "Блокировка для потоков", "Ағындарға арналған құлып", "A lock for threads"],
    ["PARSE", "Разобрать текст в структуру", "Мәтінді құрылымға талдау", "Turn text into a structure"],
    ["PIXEL", "Точка на экране", "Экрандағы нүкте", "A dot on the screen"],
    ["PROXY", "Посредник между клиентом и сервером", "Клиент пен сервер арасындағы делдал", "A middleman between client and server"],
    ["REACT", "JS-библиотека для интерфейсов", "Интерфейстерге арналған JS кітапханасы", "A JS library for building UIs"],
    ["REGEX", "Регулярное выражение", "Тұрақты өрнек", "A regular expression"],
    ["ROUTE", "Маршрут: URL → контроллер", "Маршрут: URL → контроллер", "Maps a URL to a controller"],
    ["SCOPE", "Область видимости переменной", "Айнымалының көріну аймағы", "Where a variable is visible"],
    ["SHELL", "Командная оболочка", "Командалық қабық", "A command-line shell"],
    ["SLICE", "Срез части массива или строки", "Массив не жол бөлігінің кесіндісі", "A slice of an array or string"],
    ["SPLIT", "Разбить строку на части", "Жолды бөліктерге бөлу", "Split a string into parts"],
    ["TABLE", "Таблица в базе данных", "Деректер қорындағы кесте", "A database table"],
    ["TOKEN", "Ключ доступа или лексема", "Қол жеткізу кілті немесе лексема", "An access key or a lexeme"],
    ["TRAIT", "Переиспользуемый набор методов в PHP", "PHP-дағы қайта қолданылатын әдістер жиыны", "A reusable set of methods in PHP"],
    ["VALUE", "Значение переменной", "Айнымалының мәні", "A variable's value"],
    ["BYTES", "Единицы данных по 8 бит", "8 биттік деректер бірліктері", "Units of 8 bits"],
    ["CHMOD", "Изменить права файла в Linux", "Linux-та файл құқығын өзгерту", "Change file permissions in Linux"],
    ["CRASH", "Аварийное падение программы", "Бағдарламаның апаттық құлауы", "A program crash"],
    ["FLOOR", "Округление вниз", "Төмен дөңгелектеу", "Round down"],
    ["FRAME", "Кадр анимации или фрейм", "Анимация кадры немесе фрейм", "An animation frame"],
    ["INPUT", "Поле ввода или входные данные", "Енгізу өрісі немесе кіріс деректер", "An input field or input data"],
    ["LAYER", "Слой архитектуры", "Архитектура қабаты", "An architecture layer"],
    ["LOCAL", "Локальный, не глобальный", "Жергілікті, жаһандық емес", "Local, not global"],
    ["MATCH", "Сопоставление с образцом", "Үлгімен салыстыру", "Pattern matching"],
    ["NGINX", "Веб-сервер и обратный прокси", "Веб-сервер және кері прокси", "A web server and reverse proxy"],
    ["PATCH", "HTTP-метод частичного обновления", "Ішінара жаңартудың HTTP әдісі", "HTTP method for partial updates"],
    ["PIVOT", "Опорный элемент в быстрой сортировке", "Жылдам сұрыптаудағы тірек элемент", "The pivot in quicksort"],
    ["RANGE", "Диапазон значений", "Мәндер ауқымы", "A range of values"],
    ["ROUND", "Округление числа", "Санды дөңгелектеу", "Round a number"],
    ["SHIFT", "Удалить первый элемент массива", "Массивтің бірінші элементін жою", "Remove the first array item"],
    ["SLEEP", "Пауза в выполнении", "Орындаудағы үзіліс", "Pause execution"],
    ["STYLE", "Оформление элемента в CSS", "CSS-тағы элемент безендіруі", "An element's CSS styling"],
    ["SUPER", "Обращение к родительскому классу", "Ата-ана класына жүгіну", "Calls the parent class"],
    ["SWIFT", "Язык Apple для iOS", "iOS-қа арналған Apple тілі", "Apple's language for iOS"],
    ["TIMER", "Таймер, отложенный вызов", "Таймер, кешіктірілген шақыру", "A timer, a delayed call"],
    ["TRACE", "След вызовов при ошибке", "Қате кезіндегі шақырулар ізі", "The call trace of an error"],
    ["UNION", "Объединение результатов SQL", "SQL нәтижелерін біріктіру", "Combine SQL results"],
    ["WHERE", "Условие фильтрации в SQL", "SQL-дағы сүзу шарты", "The SQL filter clause"],
    ["WRITE", "Запись данных", "Деректерді жазу", "Writing data"],
    ["CLICK", "Событие нажатия мышью", "Тышқанмен басу оқиғасы", "A mouse click event"],
    ["ASSET", "Статический ресурс: CSS, JS, картинки", "Статикалық ресурс: CSS, JS, суреттер", "A static asset: CSS, JS, images"],
    ["BLOCK", "Блок кода в фигурных скобках", "Фигуралы жақшадағы код блогы", "A code block in braces"],
    ["BUILD", "Сборка проекта", "Жобаны жинау", "Building the project"],
    ["CHAIN", "Цепочка вызовов методов", "Әдістерді шақыру тізбегі", "A chain of method calls"],
    ["COUNT", "Подсчёт количества строк", "Жолдар санын есептеу", "Counting rows"],
    ["GUARD", "Защитная проверка в начале функции", "Функция басындағы қорғаныш тексеру", "A defensive check at the start of a function"],
    ["ALERT", "Всплывающее окно в браузере", "Браузердегі қалқымалы терезе", "A browser pop-up"],
    ["BOOLS", "Логические значения true/false", "true/false логикалық мәндері", "true/false values"],
    ["CYCLE", "Цикл, повторение", "Цикл, қайталау", "A cycle, repetition"],
    ["DELTA", "Разница между значениями", "Мәндер арасындағы айырма", "A difference between values"],
  ].map(([w, ru, kk, en]) => ({ w, hint: { ru, kk, en } }));

  // Daily order: a fixed shuffle so the daily word isn't alphabetical
  const dailyOrder = (() => { const r = G.seeded(20261011), a = WORDS.slice(); for (let i = a.length - 1; i > 0; i--) { const j = Math.floor(r() * (i + 1)); [a[i], a[j]] = [a[j], a[i]]; } return a; })();
  const KB = ["QWERTYUIOP", "ASDFGHJKL", "↵ZXCVBNM⌫"];

  let target, mode, row, col, grid, done, hintUsed, evals;

  function buildBoard() {
    $("[data-board]").innerHTML = Array.from({ length: 6 }, () => `<div class="kodl-row">${'<span class="kt"></span>'.repeat(5)}</div>`).join("");
    $("[data-kb]").innerHTML = KB.map(r => `<div class="kodl-kbrow">${[...r].map(k => `<button type="button" class="kk ${k === "↵" || k === "⌫" ? "wide" : ""}" data-k="${k}">${k === "↵" ? "Enter" : k}</button>`).join("")}</div>`).join("");
  }
  const tile = (r, c) => $("[data-board]").children[r].children[c];
  const msg = (t, cls = "") => { const m = $("[data-kodl-msg]"); m.textContent = t; m.className = "kodl-msg " + cls; };

  function start() {
    mode = G.option("mode") || "daily";
    const st = G.state().kodl || {};
    if (mode === "daily" && st.date === G.todayKey()) { restoreDaily(st); return; }
    target = mode === "daily" ? dailyOrder[G.dayNumber() % dailyOrder.length] : WORDS[G.rnd(WORDS.length)];
    row = 0; col = 0; done = false; hintUsed = false; grid = Array.from({ length: 6 }, () => []); evals = [];
    buildBoard();
    $("[data-kodl-mode]").textContent = mode === "daily" ? `${K.daily} #${G.dayNumber() + 1}` : K.practice;
    $("[data-kodl-hinttext]").hidden = true; $("[data-kodl-hint]").disabled = false; $("[data-kodl-share]").hidden = true;
    msg("");
    G.owl.say(mode === "daily" ? K.daily + "! 🔤" : "🔤", 1800);
  }

  // The daily word was already played today: show the finished board
  function restoreDaily(st) {
    target = WORDS.find(x => x.w === st.word) || dailyOrder[G.dayNumber() % dailyOrder.length];
    buildBoard(); done = true; evals = st.evals || [];
    (st.guesses || []).forEach((g, r) => [...g].forEach((ch, c) => { const t = tile(r, c); t.textContent = ch; t.classList.add("filled", ["x", "y", "g"][evals[r]?.[c] ?? 0]); }));
    $("[data-kodl-mode]").textContent = `${K.daily} #${G.dayNumber() + 1}`;
    showMeaning(); $("[data-kodl-share]").hidden = false; $("[data-kodl-hint]").disabled = true;
    msg(K.already, "info");
  }

  function evaluate(guess) {
    const res = Array(5).fill(0), left = {};
    [...target.w].forEach((ch, i) => { if (guess[i] === ch) res[i] = 2; else left[ch] = (left[ch] || 0) + 1; });
    [...guess].forEach((ch, i) => { if (res[i] !== 2 && left[ch]) { res[i] = 1; left[ch]--; } });
    return res;
  }

  function type(k) {
    if (done) return;
    if (k === "⌫" || k === "BACKSPACE") { if (col > 0) { col--; grid[row].pop(); const t = tile(row, col); t.textContent = ""; t.classList.remove("filled"); } return; }
    if (k === "↵" || k === "ENTER") { submit(); return; }
    if (!/^[A-Z]$/.test(k) || col >= 5) return;
    grid[row].push(k); const t = tile(row, col); t.textContent = k; t.classList.add("filled", "pop"); setTimeout(() => t.classList.remove("pop"), 120); col++;
    G.sfx.click();
  }

  function submit() {
    if (col < 5) { msg(K.notWord, "bad"); G.shake($("[data-board]").children[row]); G.sfx.bad(); return; }
    const guess = grid[row].join(""), res = evaluate(guess);
    evals.push(res);
    const r = row;
    res.forEach((v, i) => setTimeout(() => {
      const t = tile(r, i); t.classList.add("flip");
      setTimeout(() => { t.classList.add(["x", "y", "g"][v]); paintKey(guess[i], v); }, 180);
    }, i * 220));
    const win = res.every(v => v === 2);
    row++; col = 0;
    setTimeout(() => {
      if (win) end(true);
      else if (row >= 6) end(false);
      else { if (res.some(v => v)) G.sfx.ok(); else G.sfx.click(); }
    }, 5 * 220 + 260);
    if (win || row >= 6) done = true;
  }

  function paintKey(ch, v) {
    const b = document.querySelector(`[data-k="${ch}"]`); if (!b) return;
    const cur = b.classList.contains("g") ? 2 : b.classList.contains("y") ? 1 : b.classList.contains("x") ? 0 : -1;
    if (v > cur) { b.classList.remove("x", "y", "g"); b.classList.add(["x", "y", "g"][v]); }
  }

  function showMeaning() {
    const h = $("[data-kodl-hinttext]");
    h.innerHTML = `<b>${G.esc(target.w)}</b> — ${G.esc(G.tr(target.hint))}`; h.hidden = false;
  }

  function end(win) {
    const tries = row;
    showMeaning();
    $("[data-kodl-share]").hidden = false; $("[data-kodl-hint]").disabled = true;
    msg(win ? K.solved.replace("{n}", tries) : K.failed.replace("{w}", target.w), win ? "ok" : "bad");
    let streak = 0;
    if (mode === "daily") {
      const s = G.patch(st => {
        const prev = st.kodl || {};
        const yesterday = new Date(Date.now() - 86400000); const yKey = `${yesterday.getFullYear()}-${String(yesterday.getMonth() + 1).padStart(2, "0")}-${String(yesterday.getDate()).padStart(2, "0")}`;
        const streakNow = win ? (prev.lastSolved === yKey ? (prev.streak || 0) + 1 : 1) : 0;
        st.kodl = { date: G.todayKey(), word: target.w, solved: win, guesses: grid.slice(0, tries).map(g => g.join("")), evals, streak: streakNow, lastSolved: win ? G.todayKey() : prev.lastSolved, everSolved: prev.everSolved || win };
      });
      streak = s.kodl.streak;
    }
    const base = win ? 10 + (7 - tries) * 5 : 0;
    setTimeout(() => G.finish("kodl", {
      win, base, perfect: win && !hintUsed && tries <= 3, score: win ? (7 - tries) * 100 : 0,
      stars: win ? (tries <= 2 ? 3 : tries <= 4 ? 2 : 1) : 0,
      title: win ? K.solved.replace("{n}", tries) : K.failed.replace("{w}", target.w),
      text: `${target.w} — ${G.tr(target.hint)}`,
      stats: [{ label: K.attempts, value: win ? `${tries}/6` : "—" }].concat(mode === "daily" ? [{ label: K.streak, value: "🔥 " + streak }] : []),
    }), 600);
  }

  function share() {
    const lines = evals.map(r => r.map(v => ["⬛", "🟨", "🟩"][v]).join("")).join("\n");
    const head = `${K.title} ${mode === "daily" ? "#" + (G.dayNumber() + 1) : ""} ${evals.at(-1)?.every(v => v === 2) ? evals.length : "X"}/6 · EDU AI`;
    navigator.clipboard?.writeText(head + "\n" + lines).then(() => window.eduToast?.(K.copied, "share-2"), () => window.eduToast?.(head));
  }

  document.addEventListener("click", e => {
    const k = e.target.closest("[data-k]"); if (k) type(k.dataset.k);
    if (e.target.closest("[data-kodl-hint]")) { hintUsed = true; showMeaning(); $("[data-kodl-hint]").disabled = true; G.owl.think(); }
    if (e.target.closest("[data-kodl-share]")) share();
  });
  document.addEventListener("keydown", e => {
    if (document.querySelector("[data-game-stage]").hidden || e.ctrlKey || e.metaKey || e.altKey) return;
    const k = e.key.toUpperCase();
    if (k === "ENTER" || k === "BACKSPACE" || /^[A-Z]$/.test(k)) { e.preventDefault(); type(k); }
  });

  G.intro(start);
})();
