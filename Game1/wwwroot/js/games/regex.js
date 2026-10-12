/* Regex ninja — regex golf with 10 levels, live matching and stars for short expressions. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.RX_T, $ = (s) => document.querySelector(s);

  const LEVELS = [
    { t: { ru: "Только цифры", kk: "Тек сандар", en: "Digits only" }, d: { ru: "Строка целиком из цифр. Подсказка: якоря ^ и $.", kk: "Жол толығымен сандардан. Кеңес: ^ және $ зәкірлері.", en: "The whole string is digits. Tip: anchors ^ and $." },
      left: ["42", "2026", "7"], right: ["abc", "x1y", "12a", "a12"], sol: "^\\d+$" },
    { t: { ru: "Группы потока", kk: "Ағын топтары", en: "Cohort groups" }, d: { ru: "Группы начинаются с «IT-» заглавными буквами.", kk: "Топтар бас әріппен «IT-» деп басталады.", en: "Groups start with “IT-” in capitals." },
      left: ["IT-2101", "IT-2102", "IT-99"], right: ["CS-2101", "it-2101", "IT2101", "XIT-1"], sol: "^IT-" },
    { t: { ru: "PHP-файлы", kk: "PHP файлдары", en: "PHP files" }, d: { ru: "Имя файла заканчивается на .php — точку нужно экранировать.", kk: "Файл аты .php-мен аяқталады — нүктені экрандау керек.", en: "The file name ends with .php — escape the dot." },
      left: ["index.php", "UserController.php", "a.php"], right: ["php.ini", "style.css", "app.phpx", "indexphp"], sol: "\\.php$" },
    { t: { ru: "Ошибки клиента 4xx", kk: "Клиент қателері 4xx", en: "Client errors 4xx" }, d: { ru: "Ровно три цифры, первая — 4.", kk: "Дәл үш сан, біріншісі — 4.", en: "Exactly three digits starting with 4." },
      left: ["400", "404", "422", "429"], right: ["200", "500", "4040", "40", "a404"], sol: "^4\\d\\d$" },
    { t: { ru: "Двойные буквы", kk: "Қос әріптер", en: "Double letters" }, d: { ru: "В слове есть две одинаковые буквы подряд. Нужна обратная ссылка \\1.", kk: "Сөзде қатар екі бірдей әріп бар. \\1 кері сілтемесі керек.", en: "Two identical letters in a row. You'll need a backreference \\1." },
      left: ["class", "commit", "null", "apple"], right: ["stack", "query", "index", "fetch"], sol: "(.)\\1" },
    { t: { ru: "Email", kk: "Email", en: "Email" }, d: { ru: "Есть имя, @, домен и точка с зоной.", kk: "Аты, @, домен және нүктелі аймақ бар.", en: "A name, @, a domain and a dot with a zone." },
      left: ["akerke@edu.kz", "d.serikov@mail.ru", "a_kim@uni.edu.kz"], right: ["akerke@", "@edu.kz", "akerke.edu.kz", "a@b"], sol: "^\\S+@\\S+\\.\\w+$" },
    { t: { ru: "Переменные PHP", kk: "PHP айнымалылары", en: "PHP variables" }, d: { ru: "$, затем буква или _, дальше буквы, цифры и _.", kk: "$, содан кейін әріп немесе _, одан әрі әріптер, сандар және _.", en: "$, then a letter or _, then letters, digits and _." },
      left: ["$name", "$user_id", "$x1", "$_tmp"], right: ["name", "$1abc", "$-x", "$ a"], sol: "^\\$[a-z_]\\w*$" },
    { t: { ru: "Hex-цвет", kk: "Hex-түс", en: "Hex color" }, d: { ru: "# и 3 или 6 шестнадцатеричных символов.", kk: "# және 3 не 6 он алтылық таңба.", en: "# followed by 3 or 6 hex digits." },
      left: ["#7C3AED", "#fff", "#0E9FB0", "#abc"], right: ["7C3AED", "#ggg", "#12345", "#1234567"], sol: "^#([\\da-fA-F]{3}){1,2}$" },
    { t: { ru: "Время ЧЧ:ММ", kk: "Уақыт СС:ММ", en: "Time HH:MM" }, d: { ru: "От 00:00 до 23:59.", kk: "00:00-ден 23:59-ға дейін.", en: "From 00:00 to 23:59." },
      left: ["09:30", "23:59", "00:00", "18:05"], right: ["24:00", "9:30", "12:60", "12-30"], sol: "^([01]\\d|2[0-3]):[0-5]\\d$" },
    { t: { ru: "Телефон KZ", kk: "KZ телефоны", en: "KZ phone" }, d: { ru: "Формат +7 XXX XXX XX XX.", kk: "+7 XXX XXX XX XX форматы.", en: "Format +7 XXX XXX XX XX." },
      left: ["+7 701 123 45 67", "+7 777 000 11 22"], right: ["8 701 123 45 67", "+7 70 123 45 67", "+7 701 1234567", "+77011234567"], sol: "^\\+7( \\d{3}){2}( \\d\\d){2}$" },
  ];

  let lvl, solvedNow, givenUp;
  const stars = () => G.getStars("regex");

  function start() {
    const st = stars();
    // continue from the first level without stars
    lvl = LEVELS.findIndex((_, i) => !st[i]); if (lvl < 0) lvl = 0;
    load();
  }

  const seen = () => G.state().rxSeen || {};
  function unlocked(i) { const st = stars(), sn = seen(); return i === 0 || !!st[i - 1] || !!sn[i - 1] || !!st[i]; }

  function renderLevels() {
    const st = stars();
    $("[data-rx-levels]").innerHTML = LEVELS.map((L, i) => `<button type="button" class="rx-lvl ${i === lvl ? "active" : ""} ${st[i] ? "solved" : ""}" data-lvl="${i}" ${unlocked(i) ? "" : "disabled"} title="${G.esc(G.tr(L.t))}">
        <b>${i + 1}</b><small>${st[i] ? "★".repeat(st[i]) : unlocked(i) ? "·" : "🔒"}</small></button>`).join("");
  }

  function load() {
    const L = LEVELS[lvl]; solvedNow = false; givenUp = false;
    renderLevels();
    G.hud.progress(lvl + 1, LEVELS.length);
    $("[data-rx-title]").textContent = `${T.level} ${lvl + 1}. ${G.tr(L.t)}`;
    $("[data-rx-desc]").textContent = G.tr(L.d);
    $("[data-rx-par]").textContent = T.par.replace("{n}", L.sol.length);
    $("[data-rx-done]").hidden = true; $("[data-rx-next]").hidden = true; $("[data-rx-give]").disabled = false;
    const inp = $("[data-rx-input]"); inp.value = ""; inp.disabled = false; inp.focus();
    test();
  }

  function test() {
    const L = LEVELS[lvl], src = $("[data-rx-input]").value;
    $("[data-rx-len]").textContent = src.length;
    let re = null, err = "";
    if (src) { try { re = new RegExp(src); } catch (e) { err = e.message; } }
    $("[data-rx-error]").hidden = !err; $("[data-rx-error]").textContent = err;
    const row = (w, should) => {
      const m = re ? re.test(w) : false;
      const ok = re ? m === should : false;
      let shown = G.esc(w);
      if (re && m) { const mm = w.match(re); if (mm && mm[0]) shown = G.esc(w.slice(0, mm.index)) + `<mark>${G.esc(mm[0])}</mark>` + G.esc(w.slice(mm.index + mm[0].length)); }
      return `<li class="${re ? (ok ? "ok" : "no") : ""}"><span class="rx-ico">${re ? (ok ? "✓" : "✗") : "·"}</span><code>${shown}</code></li>`;
    };
    $("[data-rx-left]").innerHTML = L.left.map(w => row(w, true)).join("");
    $("[data-rx-right]").innerHTML = L.right.map(w => row(w, false)).join("");
    const all = re && L.left.every(w => re.test(w)) && L.right.every(w => !re.test(w));
    if (all && !solvedNow && !givenUp) win(src);
  }

  function win(src) {
    solvedNow = true;
    const L = LEVELS[lvl], par = L.sol.length;
    const s = src.length <= par ? 3 : src.length <= par * 1.5 ? 2 : 1;
    G.setStars("regex", lvl, s);
    renderLevels();
    $("[data-rx-input]").disabled = true;
    const e = $("[data-rx-done]"); e.hidden = false;
    e.innerHTML = `<b>${T.solved.replace("{n}", src.length).replace("{s}", "★".repeat(s))}</b>`;
    G.sfx.win(); G.owl.good();
    if (lvl < LEVELS.length - 1) $("[data-rx-next]").hidden = false;
    const total = Object.values(stars()).reduce((x, y) => x + y, 0), solved = Object.keys(stars()).length;
    G.finish("regex", {
      win: true, score: total * 10, base: 10 + s * 5, perfect: s === 3, stars: s,
      title: lvl === LEVELS.length - 1 ? T.allDone : `${T.level} ${lvl + 1} ✓`, text: `/${src}/`,
      stats: [{ label: T.solvedCount, value: `${solved}/${LEVELS.length}` }, { label: T.stars, value: `★ ${total}` }],
    });
  }

  function giveUp() {
    givenUp = true; const L = LEVELS[lvl];
    $("[data-rx-input]").value = L.sol; test();
    const e = $("[data-rx-done]"); e.hidden = false; e.innerHTML = T.given.replace("{r}", `<code>/${G.esc(L.sol)}/</code>`);
    $("[data-rx-give]").disabled = true; $("[data-rx-input]").disabled = true;
    G.patch(s => { s.rxSeen ??= {}; s.rxSeen[lvl] = 1; }); renderLevels();
    if (lvl < LEVELS.length - 1) $("[data-rx-next]").hidden = false;
    G.owl.think();
  }

  document.addEventListener("input", e => { if (e.target.matches("[data-rx-input]")) test(); });
  document.addEventListener("click", e => {
    const b = e.target.closest("[data-lvl]"); if (b && !b.disabled) { lvl = +b.dataset.lvl; load(); }
    if (e.target.closest("[data-rx-next]")) { lvl = Math.min(LEVELS.length - 1, lvl + 1); load(); }
    if (e.target.closest("[data-rx-give]")) giveUp();
  });
  G.intro(start);
  // "Play again" from the result dialog continues to the next level
  G.onAgain(() => { $("[data-game-intro]").hidden = true; $("[data-game-stage]").hidden = false; if (solvedNow && lvl < LEVELS.length - 1) lvl++; load(); });
})();
