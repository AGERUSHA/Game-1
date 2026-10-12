/* ==========================================================================
   EDU AI — game engine shared by every game in /Games
   - XP economy: daily cap, perfect / first-win / game-of-the-day bonuses, repeat decay
   - HUD (score, combo, lives, timer, progress), countdown timer
   - sound effects (Web Audio, off by default), owl reactions, floating points
   - tiny syntax highlighter for code snippets
   - shared result dialog (#gameResult)
   Public API: window.EduGames
   ========================================================================== */
(function () {
  "use strict";
  const LANG = document.documentElement.lang || "ru";
  const T = window.GAME_T || {};
  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));

  /** Picks the current language from { ru, kk, en } (falls back to ru). */
  const tr = (o) => o == null ? "" : typeof o === "string" ? o : (o[LANG] ?? o.ru ?? o.en ?? "");
  const esc = (s) => String(s ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

  /* ---------- Randomness ---------- */
  const rnd = (n) => Math.floor(Math.random() * n);
  const shuffle = (a) => { a = a.slice(); for (let i = a.length - 1; i > 0; i--) { const j = rnd(i + 1); [a[i], a[j]] = [a[j], a[i]]; } return a; };
  const sample = (a, n) => shuffle(a).slice(0, n);
  /** Deterministic generator — the same "daily" content for everyone. */
  function seeded(seed) { let s = (seed >>> 0) || 1; return () => { s = (Math.imul(s, 1664525) + 1013904223) >>> 0; return s / 4294967296; }; }
  const todayKey = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`; };
  const dayNumber = () => Math.floor((Date.parse(todayKey()) - Date.parse("2026-01-01")) / 86400000);

  /* ---------- Progress & XP economy (per browser until the backend stores it) ---------- */
  const KEY = "edu_games_v1", CAP = 300;
  function load() {
    let s; try { s = JSON.parse(localStorage.getItem(KEY)) || {}; } catch { s = {}; }
    s.best ??= {}; s.stars ??= {}; s.totals ??= {}; s.kodl ??= {}; s.games ??= 0;
    if (s.day !== todayKey()) { s.day = todayKey(); s.xpToday = 0; s.plays = {}; s.wins = {}; }
    return s;
  }
  const save = (s) => { try { localStorage.setItem(KEY, JSON.stringify(s)); } catch { } };

  /**
   * Computes and records XP for a finished round.
   * base — points earned in the round; perfect — no mistakes / hints; win — the round was won.
   */
  function award(id, base, { perfect = false, win = true } = {}) {
    const s = load();
    const lines = [{ label: T.xpBase, value: base }];
    let total = base;
    if (perfect && base > 0) { const b = Math.round(base * .5); total += b; lines.push({ label: T.xpPerfect, value: b }); }
    if (win && base > 0 && !s.wins[id]) { total += 20; lines.push({ label: T.xpFirstWin, value: 20 }); }
    if (window.GAME_OF_DAY && total > 0) { lines.push({ label: T.xpGameOfDay, value: total }); total *= 2; }
    const plays = (s.plays[id] || 0) + 1;
    const factor = plays <= 3 ? 1 : plays <= 5 ? .5 : .2;           // repeats count less — learn, don't farm
    if (factor < 1 && total > 0) { const cut = total - Math.round(total * factor); total -= cut; lines.push({ label: (T.xpRepeat || "").replace("{p}", Math.round(factor * 100)), value: -cut }); }
    const room = Math.max(0, CAP - s.xpToday);
    let capped = false;
    if (total > room) { lines.push({ label: T.xpCap, value: room - total }); total = room; capped = true; }
    s.plays[id] = plays; if (win) s.wins[id] = true;
    s.xpToday += total; s.totals[id] = (s.totals[id] || 0) + total; s.games += 1;
    save(s);
    return { total, lines, capped, xpToday: s.xpToday, cap: CAP };
  }
  function setBest(id, score) { const s = load(); if (score > (s.best[id] || 0)) { s.best[id] = score; save(s); return true; } return false; }
  function setStars(id, level, stars) { const s = load(); s.stars[id] ??= {}; if ((s.stars[id][level] || 0) < stars) { s.stars[id][level] = stars; save(s); } }
  function getStars(id) { return load().stars[id] || {}; }
  function patch(fn) { const s = load(); fn(s); save(s); return s; }

  /* ---------- Sound (Web Audio, off by default) ---------- */
  const SOUND_KEY = "edu_games_sound";
  let soundOn = (() => { try { return localStorage.getItem(SOUND_KEY) === "1"; } catch { return false; } })();
  let actx;
  function tone(freqs, dur = .12, type = "sine", vol = .07) {
    if (!soundOn) return;
    try {
      actx ??= new (window.AudioContext || window.webkitAudioContext)();
      let t = actx.currentTime;
      freqs.forEach(f => {
        const o = actx.createOscillator(), g = actx.createGain();
        o.type = type; o.frequency.value = f;
        g.gain.setValueAtTime(vol, t); g.gain.exponentialRampToValueAtTime(.0001, t + dur);
        o.connect(g).connect(actx.destination); o.start(t); o.stop(t + dur); t += dur * .75;
      });
    } catch { }
  }
  const sfx = {
    ok: () => tone([660, 880]), bad: () => tone([220, 150], .16, "triangle"), combo: () => tone([660, 880, 1175], .08),
    win: () => tone([523, 659, 784, 1047], .15), lose: () => tone([392, 330, 262], .18, "triangle"), click: () => tone([900], .03, "square", .02),
  };
  function renderSound() {
    $$("[data-game-sound]").forEach(b => {
      b.innerHTML = `<i data-lucide="${soundOn ? "volume-2" : "volume-x"}" class="icon"></i>`;
      b.setAttribute("aria-pressed", soundOn);
    });
    window.eduIcons?.();
  }
  document.addEventListener("click", e => {
    if (!e.target.closest("[data-game-sound]")) return;
    soundOn = !soundOn; try { localStorage.setItem(SOUND_KEY, soundOn ? "1" : "0"); } catch { }
    renderSound(); sfx.ok();
  });

  /* ---------- HUD ---------- */
  const hudEl = (k) => $(`[data-hud="${k}"]`);
  const hud = {
    score(v) { const el = hudEl("score"); if (!el) return; el.hidden = false; const b = el.querySelector("b"); b.textContent = v; b.classList.remove("pop"); void b.offsetWidth; b.classList.add("pop"); },
    combo(v) { const el = hudEl("combo"); if (!el) return; el.hidden = v < 2; el.querySelector("b").textContent = "×" + v; el.classList.toggle("hot", v >= 4); },
    lives(n, max) { const el = hudEl("lives"); if (!el) return; el.hidden = false; el.innerHTML = Array.from({ length: max }, (_, i) => `<span class="heart ${i < n ? "" : "lost"}">♥</span>`).join(""); },
    timer(left, total) {
      const el = hudEl("timer"); if (!el) return; el.hidden = false;
      el.querySelector("b").textContent = left >= 60 ? `${Math.floor(left / 60)}:${String(left % 60).padStart(2, "0")}` : left;
      el.classList.toggle("urgent", left <= Math.min(10, Math.ceil(total * .25)));
      el.style.setProperty("--t", Math.max(0, left / total * 100) + "%");
    },
    progress(i, n) { const el = hudEl("progress"); if (!el) return; el.hidden = false; el.querySelector("b").textContent = `${Math.min(i, n)}/${n}`; },
    hideAll() { $$("[data-hud]").forEach(e => e.hidden = true); },
  };

  /** Countdown that survives pauses: timer(seconds, onTick(left), onEnd()). */
  function timer(seconds, onTick, onEnd) {
    let left = seconds, id = null, paused = false;
    const t = {
      start() { t.stop(); onTick?.(left, seconds); id = setInterval(() => { if (paused) return; left--; onTick?.(left, seconds); if (left <= 0) { t.stop(); onEnd?.(); } }, 1000); return t; },
      stop() { clearInterval(id); id = null; },
      reset(s) { left = seconds = s ?? seconds; },
      add(s) { left = Math.max(0, left + s); onTick?.(left, seconds); if (left <= 0 && id) { t.stop(); onEnd?.(); } },
      pause(p) { paused = p; },
      get left() { return left; },
    };
    return t;
  }

  /* ---------- Owl & feedback ---------- */
  const owlEl = () => $("[data-game-owl]");
  const pick = (a) => a?.length ? a[rnd(a.length)] : "";
  const owl = {
    say(text, ms = 2600) { window.eduOwl?.say(owlEl(), text, ms); },
    good(text) { const o = owlEl(); if (!o) return; o.classList.remove("is-sad"); window.eduOwl?.mood(o, "is-cheer", 1100); if (text !== false) owl.say(text || pick(T.cheers)); },
    bad(text) { const o = owlEl(); if (!o) return; o.classList.remove("is-cheer"); window.eduOwl?.mood(o, "is-sad", 1600); if (text !== false) owl.say(text || pick(T.oops)); },
    think() { const o = owlEl(); if (o) window.eduOwl?.mood(o, "is-thinking", 1500); },
  };

  /** "+10" flying up from an element. */
  function floatText(el, text, cls = "") {
    if (!el) return;
    const r = el.getBoundingClientRect(), f = document.createElement("span");
    f.className = "game-float " + cls; f.textContent = text;
    f.style.left = (r.left + r.width / 2) + "px"; f.style.top = (r.top + 4) + "px";
    document.body.appendChild(f); setTimeout(() => f.remove(), 900);
  }
  /** Shake an element (wrong answer). */
  function shake(el) { if (!el) return; el.classList.remove("shake"); void el.offsetWidth; el.classList.add("shake"); }

  /* ---------- Code highlighter (php, js, python, sql) ---------- */
  const KW = {
    php: "function|return|if|else|elseif|foreach|for|while|as|echo|new|class|public|private|protected|static|use|namespace|null|true|false|array|fn|match|throw|try|catch|isset|count|strlen",
    js: "function|return|if|else|for|while|of|in|const|let|var|new|class|this|null|undefined|true|false|await|async|typeof|console|=>",
    python: "def|return|if|elif|else|for|while|in|not|and|or|import|from|as|class|None|True|False|print|len|range|lambda|with|yield",
    sql: "SELECT|FROM|WHERE|JOIN|LEFT|RIGHT|INNER|ON|GROUP|BY|ORDER|HAVING|LIMIT|INSERT|INTO|VALUES|UPDATE|SET|DELETE|AND|OR|NOT|NULL|IS|AS|COUNT|SUM|AVG|MAX|MIN|DISTINCT|LIKE|IN|DESC|ASC",
  };
  function highlight(code, lang = "js") {
    const kw = KW[lang] || KW.js;
    const comment = { sql: String.raw`--[^\n]*`, python: String.raw`#[^\n]*`, php: String.raw`\/\/[^\n]*|#[^\n]*` }[lang] || String.raw`\/\/[^\n]*`;
    const re = new RegExp(String.raw`(${comment})|("(?:\\.|[^"\\\n])*"|'(?:\\.|[^'\\\n])*'|\x60(?:\\.|[^\x60\\])*\x60)|(\$[A-Za-z_]\w*)|\b(\d+(?:\.\d+)?)\b|(?:^|(?<=[^\w$]))(${kw})(?![\w$])`, lang === "sql" ? "gi" : "g");
    let out = "", last = 0, m;
    while ((m = re.exec(code))) {
      if (!m[0]) { re.lastIndex++; continue; }
      out += esc(code.slice(last, m.index));
      const [tok, com, str, vr, num, k] = m;
      if (com) out += `<span class="tk-com">${esc(tok)}</span>`;
      else if (str) out += `<span class="tk-str">${esc(tok)}</span>`;
      else if (vr) out += `<span class="tk-var">${esc(tok)}</span>`;
      else if (num) out += `<span class="tk-num">${esc(tok)}</span>`;
      else if (k) out += `<span class="tk-kw">${esc(tok)}</span>`;
      else out += esc(tok);
      last = m.index + tok.length;
    }
    return out + esc(code.slice(last));
  }

  /* ---------- Result dialog ---------- */
  let againFn = null;
  function finish(id, o) {
    const res = award(id, Math.max(0, Math.round(o.base || 0)), { perfect: !!o.perfect, win: o.win !== false });
    const record = o.score != null && setBest(id, o.score);
    const m = $("#gameResult"); if (!m) return res;
    const stars = Math.max(0, Math.min(3, o.stars ?? 0));
    m.querySelector("[data-r-stars]").innerHTML = [0, 1, 2].map(i => `<span class="r-star ${i < stars ? "on" : ""}" style="--d:${i * 160}ms">★</span>`).join("");
    m.querySelector("[data-r-title]").textContent = o.title || (o.win === false ? T.lose : T.win);
    m.querySelector("[data-r-text]").textContent = o.text || "";
    m.querySelector("[data-r-record]").hidden = !record;
    m.querySelector("[data-r-stats]").innerHTML = (o.stats || []).map(s => `<div><small>${esc(s.label)}</small><b>${esc(s.value)}</b></div>`).join("");
    m.querySelector("[data-r-lines]").innerHTML = res.lines.filter(l => l.value !== 0 || l === res.lines[0])
      .map(l => `<li><span>${esc(l.label)}</span><b class="${l.value < 0 ? "neg" : ""}">${l.value > 0 ? "+" : ""}${l.value}</b></li>`).join("");
    m.querySelector("[data-r-xp]").textContent = "+" + res.total;
    m.querySelector("[data-r-cap]").hidden = !res.capped;
    m.querySelector("[data-r-today]").textContent = `${res.xpToday} / ${res.cap}`;
    m.querySelector("[data-r-todaybar]").style.width = Math.min(100, res.xpToday / res.cap * 100) + "%";
    const review = m.querySelector("[data-r-review]");
    review.hidden = !o.review?.length;
    review.querySelector("ul").innerHTML = (o.review || []).map(r => `<li>${r}</li>`).join("");
    m.classList.add("open");
    window.eduIcons?.();
    if (o.win !== false) { sfx.win(); owl.good(false); if (res.total > 0) setTimeout(() => window.eduCelebrate?.({ xp: res.total }), 250); }
    else { sfx.lose(); owl.bad(false); }
    return res;
  }
  document.addEventListener("click", e => {
    if (e.target.closest("[data-game-again]")) { $("#gameResult")?.classList.remove("open"); againFn?.(); }
  });

  /* ---------- Option chips on the intro screen ---------- */
  document.addEventListener("click", e => {
    const b = e.target.closest("[data-opt]"); if (!b) return;
    const g = b.closest("[data-opt-group]");
    $$("[data-opt]", g).forEach(x => { x.classList.toggle("active", x === b); x.setAttribute("aria-checked", x === b); });
    sfx.click();
  });
  const option = (group) => $(`[data-opt-group="${group}"] [data-opt].active`)?.dataset.opt;

  /* ---------- Intro screen helper: shows the stage when "Start" is pressed ---------- */
  function intro(onStart) {
    const start = $("[data-game-start]");
    start?.addEventListener("click", () => { $("[data-game-intro]").hidden = true; $("[data-game-stage]").hidden = false; onStart(); });
    againFn = () => { $("[data-game-intro]").hidden = true; $("[data-game-stage]").hidden = false; onStart(); };
  }
  function toIntro() { $("[data-game-intro]").hidden = false; $("[data-game-stage]").hidden = true; hud.hideAll(); }

  document.addEventListener("DOMContentLoaded", renderSound);

  window.EduGames = {
    LANG, T, tr, esc, rnd, shuffle, sample, seeded, pick, todayKey, dayNumber,
    award, setBest, setStars, getStars, state: load, patch, sfx, hud, timer, owl, floatText, shake, highlight,
    finish, intro, toIntro, option, onAgain: (fn) => { againFn = fn; },
  };
})();
