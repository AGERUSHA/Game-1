/* Bug hunter — click the buggy line, then name the bug type. 18 snippets, 8 random per round. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.BH_T, $ = (s) => document.querySelector(s);

  // lang, file, code, bug (0-based line), type, explanation
  const BANK = [
    { lang: "php", file: "Stats.php", bug: 5, type: "logic",
      code: "function average(array $nums): float {\n    $sum = 0;\n    foreach ($nums as $n) {\n        $sum += $n;\n    }\n    return $sum / count($num);\n}",
      ex: { ru: "Опечатка: <code>$num</code> вместо <code>$nums</code> — переменная не определена, count() получит null.", kk: "Қате жазылған: <code>$nums</code> орнына <code>$num</code> — айнымалы анықталмаған.", en: "Typo: <code>$num</code> instead of <code>$nums</code> — the variable is undefined." } },
    { lang: "js", file: "list.js", bug: 1, type: "logic",
      code: "const items = [\"a\", \"b\", \"c\"];\nfor (let i = 0; i <= items.length; i++) {\n  console.log(items[i].toUpperCase());\n}",
      ex: { ru: "Выход за границу: <code>i &lt;= length</code> берёт индекс 3, которого нет. Нужно <code>i &lt; items.length</code>.", kk: "Шекарадан шығу: <code>i &lt;= length</code> жоқ 3-индексті алады. <code>i &lt; items.length</code> керек.", en: "Off-by-one: <code>i &lt;= length</code> reaches index 3, which doesn't exist. Use <code>i &lt; items.length</code>." } },
    { lang: "php", file: "UserController.php", bug: 1, type: "security",
      code: "$id = $_GET['id'];\n$sql = \"SELECT * FROM users WHERE id = \" . $id;\n$user = $db->query($sql)->fetch();",
      ex: { ru: "SQL-инъекция: ввод пользователя склеивается с запросом. Используйте подготовленные запросы с параметрами.", kk: "SQL-инъекция: пайдаланушы енгізгені сұрауға жапсырылады. Параметрлі сұрауларды қолданыңыз.", en: "SQL injection: user input is glued into the query. Use prepared statements with parameters." } },
    { lang: "python", file: "cart.py", bug: 0, type: "logic",
      code: "def add_item(item, bucket=[]):\n    bucket.append(item)\n    return bucket\n\nprint(add_item(\"a\"))\nprint(add_item(\"b\"))  # ['a', 'b'] ?!",
      ex: { ru: "Изменяемое значение по умолчанию создаётся один раз и «запоминает» прошлые вызовы. Пишите <code>bucket=None</code>.", kk: "Әдепкі өзгермелі мән бір рет жасалып, алдыңғы шақыруларды «есте сақтайды». <code>bucket=None</code> жазыңыз.", en: "A mutable default is created once and remembers earlier calls. Use <code>bucket=None</code>." } },
    { lang: "js", file: "api.js", bug: 1, type: "logic",
      code: "async function loadUser(id) {\n  const res = fetch(`/api/users/${id}`);\n  const data = await res.json();\n  return data.name;\n}",
      ex: { ru: "Нет <code>await</code> перед <code>fetch</code> — в <code>res</code> лежит промис, а не ответ.", kk: "<code>fetch</code> алдында <code>await</code> жоқ — <code>res</code> ішінде жауап емес, промис.", en: "Missing <code>await</code> before <code>fetch</code> — <code>res</code> is a promise, not a response." } },
    { lang: "php", file: "PostController.php", bug: 0, type: "perf",
      code: "$posts = Post::all();\nforeach ($posts as $post) {\n    echo $post->author->name;\n}",
      ex: { ru: "Проблема N+1: на каждый пост отдельный запрос автора. Нужно <code>Post::with('author')->get()</code>.", kk: "N+1 мәселесі: әр постқа авторды бөлек сұрау. <code>Post::with('author')->get()</code> керек.", en: "N+1 problem: one extra query per post. Use <code>Post::with('author')->get()</code>." } },
    { lang: "js", file: "check.js", bug: 1, type: "logic",
      code: "function isEmpty(value) {\n  return value == 0;\n}\nisEmpty(\"\"); // true ?!",
      ex: { ru: "<code>==</code> приводит типы: пустая строка равна 0. Используйте строгое <code>===</code>.", kk: "<code>==</code> типтерді түрлендіреді: бос жол 0-ге тең. Қатаң <code>===</code> қолданыңыз.", en: "<code>==</code> coerces types: an empty string equals 0. Use strict <code>===</code>." } },
    { lang: "python", file: "loop.py", bug: 0, type: "syntax",
      code: "for i in range(5)\n    print(i * i)",
      ex: { ru: "После заголовка цикла нужно двоеточие: <code>for i in range(5):</code>", kk: "Цикл тақырыбынан кейін қос нүкте керек: <code>for i in range(5):</code>", en: "A loop header needs a colon: <code>for i in range(5):</code>" } },
    { lang: "js", file: "hello.js", bug: 1, type: "security",
      code: "const name = new URLSearchParams(location.search).get(\"name\");\ndocument.querySelector(\"#hello\").innerHTML = \"Привет, \" + name;",
      ex: { ru: "XSS: параметр из URL вставляется как HTML. Используйте <code>textContent</code>.", kk: "XSS: URL параметрі HTML ретінде қойылады. <code>textContent</code> қолданыңыз.", en: "XSS: a URL parameter is inserted as HTML. Use <code>textContent</code>." } },
    { lang: "sql", file: "report.sql", bug: 2, type: "syntax",
      code: "SELECT group_name, AVG(score)\nFROM results\nWHERE AVG(score) > 80\nGROUP BY group_name;",
      ex: { ru: "Агрегаты нельзя в WHERE — фильтр групп пишут в <code>HAVING AVG(score) &gt; 80</code> после GROUP BY.", kk: "Агрегаттарды WHERE-де қолдануға болмайды — <code>HAVING</code> керек.", en: "Aggregates aren't allowed in WHERE — filter groups with <code>HAVING AVG(score) &gt; 80</code>." } },
    { lang: "php", file: "RegisterController.php", bug: 0, type: "security",
      code: "$user->password = $request->input('password');\n$user->save();",
      ex: { ru: "Пароль сохраняется открытым текстом. Храните только хеш: <code>Hash::make()</code> / <code>password_hash()</code>.", kk: "Құпиясөз ашық мәтінмен сақталады. Тек хешті сақтаңыз: <code>Hash::make()</code>.", en: "The password is stored in plain text. Store only a hash: <code>Hash::make()</code>." } },
    { lang: "js", file: "countdown.js", bug: 3, type: "logic",
      code: "let i = 10;\nwhile (i > 0) {\n  console.log(i);\n  i++;\n}",
      ex: { ru: "Бесконечный цикл: <code>i</code> растёт, а условие ждёт уменьшения. Нужно <code>i--</code>.", kk: "Шексіз цикл: <code>i</code> өседі. <code>i--</code> керек.", en: "Infinite loop: <code>i</code> grows while the condition expects it to shrink. Use <code>i--</code>." } },
    { lang: "python", file: "share.py", bug: 2, type: "logic",
      code: "total = 7\npeople = 2\nshare = total // people\nprint(f\"Каждому: {share}\")  # ждём 3.5",
      ex: { ru: "<code>//</code> — целочисленное деление (3). Для 3.5 нужен обычный <code>/</code>.", kk: "<code>//</code> — бүтін бөлу (3). 3.5 үшін кәдімгі <code>/</code> керек.", en: "<code>//</code> is integer division (3). Use <code>/</code> to get 3.5." } },
    { lang: "js", file: "orders.js", bug: 2, type: "perf",
      code: "const ids = users.map(u => u.id);\nfor (const order of orders) {\n  if (ids.indexOf(order.userId) !== -1) paid.push(order);\n}",
      ex: { ru: "<code>indexOf</code> в цикле — O(n·m). Сделайте <code>new Set(ids)</code> и проверяйте <code>has()</code> за O(1).", kk: "Цикл ішіндегі <code>indexOf</code> — O(n·m). <code>new Set(ids)</code> қолданыңыз.", en: "<code>indexOf</code> inside a loop is O(n·m). Use <code>new Set(ids)</code> and <code>has()</code>." } },
    { lang: "php", file: "hello.php", bug: 0, type: "syntax",
      code: "$name = \"Akerke\"\necho \"Hello, $name\";",
      ex: { ru: "В PHP каждая инструкция заканчивается точкой с запятой — её нет после присваивания.", kk: "PHP-да әр нұсқаулық нүктелі үтірмен аяқталады.", en: "PHP statements end with a semicolon — it's missing after the assignment." } },
    { lang: "js", file: "total.js", bug: 0, type: "logic",
      code: "const total = 0;\nfor (const p of prices) {\n  total += p;\n}",
      ex: { ru: "<code>const</code> нельзя переприсвоить — будет TypeError. Нужно <code>let total = 0</code>.", kk: "<code>const</code> қайта меншіктеуге болмайды. <code>let total = 0</code> керек.", en: "A <code>const</code> can't be reassigned — TypeError. Use <code>let total = 0</code>." } },
    { lang: "js", file: "pay.js", bug: 0, type: "security",
      code: "const API_KEY = \"sk-live-8f2a91c0\";\nfetch(\"https://api.pay.kz/charge\", {\n  headers: { Authorization: API_KEY }\n});",
      ex: { ru: "Секретный ключ лежит во фронтенде — его увидит любой. Запросы с ключом делают только на сервере.", kk: "Құпия кілт фронтендте — оны кез келген адам көреді. Кілтпен сұрауларды тек серверде жасаңыз.", en: "A secret key in front-end code is visible to everyone. Call the API from the server." } },
    { lang: "sql", file: "cleanup.sql", bug: 1, type: "logic",
      code: "-- удалить заблокированных пользователей\nDELETE FROM users;",
      ex: { ru: "Нет WHERE — удалятся все пользователи! Нужно <code>WHERE status = 'blocked'</code>.", kk: "WHERE жоқ — барлық пайдаланушы жойылады! <code>WHERE status = 'blocked'</code> керек.", en: "No WHERE — every user gets deleted! Add <code>WHERE status = 'blocked'</code>." } },
  ];

  const LEVEL = { junior: 30, middle: 20, senior: 12 };
  const ROUND = 8, LIVES = 3;
  let round, i, score, combo, bestCombo, lives, hits, perfectAll, clock, locked, review;

  function start() {
    round = G.sample(BANK, ROUND); i = 0; score = 0; combo = 0; bestCombo = 0; lives = LIVES; hits = 0; perfectAll = true; review = [];
    G.hud.score(0); G.hud.lives(lives, LIVES); G.hud.combo(0);
    clock?.stop();
    clock = G.timer(LEVEL[G.option("level")] || 20, (l, t) => G.hud.timer(l, t), () => miss(true));
    show();
  }

  function show() {
    const s = round[i]; locked = false;
    G.hud.progress(i + 1, ROUND);
    $("[data-bh-lang]").textContent = s.lang.toUpperCase();
    $("[data-bh-file]").textContent = s.file;
    $("[data-bh-code]").innerHTML = s.code.split("\n").map((ln, n) => `<button type="button" class="g-line" data-line="${n}"><span class="ln">${n + 1}</span><span>${G.highlight(ln, s.lang) || " "}</span></button>`).join("");
    $("[data-bh-types]").hidden = true; $("[data-bh-explain]").hidden = true; $("[data-bh-next]").hidden = true;
    document.querySelectorAll("[data-type]").forEach(b => { b.disabled = false; b.classList.remove("ok", "no"); });
    clock.reset(LEVEL[G.option("level")] || 20); clock.start();
  }

  function pickLine(btn) {
    if (locked) return;
    const s = round[i], n = +btn.dataset.line;
    locked = true; clock.stop();
    if (n === s.bug) {
      combo++; bestCombo = Math.max(bestCombo, combo); hits++;
      const pts = Math.round((100 + clock.left * 5) * Math.min(combo, 5));
      score += pts; G.hud.score(score); G.hud.combo(combo);
      btn.classList.add("is-bug"); G.floatText(btn, "+" + pts);
      combo >= 3 ? G.sfx.combo() : G.sfx.ok(); G.owl.good();
      $("[data-bh-types]").hidden = false;
    } else {
      btn.classList.add("is-picked"); miss(false);
    }
  }

  function miss(timeout) {
    const s = round[i];
    locked = true; clock.stop(); combo = 0; perfectAll = false; lives--;
    G.hud.combo(0); G.hud.lives(lives, LIVES); G.sfx.bad(); G.owl.bad();
    const bugBtn = document.querySelector(`[data-line="${s.bug}"]`); bugBtn?.classList.add("is-bug");
    G.shake(document.querySelector(".g-code"));
    review.push(`<b>${G.esc(s.file)}</b>: ${G.tr(s.ex)}`);
    explain((timeout ? T.timeout + " " : "") + T.missed.replace("{n}", s.bug + 1), s);
  }

  function pickType(btn) {
    const s = round[i];
    document.querySelectorAll("[data-type]").forEach(b => b.disabled = true);
    const ok = btn.dataset.type === s.type;
    btn.classList.add(ok ? "ok" : "no");
    if (!ok) { document.querySelector(`[data-type="${s.type}"]`).classList.add("ok"); perfectAll = false; G.sfx.bad(); }
    else { score += 50; G.hud.score(score); G.floatText(btn, "+50", "gold"); G.sfx.ok(); }
    explain(`${T.found}! ${(ok ? T.typeOk : T.typeNo).replace("{t}", T.types[s.type])}.`, s);
  }

  function explain(head, s) {
    const e = $("[data-bh-explain]");
    e.innerHTML = `<b>${G.esc(head)}</b><br>${G.tr(s.ex)}`; e.hidden = false;
    $("[data-bh-next]").hidden = false; $("[data-bh-next]").focus();
  }

  function next() {
    if (lives <= 0 || i >= ROUND - 1) return finish();
    i++; show();
  }

  function finish() {
    clock.stop();
    const acc = Math.round(hits / (i + 1) * 100);
    G.finish("bughunt", {
      win: lives > 0, score, base: Math.min(120, Math.round(score / 25)), perfect: perfectAll && lives === LIVES,
      stars: hits === ROUND ? 3 : hits >= ROUND - 2 ? 2 : hits >= ROUND / 2 ? 1 : 0,
      title: lives > 0 ? T.done : T.outOfLives,
      stats: [{ label: T.score, value: score }, { label: T.accuracy, value: acc + "%" }, { label: T.bestCombo, value: "×" + bestCombo }],
      review,
    });
  }

  document.addEventListener("click", e => {
    const ln = e.target.closest("[data-line]"); if (ln) pickLine(ln);
    const ty = e.target.closest("[data-type]"); if (ty && !ty.disabled) pickType(ty);
    if (e.target.closest("[data-bh-next]")) next();
  });
  G.intro(start);
})();
