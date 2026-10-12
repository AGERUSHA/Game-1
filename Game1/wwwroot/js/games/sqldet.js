/* SQL detective — a freshly generated city database (SQLite in the browser via sql.js) and a theft to solve. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.SD_T, $ = (s) => document.querySelector(s);

  const FIRST_M = ["Арман", "Данияр", "Ерлан", "Тимур", "Нурлан", "Санжар", "Алихан", "Бауыржан", "Руслан", "Максим", "Дамир", "Ильяс", "Азамат", "Айдос", "Олжас", "Ержан", "Канат", "Марат"];
  const FIRST_F = ["Айгерим", "Алина", "Дана", "Асель", "Жанель", "Мадина", "Сабина", "Нурай", "Камила", "Аружан", "Динара", "Карина", "Томирис", "Аяулым", "Мира", "Гульназ", "Зарина", "Лаура"];
  const LAST = ["Ахметов", "Сериков", "Нурланов", "Абенов", "Тохтаров", "Ибраев", "Омаров", "Каиров", "Есенов", "Оспанов", "Жумабаев", "Касымов", "Беков", "Мусин", "Садыков", "Смагулов", "Волков", "Петров", "Ким", "Ли"];
  const STREETS = ["Абая", "Толе би", "Сатпаева", "Жибек жолы", "Достык", "Гагарина", "Тимирязева", "Байтурсынова"];
  const JOBS = ["студент", "программист", "дизайнер", "курьер", "таксист", "бариста", "охранник", "преподаватель", "бухгалтер", "фотограф"];
  const COLORS = ["красный", "синий", "белый", "чёрный", "серебристый", "зелёный"];
  const MODELS = ["Toyota Camry", "Hyundai Accent", "Kia Rio", "Lada Granta", "Chevrolet Cobalt", "Skoda Octavia", "Hyundai Tucson"];
  const LETTERS = "ABCDEHKMOPTX";
  const DISTRICTS = ["Центр", "Север", "Юг", "Восток", "Запад"];
  const TYPES = ["кража", "вандализм", "мошенничество", "ДТП", "кража"];

  let db, rnd, data, found, queries, hints, wrong, solved;
  const pick = (a) => a[Math.floor(rnd() * a.length)];
  const int = (a, b) => a + Math.floor(rnd() * (b - a + 1));

  /* ---------- Case generator ---------- */
  function generate(seed) {
    rnd = G.seeded(seed);
    const people = [], cars = [], usedNames = new Set();
    const person = (id, gender, extra = {}) => {
      let name;
      do {
        const f = gender === "f" ? pick(FIRST_F) : pick(FIRST_M);
        let l = pick(LAST); if (gender === "f" && /(ов|ев|ин)$/.test(l)) l += "а";
        name = `${f} ${l}`;
      } while (usedNames.has(name));
      usedNames.add(name);
      return Object.assign({ id, name, job: pick(JOBS), street: pick(STREETS), house: int(1, 120), car_id: id }, extra);
    };
    for (let i = 1; i <= 80; i++) people.push(person(i, rnd() < .5 ? "f" : "m"));
    const plate = (pre, reg) => `${pre}${int(0, 9)} ${pick([...LETTERS])}${pick([...LETTERS])}${pick([...LETTERS])} ${reg}`;
    people.forEach(p => cars.push({ id: p.id, plate: plate(String(int(10, 99)), String(pick(["01", "02", "03", "05", "13", "17"]))), color: pick(COLORS), model: pick(MODELS) }));

    // Thief and three look-alike suspects (same color, plate prefix and region)
    const ids = G.shuffle(people.map(p => p.id));
    const thief = people[ids[0] - 1], suspects = [ids[1], ids[2], ids[3]].map(i => people[i - 1]);
    const color = pick(COLORS), pre = String(int(10, 99)), reg = pick(["01", "02"]);
    [thief, ...suspects].forEach(p => { const c = cars[p.id - 1]; c.color = color; c.plate = plate(pre, reg); });
    // nobody else shares that combination
    cars.forEach(c => { if (![thief, ...suspects].some(p => p.id === c.id) && c.color === color && c.plate.startsWith(pre) && c.plate.endsWith(reg)) c.color = COLORS.find(x => x !== color); });

    // Witnesses: the last house on one street, and a unique first name on another
    const w1 = people[ids[4] - 1], w2 = people[ids[5] - 1];
    w1.street = pick(STREETS); w1.house = 150;
    people.forEach(p => { if (p !== w1 && p.street === w1.street && p.house >= 150) p.house = int(1, 120); });
    w2.street = STREETS.find(s => s !== w1.street);
    const w2first = w2.name.split(" ")[0];
    people.forEach(p => { if (p !== w2 && p.street === w2.street && p.name.startsWith(w2first + " ")) p.street = STREETS.find(s => s !== w2.street && s !== w1.street); });

    // Crime reports (1 real + decoys)
    const reports = [];
    const desc = `Из музея EDU-Сити похищена Золотая сова. Свидетель 1 живёт в последнем доме на улице ${w1.street}. Свидетель 2 по имени ${w2first} живёт на улице ${w2.street}.`;
    for (let i = 0; i < 16; i++) {
      const d = `2026-10-${String(int(1, 20)).padStart(2, "0")}`;
      reports.push({ date: d === "2026-10-10" ? "2026-10-09" : d, district: pick(DISTRICTS), type: pick(TYPES), description: pick(["Повреждена витрина магазина.", "Украден велосипед у университета.", "Сообщение о подозрительном звонке.", "Столкновение двух машин на перекрёстке.", "Граффити на стене библиотеки.", "Пропал ноутбук в коворкинге."]) });
    }
    reports.push({ date: "2026-10-10", district: "Север", type: "кража", description: "Украден самокат у торгового центра." });
    reports.push({ date: "2026-10-10", district: "Центр", type: "вандализм", description: "Сломана скамейка в сквере." });
    reports.push({ date: "2026-10-10", district: "Центр", type: "кража", description: desc });
    const shuffled = G.shuffle(reports).map((r, i) => ({ id: i + 1, ...r }));
    const realId = shuffled.find(r => r.description === desc).id;

    // Interviews
    const interviews = [
      { person_id: w1.id, transcript: `Ночью видел, как человек выбежал из музея и уехал на машине. Цвет — ${color}, номер начинался на «${pre}», регион ${reg}.` },
      { person_id: w2.id, transcript: "Я охраняю вход в музей. 10 октября вор находился в музее примерно с 21:00 до 22:30 — это видно по журналу визитов." },
    ];
    G.sample(people.filter(p => p !== w1 && p !== w2), 8).forEach(p => interviews.push({ person_id: p.id, transcript: pick(["Ничего необычного не видел.", "В тот вечер я был дома.", "Слышал шум, но не выходил.", "Видел кота на крыше, больше ничего.", "Работал допоздна в офисе."]) }));

    // Museum visits: thief in the evening window, look-alikes at other times, plus random visitors
    const visits = [{ person_id: thief.id, date: "2026-10-10", time_in: "21:04", time_out: "22:21" }];
    suspects.forEach((p, k) => visits.push({ person_id: p.id, date: k === 0 ? "2026-10-09" : "2026-10-10", time_in: ["19:10", "11:30", "14:05"][k], time_out: ["20:40", "12:45", "15:20"][k] }));
    G.sample(people.filter(p => p !== thief && !suspects.includes(p)), 30).forEach(p => {
      const h = int(9, 19);
      visits.push({ person_id: p.id, date: `2026-10-${String(int(5, 12)).padStart(2, "0")}`, time_in: `${String(h).padStart(2, "0")}:${String(int(0, 59)).padStart(2, "0")}`, time_out: `${String(h + 1).padStart(2, "0")}:${String(int(0, 59)).padStart(2, "0")}` });
    });

    return { people, cars, reports: shuffled, interviews, visits: G.shuffle(visits), thief, w1, w2, realId, color, pre, reg, seed };
  }

  function buildDb(SQL, d) {
    db = new SQL.Database();
    db.run(`CREATE TABLE people (id INTEGER PRIMARY KEY, name TEXT, job TEXT, street TEXT, house INTEGER, car_id INTEGER);
            CREATE TABLE cars (id INTEGER PRIMARY KEY, plate TEXT, color TEXT, model TEXT);
            CREATE TABLE crime_reports (id INTEGER PRIMARY KEY, date TEXT, district TEXT, type TEXT, description TEXT);
            CREATE TABLE interviews (person_id INTEGER, transcript TEXT);
            CREATE TABLE museum_visits (person_id INTEGER, date TEXT, time_in TEXT, time_out TEXT);`);
    const ins = (sql, rows, f) => { const st = db.prepare(sql); rows.forEach(r => st.run(f(r))); st.free(); };
    ins("INSERT INTO people VALUES (?,?,?,?,?,?)", d.people, p => [p.id, p.name, p.job, p.street, p.house, p.car_id]);
    ins("INSERT INTO cars VALUES (?,?,?,?)", d.cars, c => [c.id, c.plate, c.color, c.model]);
    ins("INSERT INTO crime_reports VALUES (?,?,?,?,?)", d.reports, r => [r.id, r.date, r.district, r.type, r.description]);
    ins("INSERT INTO interviews VALUES (?,?)", d.interviews, r => [r.person_id, r.transcript]);
    ins("INSERT INTO museum_visits VALUES (?,?,?,?)", d.visits, r => [r.person_id, r.date, r.time_in, r.time_out]);
  }

  /* ---------- UI ---------- */
  const SCHEMA = {
    people: ["id", "name", "job", "street", "house", "car_id"],
    cars: ["id", "plate", "color", "model"],
    crime_reports: ["id", "date", "district", "type", "description"],
    interviews: ["person_id", "transcript"],
    museum_visits: ["person_id", "date", "time_in", "time_out"],
  };
  function renderSchema() {
    $("[data-sd-schema]").innerHTML = Object.entries(SCHEMA).map(([t, cols]) =>
      `<button type="button" class="sd-table" data-table="${t}"><b>${t}</b><small>${cols.join(", ")}</small></button>`).join("");
  }
  function renderSteps() {
    const facts = [
      found[0] ? `#${data.realId}` : "",
      found[1] ? `${data.w1.name} · ${data.w2.name}` : "",
      found[2] ? `${data.color}, «${data.pre}…», ${data.reg} · 21:00–22:30` : "",
      found[3] ? "✓" : "",
    ];
    $("[data-sd-steps]").innerHTML = T.steps.map((s, i) => `<li class="${found[i] ? "done" : ""}"><span class="sd-check">${found[i] ? "✓" : i + 1}</span><div><span>${G.esc(s)}</span>${facts[i] ? `<small>${G.esc(facts[i])}</small>` : ""}</div></li>`).join("");
  }

  function run() {
    if (!db) return;
    const sql = $("[data-sd-sql]").value.trim(); if (!sql) return;
    queries++;
    const out = $("[data-sd-out]");
    try {
      const res = db.exec(sql);
      if (!res.length) { out.innerHTML = `<p class="sd-msg">${T.empty}</p>`; return; }
      const r = res[res.length - 1];
      const rows = r.values.slice(0, 100);
      out.innerHTML = `<div class="sd-meta">${r.values.length} ${T.rows}</div><div class="sd-table-wrap"><table class="sd-result"><thead><tr>${r.columns.map(c => `<th>${G.esc(c)}</th>`).join("")}</tr></thead><tbody>${rows.map(v => `<tr>${v.map(x => `<td>${G.esc(x)}</td>`).join("")}</tr>`).join("")}</tbody></table></div>`;
      detect(JSON.stringify(r.values));
      G.sfx.click();
    } catch (e) {
      out.innerHTML = `<p class="sd-msg sd-err"><b>${T.error}:</b> ${G.esc(e.message)}</p>`; G.sfx.bad();
    }
  }

  // Auto-mark clues the student has uncovered
  let sawW1 = false, sawW2 = false, sawT1 = false, sawT2 = false;
  function detect(txt) {
    const before = found.slice();
    if (txt.includes("Золотая сова")) found[0] = true;
    if (txt.includes(data.w1.name)) sawW1 = true;
    if (txt.includes(data.w2.name)) sawW2 = true;
    if (sawW1 && sawW2) found[1] = true;
    if (txt.includes("начинался на")) sawT1 = true;
    if (txt.includes("журналу визитов")) sawT2 = true;
    if (sawT1 && sawT2) found[2] = true;
    if (txt.includes(data.thief.name) && found[2]) found[3] = true;
    if (found.some((f, i) => f && !before[i])) { renderSteps(); G.sfx.ok(); G.owl.good(); }
  }

  const HINTS = () => [
    "SELECT * FROM crime_reports\nWHERE date = '2026-10-10' AND district = 'Центр';",
    `SELECT * FROM people\nWHERE (street = '${data.w1.street}'\n       AND house = (SELECT MAX(house) FROM people WHERE street = '${data.w1.street}'))\n   OR (street = '${data.w2.street}' AND name LIKE '${data.w2.name.split(" ")[0]} %');`,
    "SELECT p.name, i.transcript\nFROM interviews i JOIN people p ON p.id = i.person_id\nWHERE p.id IN (" + data.w1.id + ", " + data.w2.id + ");",
    `SELECT p.name, c.plate, v.date, v.time_in, v.time_out\nFROM people p\nJOIN cars c ON c.id = p.car_id\nJOIN museum_visits v ON v.person_id = p.id\nWHERE c.color = '${data.color}' AND c.plate LIKE '${data.pre}%' AND v.date = '2026-10-10';`,
  ];
  function hint() {
    const step = found.findIndex(f => !f); if (step < 0) return;
    hints++;
    $("[data-sd-sql]").value = `-- ${T.hintPrefix}\n` + HINTS()[step];
    G.owl.think();
  }

  function accuse() {
    if (solved) return;
    const name = $("[data-sd-name]").value.trim().toLowerCase().replace(/\s+/g, " ");
    if (!name) return;
    const who = data.people.find(p => p.name.toLowerCase() === name);
    if (who && who.id === data.thief.id) {
      solved = true;
      const base = Math.max(60, 200 - hints * 35 - wrong * 30);
      G.patch(s => { s.best.sqldet = Math.max(s.best.sqldet || 0, base); });
      G.finish("sqldet", {
        win: true, score: base, base, perfect: hints === 0 && wrong === 0,
        stars: hints === 0 && wrong === 0 ? 3 : hints <= 1 && wrong <= 1 ? 2 : 1,
        title: T.solved, text: T.solvedText.replace("{n}", data.thief.name),
        stats: [{ label: T.queries, value: queries }, { label: T.hints, value: hints }, { label: T.mistakes, value: wrong }],
      });
    } else {
      wrong++; G.sfx.bad(); G.owl.bad(T.wrong.replace("{n}", who ? who.name : $("[data-sd-name]").value));
      G.shake($(".sd-accuse"));
    }
  }

  let SQLlib = null;
  function loadEngine() {
    return new Promise((ok, fail) => {
      if (SQLlib) return ok(SQLlib);
      const s = document.createElement("script"); s.src = "/lib/sqljs/sql-wasm.js";
      s.onload = () => window.initSqlJs({ locateFile: f => "/lib/sqljs/" + f }).then(lib => { SQLlib = lib; ok(lib); }, fail);
      s.onerror = fail; document.head.appendChild(s);
    });
  }

  async function start() {
    found = [false, false, false, false]; sawW1 = sawW2 = sawT1 = sawT2 = false; queries = 0; hints = 0; wrong = 0; solved = false;
    const seed = (Date.now() ^ Math.floor(Math.random() * 1e9)) >>> 0;
    data = generate(seed);
    $("[data-sd-case]").textContent = "№" + String(seed % 100000).padStart(5, "0");
    renderSchema(); renderSteps();
    $("[data-sd-sql]").value = "-- " + (G.LANG === "en" ? "Start here:" : G.LANG === "kk" ? "Осыдан бастаңыз:" : "Начните отсюда:") + "\nSELECT * FROM crime_reports\nLIMIT 5;";
    $("[data-sd-name]").value = "";
    try {
      const SQL = await loadEngine(); buildDb(SQL, data);
      $("[data-sd-out]").innerHTML = "";
      run(); queries = 0;
    } catch { $("[data-sd-out]").innerHTML = `<p class="sd-msg sd-err">${T.noDb}</p>`; }
  }

  document.addEventListener("click", e => {
    if (e.target.closest("[data-sd-run]")) run();
    if (e.target.closest("[data-sd-hint]")) hint();
    if (e.target.closest("[data-sd-accuse]")) accuse();
    const t = e.target.closest("[data-table]"); if (t) { $("[data-sd-sql]").value = `SELECT * FROM ${t.dataset.table}\nLIMIT 10;`; run(); }
  });
  document.addEventListener("keydown", e => {
    if (e.target.matches("[data-sd-sql]") && (e.ctrlKey || e.metaKey) && e.key === "Enter") { e.preventDefault(); run(); }
    if (e.target.matches("[data-sd-name]") && e.key === "Enter") accuse();
    // Tab inserts spaces inside the editor
    if (e.target.matches("[data-sd-sql]") && e.key === "Tab") { e.preventDefault(); const t = e.target, p = t.selectionStart; t.value = t.value.slice(0, p) + "  " + t.value.slice(t.selectionEnd); t.selectionStart = t.selectionEnd = p + 2; }
  });
  G.intro(start);
})();
