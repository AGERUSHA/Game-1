/* ==========================================================================
   EDU AI — gamification widgets (prototype, state kept in localStorage)
   - eduCelebrate(): confetti + floating "+XP"; any [data-xp] element triggers it
   - learning streak (topbar flame): goes out when a day is missed
   - Pomodoro focus mode (topbar timer): owl wears headphones, distractions hidden
   - floating owl buddy: greetings, tips, sleeps in the night theme
   - secret "Owl Friend" achievement card
   ========================================================================== */
(function () {
  "use strict";

  const T = window.EDU_T || {};
  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));
  const reduce = matchMedia("(prefers-reduced-motion: reduce)");
  const owl = () => window.eduOwl;
  const store = {
    get(k) { try { return JSON.parse(localStorage.getItem(k)); } catch { return null; } },
    set(k, v) { try { localStorage.setItem(k, JSON.stringify(v)); } catch { } }
  };
  const fill = (s, vars) => String(s || "").replace(/\{(\w+)\}/g, (_, k) => vars?.[k] ?? "");
  const pick = (a) => Array.isArray(a) ? a[Math.floor(Math.random() * a.length)] : a;

  /* ======================= Confetti + floating XP ======================= */
  const COLORS = ["#7C3AED", "#A78BFA", "#F472B6", "#FBBF24", "#22B8CF", "#4ADE80"];
  let canvas, cctx, parts = [], running = false;

  function confetti(x, y, count) {
    if (reduce.matches) return;
    if (!canvas) {
      canvas = document.createElement("canvas");
      canvas.className = "confetti-canvas";
      canvas.setAttribute("aria-hidden", "true");
      document.body.appendChild(canvas);
      cctx = canvas.getContext("2d");
    }
    const dpr = Math.min(2, devicePixelRatio || 1);
    canvas.width = innerWidth * dpr; canvas.height = innerHeight * dpr;
    cctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    for (let i = 0; i < count; i++) {
      const a = -Math.PI / 2 + (Math.random() - .5) * Math.PI * 1.1, v = 6 + Math.random() * 8;
      parts.push({ x, y, vx: Math.cos(a) * v, vy: Math.sin(a) * v, r: Math.random() * Math.PI, vr: (Math.random() - .5) * .3,
        w: 6 + Math.random() * 6, h: 4 + Math.random() * 5, c: COLORS[i % COLORS.length], life: 0, round: Math.random() < .3 });
    }
    if (!running) { running = true; requestAnimationFrame(step); }
  }
  function step() {
    cctx.clearRect(0, 0, innerWidth, innerHeight);
    parts = parts.filter(p => p.life < 140 && p.y < innerHeight + 30);
    parts.forEach(p => {
      p.life++; p.vy += .28; p.vx *= .985; p.x += p.vx; p.y += p.vy; p.r += p.vr;
      cctx.save(); cctx.translate(p.x, p.y); cctx.rotate(p.r);
      cctx.globalAlpha = Math.min(1, (140 - p.life) / 40);
      cctx.fillStyle = p.c;
      if (p.round) { cctx.beginPath(); cctx.arc(0, 0, p.h / 2, 0, Math.PI * 2); cctx.fill(); }
      else cctx.fillRect(-p.w / 2, -p.h / 2, p.w, p.h);
      cctx.restore();
    });
    if (parts.length) requestAnimationFrame(step);
    else { running = false; cctx.clearRect(0, 0, innerWidth, innerHeight); }
  }

  function xpFloat(xp, x, y) {
    const el = document.createElement("div");
    el.className = "xp-float";
    el.textContent = `+${xp} XP`;
    el.style.left = x + "px"; el.style.top = y + "px";
    document.body.appendChild(el);
    el.addEventListener("animationend", () => el.remove());
    setTimeout(() => el.remove(), 2000);
  }

  // { xp, x, y, big, quiet } — quiet: the owl stays silent (caller speaks)
  function celebrate(o = {}) {
    const x = o.x ?? innerWidth / 2, y = o.y ?? innerHeight / 2;
    if (o.xp) xpFloat(o.xp, x, y);
    confetti(x, y, o.big ? 140 : o.xp ? 60 : 40);
    const buddy = $("[data-owl-buddy]:not([hidden])");
    if (buddy) {
      buddy.classList.remove("is-sleep");
      owl()?.mood(buddy, "is-cheer", 1100);
      if (o.xp && !o.quiet) owl()?.say(buddy, fill(T.xp, { xp: o.xp }));
    }
  }
  window.eduCelebrate = celebrate;

  // Declarative trigger: <button data-xp="50">, links navigate after the animation
  document.addEventListener("click", e => {
    const el = e.target.closest("[data-xp]");
    if (!el || el.matches("input")) return;
    const r = el.getBoundingClientRect();
    celebrate({ xp: +el.dataset.xp, x: r.left + r.width / 2, y: r.top, big: el.hasAttribute("data-xp-big") });
    if (el.tagName === "A" && el.getAttribute("href") && !el.getAttribute("href").startsWith("#")) {
      e.preventDefault();
      setTimeout(() => location.href = el.href, reduce.matches ? 200 : 1100);
    }
  });
  document.addEventListener("change", e => {
    const el = e.target;
    if (!el.matches?.("input[type=checkbox][data-xp]")) return;
    if (!el.checked) return;
    const r = el.getBoundingClientRect();
    celebrate({ xp: +el.dataset.xp, x: r.left + r.width / 2, y: r.top });
  });

  /* ============================ Streak ============================ */
  const dayKey = (d) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
  const addDays = (d, n) => { const c = new Date(d); c.setDate(c.getDate() + n); return c; };

  function initStreak() {
    const btn = $("[data-streak]");
    if (!btn) return;
    const today = new Date(), tk = dayKey(today), yk = dayKey(addDays(today, -1));
    let st = store.get("edu_streak");
    let lost = 0;
    if (!st || typeof st.count !== "number") st = { count: +btn.dataset.streak || 1, last: tk };
    else if (st.last === yk) { st.count++; st.last = tk; }
    else if (st.last !== tk) { lost = st.count; st = { count: 1, last: tk, lostAt: tk, lost }; }
    if (st.lostAt === tk) lost = st.lost;
    store.set("edu_streak", st);

    $$("[data-streak-count]").forEach(el => el.textContent = st.count);
    document.documentElement.classList.toggle("streak-lost", !!lost);

    // Week strip (Mon..Sun): flames for the days inside the streak
    const monday = addDays(today, -((today.getDay() + 6) % 7));
    $$("[data-streak-day]").forEach((cell, i) => {
      const d = addDays(monday, i), diff = Math.round((today - d) / 864e5);
      cell.classList.toggle("on", diff >= 0 && diff < st.count);
      cell.classList.toggle("today", diff === 0);
      cell.classList.toggle("future", diff < 0);
    });
    const msg = $("[data-streak-msg]");
    if (msg) msg.textContent = lost ? fill(T.streakLost, { n: lost }) : fill(T.streakKeep, { n: st.count });

    return { st, lost };
  }

  /* ============================ Pomodoro ============================ */
  const POMO_KEY = "edu_pomo";
  // Durations can be switched by presets (data-pomo-preset="50/10"); kept in their own key
  const DUR_KEY = "edu_pomo_dur";
  const DUR = Object.assign({ focus: 25 * 60, break: 5 * 60 }, store.get(DUR_KEY) || {});
  let pomo = store.get(POMO_KEY) || { mode: "focus", end: null, left: DUR.focus };
  if (!DUR[pomo.mode]) pomo = { mode: "focus", end: null, left: DUR.focus };

  const pomoLeft = () => pomo.end ? Math.max(0, Math.round((pomo.end - Date.now()) / 1000)) : pomo.left;
  const fmt = (s) => String(Math.floor(s / 60)).padStart(2, "0") + ":" + String(s % 60).padStart(2, "0");

  function pomoRender() {
    const left = pomoLeft(), runningFocus = !!pomo.end && pomo.mode === "focus";
    $$("[data-pomo-time]").forEach(el => el.textContent = fmt(left));
    $$("[data-pomo-mini]").forEach(el => { el.textContent = pomo.end ? fmt(left) : ""; el.hidden = !pomo.end; });
    $$("[data-pomo-bar]").forEach(el => el.style.setProperty("--value", (100 - left / DUR[pomo.mode] * 100) + "%"));
    $$("[data-pomo-mode]").forEach(el => { el.textContent = pomo.mode === "focus" ? T.pomoFocus : T.pomoBreak; el.classList.toggle("badge-success", pomo.mode === "break"); });
    $$("[data-pomo-action=toggle]").forEach(b => b.textContent = pomo.end ? T.pomoBtnPause : (left < DUR[pomo.mode] ? T.pomoBtnResume : T.pomoBtnStart));
    $$(".pomo-btn").forEach(b => b.classList.toggle("running", !!pomo.end));
    $$("[data-pomo-preset]").forEach(b => b.classList.toggle("active", b.dataset.pomoPreset === `${DUR.focus / 60}/${DUR.break / 60}`));
    $$("[data-pomo-running]").forEach(el => el.classList.toggle("is-running", !!pomo.end));
    document.documentElement.classList.toggle("focus-on", runningFocus);
    $$("[data-owl-buddy]").forEach(o => o.classList.toggle("is-focus", runningFocus));
  }
  function pomoSave() { store.set(POMO_KEY, pomo); pomoRender(); }

  function pomoTick() {
    if (pomo.end && Date.now() >= pomo.end) {
      const wasFocus = pomo.mode === "focus";
      pomo = { mode: wasFocus ? "break" : "focus", end: null, left: wasFocus ? DUR.break : DUR.focus };
      pomoSave();
      window.eduToast?.(wasFocus ? T.pomoFocusDone : T.pomoBreakDone, wasFocus ? "coffee" : "timer");
      const buddy = $("[data-owl-buddy]:not([hidden])");
      if (wasFocus) celebrate({ xp: 25, quiet: true, x: innerWidth - 60, y: innerHeight - 90 });
      if (buddy) owl()?.say(buddy, wasFocus ? T.pomoFocusDone : T.pomoBreakDone, 4500);
      return;
    }
    pomoRender();
  }

  document.addEventListener("click", e => {
    const a = e.target.closest("[data-pomo-action]");
    if (!a) return;
    const act = a.dataset.pomoAction;
    if (act === "toggle") {
      if (pomo.end) pomo = { mode: pomo.mode, end: null, left: pomoLeft() };
      else {
        pomo = { mode: pomo.mode, end: Date.now() + pomoLeft() * 1000, left: null };
        const buddy = $("[data-owl-buddy]:not([hidden])");
        if (pomo.mode === "focus" && buddy) owl()?.say(buddy, T.pomoStart);
      }
    } else if (act === "preset") {
      const [f, b] = (a.dataset.pomoPreset || "25/5").split("/").map(Number);
      DUR.focus = f * 60; DUR.break = b * 60; store.set(DUR_KEY, DUR);
      pomo = { mode: "focus", end: null, left: DUR.focus };
    } else if (act === "reset") pomo = { mode: pomo.mode, end: null, left: DUR[pomo.mode] };
    else if (act === "skip") pomo = { mode: pomo.mode === "focus" ? "break" : "focus", end: null, left: DUR[pomo.mode === "focus" ? "break" : "focus"] };
    pomoSave();
  });
  // Another tab changed the timer
  addEventListener("storage", e => { if (e.key === POMO_KEY) { pomo = store.get(POMO_KEY) || pomo; pomoRender(); } });

  /* ============================ Buddy ============================ */
  function initBuddy(streak) {
    const buddy = $("[data-owl-buddy]");
    if (!buddy) return;
    try { if (sessionStorage.getItem("edu_buddy_hidden")) { buddy.hidden = true; return; } } catch { }

    const tips = (() => { try { return JSON.parse(buddy.dataset.tips || "[]"); } catch { return []; } })();
    let tipIndex = 0;
    const isDark = () => document.documentElement.getAttribute("data-theme") === "dark";
    let sleepTimer = 0;
    const sleepIfDark = () => { clearTimeout(sleepTimer); if (isDark()) buddy.classList.add("is-sleep"); else buddy.classList.remove("is-sleep"); };
    const scheduleNap = () => { clearTimeout(sleepTimer); if (isDark()) sleepTimer = setTimeout(() => buddy.classList.add("is-sleep"), 25000); };

    sleepIfDark();
    new MutationObserver(sleepIfDark).observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });

    buddy.addEventListener("click", e => {
      if (e.target.closest(".owl-buddy-close")) {
        buddy.hidden = true;
        try { sessionStorage.setItem("edu_buddy_hidden", "1"); } catch { }
        return;
      }
      if (buddy.classList.contains("is-sleep")) {
        buddy.classList.remove("is-sleep");
        owl()?.hop(buddy);
        owl()?.say(buddy, T.wake, 3000);
        scheduleNap();
        return;
      }
      scheduleNap();
      if (tips.length) { owl()?.say(buddy, tips[tipIndex++ % tips.length], 4200); }
    });
    buddy.addEventListener("keydown", e => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); buddy.click(); } });

    // Greeting once per browser session
    let greeted = false;
    try { greeted = !!sessionStorage.getItem("edu_buddy_greeted"); sessionStorage.setItem("edu_buddy_greeted", "1"); } catch { }
    if (greeted || isDark()) return;
    const h = new Date().getHours();
    const hello = h >= 23 || h < 5 ? T.greetNight : h < 12 ? T.greetMorning : h < 18 ? T.greetDay : T.greetEvening;
    setTimeout(() => {
      if (streak?.lost) { owl()?.mood(buddy, "is-sad", 5000); owl()?.say(buddy, fill(T.streakLost, { n: streak.lost }), 5000); return; }
      owl()?.say(buddy, hello, 3600);
      if (tips.length) setTimeout(() => owl()?.say(buddy, tips[tipIndex++ % tips.length], 4600), 4000);
    }, 1400);
  }

  /* ===================== Secret achievement card ===================== */
  function renderSecret() {
    let date = null;
    try { date = localStorage.getItem(window.eduOwl?.eggKey || "edu_owl_friend"); } catch { }
    $$("[data-secret-ach]").forEach(card => {
      if (!date) return;
      card.classList.remove("locked");
      card.querySelector("[data-secret-medal]").textContent = "🦉";
      card.querySelector("[data-secret-title]").textContent = T.secretTitle;
      card.querySelector("[data-secret-desc]").textContent = T.secretDesc;
      const foot = card.querySelector("[data-secret-foot]");
      foot.hidden = false; foot.lastChild.textContent = " " + date;
      card.querySelector(".medal .lock")?.remove();
    });
  }
  document.addEventListener("edu:secret-unlocked", renderSecret);

  /* ============================ Init ============================ */
  document.addEventListener("DOMContentLoaded", () => {
    const streak = initStreak();
    pomoRender();
    setInterval(pomoTick, 1000);
    initBuddy(streak);
    renderSecret();
  });
})();
