/* Code puzzle (Parsons problems) — rebuild a program from shuffled lines; decoys hide classic mistakes. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.PZ_T, $ = (s) => document.querySelector(s);

  const BANK = [
    { lang: "python", task: { ru: "Функция возвращает сумму чётных чисел списка.", kk: "Функция тізімдегі жұп сандардың қосындысын қайтарады.", en: "Return the sum of the even numbers in a list." },
      lines: ["def sum_even(nums):", "    total = 0", "    for n in nums:", "        if n % 2 == 0:", "            total += n", "    return total"],
      decoys: ["    return n", "        if n % 2 == 1:"] },
    { lang: "js", task: { ru: "Загрузить пользователей с API и обработать ошибку HTTP.", kk: "API-ден пайдаланушыларды жүктеп, HTTP қатесін өңдеу.", en: "Load users from an API and handle HTTP errors." },
      lines: ["async function getUsers() {", "  const res = await fetch(\"/api/users\");", "  if (!res.ok) throw new Error(\"HTTP \" + res.status);", "  return await res.json();", "}"],
      decoys: ["  const res = fetch(\"/api/users\");", "  return res;"] },
    { lang: "php", task: { ru: "Метод контроллера Laravel показывает курс или 404.", kk: "Laravel контроллер әдісі курсты немесе 404 көрсетеді.", en: "A Laravel controller method shows a course or a 404." },
      lines: ["public function show(int $id)", "{", "    $course = Course::findOrFail($id);", "    return view('courses.show', ['course' => $course]);", "}"],
      decoys: ["    $course = Course::find($id)->first();", "    return $id;"] },
    { lang: "python", task: { ru: "Рекурсивный факториал.", kk: "Рекурсивті факториал.", en: "A recursive factorial." },
      lines: ["def fact(n):", "    if n <= 1:", "        return 1", "    return n * fact(n - 1)"],
      decoys: ["    return fact(n)", "    if n == 0"] },
    { lang: "js", task: { ru: "Найти максимум в массиве (в том числе отрицательных чисел).", kk: "Массивтегі максимумды табу (теріс сандарды қоса).", en: "Find the maximum of an array (negatives included)." },
      lines: ["function max(arr) {", "  let best = arr[0];", "  for (const x of arr) {", "    if (x > best) best = x;", "  }", "  return best;", "}"],
      decoys: ["  let best = 0;", "    if (x < best) best = x;"] },
    { lang: "sql", task: { ru: "Средний балл групп за 2026 год, только выше 80, по убыванию.", kk: "2026 жылғы топтардың орташа баллы, тек 80-нен жоғары, кему ретімен.", en: "Average score per group in 2026, only above 80, descending." },
      lines: ["SELECT group_name, AVG(score) AS avg_score", "FROM results", "WHERE year = 2026", "GROUP BY group_name", "HAVING AVG(score) > 80", "ORDER BY avg_score DESC;"],
      decoys: ["WHERE AVG(score) > 80", "ORDER BY group_name ASC;"] },
    { lang: "python", task: { ru: "Подсчитать, сколько раз встречается каждое слово.", kk: "Әр сөздің неше рет кездесетінін санау.", en: "Count how many times each word appears." },
      lines: ["counts = {}", "for word in text.split():", "    counts[word] = counts.get(word, 0) + 1", "print(counts)"],
      decoys: ["    counts[word] += 1", "counts = []"] },
    { lang: "php", task: { ru: "Безопасный вход: проверить хеш пароля и авторизовать.", kk: "Қауіпсіз кіру: құпиясөз хешін тексеріп, авторизациялау.", en: "Secure sign-in: verify the password hash, then log in." },
      lines: ["$user = User::where('email', $email)->first();", "if (!$user || !password_verify($password, $user->password_hash)) {", "    return back()->withErrors('Неверный email или пароль');", "}", "Auth::login($user);"],
      decoys: ["if ($user->password == $password) {", "Auth::login($email);"] },
    { lang: "js", task: { ru: "Двоичный поиск: вернуть индекс элемента или -1.", kk: "Екілік іздеу: элемент индексін немесе -1 қайтару.", en: "Binary search: return the index or -1." },
      lines: ["function search(arr, target) {", "  let lo = 0, hi = arr.length - 1;", "  while (lo <= hi) {", "    const mid = Math.floor((lo + hi) / 2);", "    if (arr[mid] === target) return mid;", "    if (arr[mid] < target) lo = mid + 1;", "    else hi = mid - 1;", "  }", "  return -1;", "}"],
      decoys: ["    const mid = (lo + hi) / 2;", "  while (lo < hi - 1) {"] },
  ];

  const ROUND = 4;
  let round, idx, pool, prog, checks, hints, score, solved, perfectAll, review, dragFrom;

  function start() {
    const lang = G.option("lang") || "all";
    const list = lang === "all" ? BANK : BANK.filter(b => b.lang === lang);
    round = G.sample(list, Math.min(ROUND, list.length));
    idx = 0; score = 0; solved = 0; perfectAll = true; review = [];
    G.hud.score(0);
    load();
  }

  function load() {
    const p = round[idx];
    checks = 0; hints = 0;
    pool = G.shuffle([...p.lines.map((t, k) => ({ t, k, decoy: false })), ...p.decoys.map((t, k) => ({ t, k: 100 + k, decoy: true }))]);
    prog = [];
    G.hud.progress(idx + 1, round.length);
    $("[data-pz-task]").textContent = G.tr(p.task);
    $("[data-pz-lang]").textContent = p.lang.toUpperCase();
    $("[data-pz-msg]").hidden = true; $("[data-pz-check]").hidden = false; $("[data-pz-next]").hidden = true;
    $("[data-pz-hint]").disabled = false; $("[data-pz-reset]").disabled = false;
    render();
  }

  const lineHtml = (it, lang) => `<code class="pz-code">${G.highlight(it.t, lang).replace(/^( +)/, m => "&nbsp;".repeat(m.length))}</code>`;
  function render(marks) {
    const lang = round[idx].lang;
    $("[data-pz-pool]").innerHTML = pool.map((it, i) => `<li><button type="button" class="pz-item" data-pool="${i}">${lineHtml(it, lang)}</button></li>`).join("");
    $("[data-pz-prog]").innerHTML = prog.map((it, i) => `<li class="pz-row ${marks ? (marks[i] ? "ok" : "no") : ""}" draggable="true" data-prog="${i}">
        <button type="button" class="pz-item" data-back="${i}">${lineHtml(it, lang)}</button>
        <span class="pz-move"><button type="button" class="icon-btn" data-up="${i}" aria-label="${T.up}"><i data-lucide="chevron-up" class="icon"></i></button><button type="button" class="icon-btn" data-down="${i}" aria-label="${T.down}"><i data-lucide="chevron-down" class="icon"></i></button></span></li>`).join("");
    $("[data-pz-empty]").hidden = prog.length > 0;
    window.eduIcons?.();
  }

  function check() {
    const p = round[idx]; checks++;
    const marks = prog.map((it, i) => !it.decoy && it.t === p.lines[i]);
    const ok = prog.length === p.lines.length && marks.every(Boolean);
    render(marks);
    const msg = $("[data-pz-msg]"); msg.hidden = false;
    if (ok) {
      const pts = Math.max(25, 120 - (checks - 1) * 30 - hints * 30);
      score += pts; solved++; G.hud.score(score);
      if (checks > 1 || hints) perfectAll = false;
      msg.innerHTML = `<b>${T.solved.replace("{p}", pts)}</b>`;
      G.sfx.win(); G.owl.good();
      $("[data-pz-check]").hidden = true; $("[data-pz-next]").hidden = false; $("[data-pz-hint]").disabled = true; $("[data-pz-reset]").disabled = true;
      $("[data-pz-pool]").innerHTML = "";
    } else {
      msg.innerHTML = prog.length < p.lines.length ? T.shortProg.replace("{n}", p.lines.length) + " " + T.wrong : T.wrong;
      G.sfx.bad(); G.owl.bad(); G.shake($("[data-pz-prog]"));
      const decoyUsed = prog.find(it => it.decoy);
      if (decoyUsed && !review.includes(decoyUsed.t)) review.push(`${T.decoy}: <code>${G.esc(decoyUsed.t.trim())}</code>`);
    }
  }

  function hint() {
    const p = round[idx]; hints++;
    // Put the first wrong position right
    const i = prog.findIndex((it, k) => it.t !== p.lines[k]);
    const pos = i < 0 ? prog.length : i;
    const need = p.lines[pos]; if (need == null) return;
    const fromPool = pool.findIndex(it => it.t === need && !it.decoy);
    const fromProg = prog.findIndex((it, k) => k > pos && it.t === need);
    let item;
    if (fromProg >= 0) item = prog.splice(fromProg, 1)[0]; else if (fromPool >= 0) item = pool.splice(fromPool, 1)[0];
    if (item) prog.splice(pos, 0, item);
    render();
    const msg = $("[data-pz-msg]"); msg.hidden = false; msg.textContent = T.hint.replace("{n}", pos + 1).replace("{l}", need.trim());
    G.owl.think();
  }

  function next() {
    if (idx >= round.length - 1) {
      return G.finish("puzzle", {
        win: solved > 0, score, base: Math.min(90, Math.round(score / 5)), perfect: perfectAll,
        stars: perfectAll ? 3 : score >= round.length * 80 ? 2 : 1,
        title: T.done, stats: [{ label: T.solvedCount, value: `${solved}/${round.length}` }, { label: T.score, value: score }], review,
      });
    }
    idx++; load();
  }

  document.addEventListener("click", e => {
    const a = e.target.closest("[data-pool]"); if (a) { prog.push(pool.splice(+a.dataset.pool, 1)[0]); G.sfx.click(); render(); return; }
    const b = e.target.closest("[data-back]"); if (b && !$("[data-pz-check]").hidden) { pool.push(prog.splice(+b.dataset.back, 1)[0]); render(); return; }
    const up = e.target.closest("[data-up]"); if (up) { const i = +up.dataset.up; if (i > 0) { [prog[i - 1], prog[i]] = [prog[i], prog[i - 1]]; render(); } return; }
    const dn = e.target.closest("[data-down]"); if (dn) { const i = +dn.dataset.down; if (i < prog.length - 1) { [prog[i + 1], prog[i]] = [prog[i], prog[i + 1]]; render(); } return; }
    if (e.target.closest("[data-pz-check]")) check();
    if (e.target.closest("[data-pz-hint]")) hint();
    if (e.target.closest("[data-pz-next]")) next();
    if (e.target.closest("[data-pz-reset]")) { pool = G.shuffle(pool.concat(prog)); prog = []; render(); $("[data-pz-msg]").hidden = true; }
  });
  // Drag to reorder inside the program
  document.addEventListener("dragstart", e => { const r = e.target.closest("[data-prog]"); if (r) { dragFrom = +r.dataset.prog; r.classList.add("dragging"); } });
  document.addEventListener("dragover", e => { if (e.target.closest("[data-prog]")) e.preventDefault(); });
  document.addEventListener("drop", e => {
    const r = e.target.closest("[data-prog]"); if (!r || dragFrom == null) return;
    e.preventDefault(); const to = +r.dataset.prog; const [it] = prog.splice(dragFrom, 1); prog.splice(to, 0, it); dragFrom = null; render();
  });
  document.addEventListener("dragend", () => { dragFrom = null; document.querySelectorAll(".dragging").forEach(x => x.classList.remove("dragging")); });

  G.intro(start);
})();
