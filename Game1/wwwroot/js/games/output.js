/* What's the output? — predict what a snippet prints. 18 items, 10 per round, answers shuffled. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.OUT_T, $ = (s) => document.querySelector(s);

  // a = correct answer, w = wrong answers
  const BANK = [
    { lang: "js", code: "console.log(0.1 + 0.2 === 0.3);", a: "false", w: ["true", "undefined", "NaN"],
      ex: { ru: "Дробные числа в двоичном виде неточны: 0.1 + 0.2 = 0.30000000000000004.", kk: "Бөлшек сандар екілік жүйеде дәл емес: 0.1 + 0.2 = 0.30000000000000004.", en: "Binary floats are inexact: 0.1 + 0.2 = 0.30000000000000004." } },
    { lang: "js", code: "console.log([1, 2, 3].map(x => x * 2).filter(x => x > 2));", a: "[4, 6]", w: ["[2, 4, 6]", "[6]", "[3]"],
      ex: { ru: "map удваивает → [2, 4, 6], filter оставляет больше 2 → [4, 6].", kk: "map екі есе → [2, 4, 6], filter 2-ден үлкенін қалдырады → [4, 6].", en: "map doubles → [2, 4, 6]; filter keeps > 2 → [4, 6]." } },
    { lang: "python", code: "print(len(\"Привет\"))", a: "6", w: ["12", "7", "5"],
      ex: { ru: "len считает символы, а не байты: 6 букв.", kk: "len байтты емес, таңбаларды санайды: 6 әріп.", en: "len counts characters, not bytes: 6 letters." } },
    { lang: "python", code: "nums = [3, 1, 2]\nnums.sort()\nprint(nums[-1])", a: "3", w: ["2", "1", "[1, 2, 3]"],
      ex: { ru: "После sort() список [1, 2, 3], индекс -1 — последний элемент.", kk: "sort()-тан кейін [1, 2, 3], -1 индексі — соңғы элемент.", en: "After sort() it's [1, 2, 3]; index -1 is the last item." } },
    { lang: "js", code: "console.log(typeof null);", a: "object", w: ["null", "undefined", "number"],
      ex: { ru: "Историческая ошибка JavaScript: typeof null возвращает \"object\".", kk: "JavaScript-тің тарихи қатесі: typeof null \"object\" қайтарады.", en: "A historic JavaScript quirk: typeof null is \"object\"." } },
    { lang: "php", code: "echo 10 % 3;", a: "1", w: ["3", "3.33", "0"],
      ex: { ru: "% — остаток от деления: 10 = 3·3 + 1.", kk: "% — бөлудің қалдығы: 10 = 3·3 + 1.", en: "% is the remainder: 10 = 3·3 + 1." } },
    { lang: "js", code: "let a = [1, 2];\nlet b = a;\nb.push(3);\nconsole.log(a.length);", a: "3", w: ["2", "1", "undefined"],
      ex: { ru: "Массивы передаются по ссылке: a и b — один и тот же массив.", kk: "Массивтер сілтеме бойынша беріледі: a мен b — бір массив.", en: "Arrays are references: a and b point to the same array." } },
    { lang: "python", code: "print(\"ab\" * 3)", a: "ababab", w: ["ab3", "Error", "aaabbb"],
      ex: { ru: "Умножение строки на число повторяет её.", kk: "Жолды санға көбейту оны қайталайды.", en: "Multiplying a string by a number repeats it." } },
    { lang: "js", code: "console.log(\"5\" + 3, \"5\" - 3);", a: "53 2", w: ["8 2", "53 53", "8 NaN"],
      ex: { ru: "+ со строкой склеивает, а - всегда превращает в числа.", kk: "+ жолмен жапсырады, ал - әрқашан санға айналдырады.", en: "+ with a string concatenates; - always converts to numbers." } },
    { lang: "python", code: "x = [i * i for i in range(4)]\nprint(x)", a: "[0, 1, 4, 9]", w: ["[1, 4, 9, 16]", "[0, 1, 2, 3]", "[0, 2, 4, 6]"],
      ex: { ru: "range(4) даёт 0, 1, 2, 3 — их квадраты 0, 1, 4, 9.", kk: "range(4) 0, 1, 2, 3 береді — квадраттары 0, 1, 4, 9.", en: "range(4) is 0, 1, 2, 3 — squares 0, 1, 4, 9." } },
    { lang: "js", code: "for (var i = 0; i < 3; i++) {\n  setTimeout(() => console.log(i));\n}", a: "3 3 3", w: ["0 1 2", "1 2 3", "undefined ×3"],
      ex: { ru: "var общая для всех итераций; к моменту вызова таймеров i = 3. С let было бы 0 1 2.", kk: "var барлық итерацияға ортақ; таймерлер шақырылғанда i = 3. let-пен 0 1 2 болар еді.", en: "var is shared by every iteration; when the timers run i = 3. With let it'd be 0 1 2." } },
    { lang: "php", code: "$a = \"5\";\nvar_dump($a == 5);", a: "bool(true)", w: ["bool(false)", "int(5)", "string(1) \"5\""],
      ex: { ru: "== в PHP приводит строку \"5\" к числу 5. Строгое === вернуло бы false.", kk: "PHP-дағы == \"5\" жолын 5 санына айналдырады. === false қайтарар еді.", en: "PHP's == converts \"5\" to 5. Strict === would give false." } },
    { lang: "python", code: "print(bool(\"\"), bool(\"0\"))", a: "False True", w: ["False False", "True True", "True False"],
      ex: { ru: "Пустая строка ложна, а \"0\" — непустая строка, поэтому истинна.", kk: "Бос жол жалған, ал \"0\" бос емес жол — ақиқат.", en: "An empty string is falsy; \"0\" is non-empty, so truthy." } },
    { lang: "js", code: "console.log([...\"abc\"].reverse().join(\"\"));", a: "cba", w: ["abc", "c,b,a", "[\"c\",\"b\",\"a\"]"],
      ex: { ru: "Строку разложили на буквы, развернули и склеили обратно.", kk: "Жол әріптерге бөлініп, аударылып, қайта жапсырылды.", en: "Split into letters, reversed, joined back." } },
    { lang: "python", code: "def f(n):\n    return 1 if n <= 1 else n * f(n - 1)\n\nprint(f(4))", a: "24", w: ["10", "4", "16"],
      ex: { ru: "Рекурсивный факториал: 4·3·2·1 = 24.", kk: "Рекурсивті факториал: 4·3·2·1 = 24.", en: "Recursive factorial: 4·3·2·1 = 24." } },
    { lang: "js", code: "const o = { a: 1 };\nconst { a, b = 5 } = o;\nconsole.log(a + b);", a: "6", w: ["1", "NaN", "undefined"],
      ex: { ru: "Деструктуризация: b нет в объекте, поэтому берётся значение по умолчанию 5.", kk: "Деструктуризация: объектте b жоқ, сондықтан әдепкі 5 алынады.", en: "Destructuring: b is missing, so its default 5 is used." } },
    { lang: "php", code: "echo strlen(\"Hi\") . count([1, 2, 3]);", a: "23", w: ["5", "2 3", "Error"],
      ex: { ru: "Точка в PHP — склейка строк: \"2\" . \"3\" = \"23\".", kk: "PHP-дағы нүкте — жолдарды жапсыру: \"2\" . \"3\" = \"23\".", en: "The dot in PHP concatenates: \"2\" . \"3\" = \"23\"." } },
    { lang: "js", code: "console.log([10, 1, 3].sort());", a: "[1, 10, 3]", w: ["[1, 3, 10]", "[10, 3, 1]", "[3, 1, 10]"],
      ex: { ru: "sort() без функции сравнивает как строки: \"10\" &lt; \"3\". Нужно sort((a, b) =&gt; a - b).", kk: "Функциясыз sort() жол ретінде салыстырады: \"10\" &lt; \"3\".", en: "sort() without a comparator compares strings: \"10\" &lt; \"3\". Use sort((a, b) =&gt; a - b)." } },
  ];

  const ROUND = 10, SECONDS = 20;
  let round, i, score, combo, correct, spent, clock, locked, review;

  function start() {
    const lang = G.option("lang") || "all";
    const pool = lang === "all" ? BANK : BANK.filter(b => b.lang === lang);
    round = G.sample(pool, Math.min(ROUND, pool.length));
    i = 0; score = 0; combo = 0; correct = 0; spent = 0; review = [];
    G.hud.score(0); G.hud.combo(0);
    clock?.stop(); clock = G.timer(SECONDS, (l, t) => G.hud.timer(l, t), () => answer(null));
    show();
  }

  function show() {
    const q = round[i]; locked = false;
    G.hud.progress(i + 1, round.length);
    $("[data-out-lang]").textContent = q.lang.toUpperCase();
    $("[data-out-code]").innerHTML = q.code.split("\n").map((ln, n) => `<div class="g-line"><span class="ln">${n + 1}</span><span>${G.highlight(ln, q.lang) || " "}</span></div>`).join("");
    const opts = G.shuffle([q.a, ...q.w]);
    $("[data-out-choices]").innerHTML = opts.map((o, k) => `<button class="g-choice" type="button" data-ans="${G.esc(o)}"><span class="key">${k + 1}</span><span>${G.esc(o)}</span></button>`).join("");
    $("[data-out-explain]").hidden = true; $("[data-out-next]").hidden = true;
    clock.reset(SECONDS); clock.start();
  }

  function answer(btn) {
    if (locked) return; locked = true; clock.stop();
    const q = round[i], used = SECONDS - clock.left; spent += used;
    const ok = btn && btn.dataset.ans === q.a;
    document.querySelectorAll("[data-ans]").forEach(b => { b.disabled = true; if (b.dataset.ans === q.a) b.classList.add("ok"); });
    if (ok) {
      combo++; correct++;
      const pts = Math.round((50 + clock.left * 5) * Math.min(combo, 4));
      score += pts; G.hud.score(score); G.hud.combo(combo); G.floatText(btn, "+" + pts);
      combo >= 3 ? G.sfx.combo() : G.sfx.ok(); G.owl.good();
    } else {
      btn?.classList.add("no"); combo = 0; G.hud.combo(0); G.sfx.bad(); G.owl.bad();
      review.push(`<code>${G.esc(q.code.split("\n").pop())}</code> → <b>${G.esc(q.a)}</b>. ${G.tr(q.ex)}`);
    }
    const e = $("[data-out-explain]");
    e.innerHTML = `<b>${ok ? T.right : (btn ? T.wrong : T.timeout).replace("{a}", G.esc(q.a))}</b><br>${G.tr(q.ex)}`;
    e.hidden = false; $("[data-out-next]").hidden = false;
  }

  function next() { if (i >= round.length - 1) return finish(); i++; show(); }

  function finish() {
    clock.stop();
    const n = round.length;
    G.finish("output", {
      win: correct >= n / 2, score, base: Math.min(100, correct * 8 + Math.round(score / 100)), perfect: correct === n,
      stars: correct === n ? 3 : correct >= n * .7 ? 2 : correct >= n / 2 ? 1 : 0,
      title: T.done, text: `${correct} / ${n}`,
      stats: [{ label: T.correct, value: `${correct}/${n}` }, { label: T.score, value: score }, { label: T.avg, value: (spent / n).toFixed(1) + "s" }],
      review,
    });
  }

  document.addEventListener("click", e => {
    const b = e.target.closest("[data-ans]"); if (b && !b.disabled) answer(b);
    if (e.target.closest("[data-out-next]")) next();
  });
  document.addEventListener("keydown", e => {
    if ($("[data-game-stage]").hidden) return;
    const k = "1234".indexOf(e.key);
    if (k >= 0 && !locked) { const b = document.querySelectorAll("[data-ans]")[k]; if (b) answer(b); }
    if (e.key === "Enter" && locked && !$("[data-out-next]").hidden) next();
  });
  G.intro(start);
})();
