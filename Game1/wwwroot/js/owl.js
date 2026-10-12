/* ==========================================================================
   EDU AI — Ух the owl: shared behaviour for every [data-owl] on the page.
   - eyes follow the pointer (or the caret while typing), head tilts
   - random blinking, click phrases + hop
   - [data-owl-shy]: covers its eyes while a password field is focused
   - easter egg: 10 clicks in a row unlock the secret "Owl Friend" achievement
   Public API: window.eduOwl.{ say, mood, hop, find }
   ========================================================================== */
(function () {
  "use strict";

  const T = window.EDU_T || {};
  const reduce = matchMedia("(prefers-reduced-motion: reduce)");
  const MAX = 6; // max pupil travel, SVG units
  const owls = () => Array.from(document.querySelectorAll("[data-owl]"));
  const pick = (arr) => Array.isArray(arr) ? arr[Math.floor(Math.random() * arr.length)] : arr;
  const fill = (s, vars) => String(s || "").replace(/\{(\w+)\}/g, (_, k) => vars?.[k] ?? "");

  /* ---------- Speech & moods ---------- */
  function say(owl, text, ms = 2800) {
    owl = owl || find();
    const b = owl?.querySelector(".owl-bubble");
    if (!b || !text) return;
    b.textContent = text;
    b.classList.add("show");
    clearTimeout(b._t);
    b._t = setTimeout(() => b.classList.remove("show"), ms);
  }
  function mood(owl, cls, ms) {
    owl = owl || find();
    if (!owl) return;
    owl.classList.add(cls);
    clearTimeout(owl["_m" + cls]);
    if (ms) owl["_m" + cls] = setTimeout(() => owl.classList.remove(cls), ms);
  }
  function hop(owl) {
    owl = owl || find();
    if (!owl || reduce.matches) return;
    owl.classList.remove("is-hop"); void owl.offsetWidth; owl.classList.add("is-hop");
  }
  // The "main" owl of the page: buddy, else the first visible one
  function find() {
    return document.querySelector("[data-owl-buddy]:not([hidden])") || owls().find(o => o.offsetParent);
  }
  document.addEventListener("animationend", e => { if (e.animationName === "owlHop") e.target.classList?.remove("is-hop"); });

  /* ---------- Looking ---------- */
  let target = null, raf = 0;
  function lookAt(x, y) { target = { x, y }; if (!raf) raf = requestAnimationFrame(render); }
  function lookAhead() {
    target = null;
    owls().forEach(o => { o.querySelectorAll(".owl-pupil").forEach(p => p.style.transform = ""); const h = o.querySelector(".owl-head"); if (h) h.style.transform = ""; });
  }
  function render() {
    raf = 0;
    if (!target) return;
    owls().forEach(o => {
      if (!o.offsetParent && getComputedStyle(o).position !== "fixed") return;
      const box = o.getBoundingClientRect();
      if (box.bottom < -200 || box.top > innerHeight + 200) return;
      const eyes = o.querySelectorAll(".owl-eye"), pupils = o.querySelectorAll(".owl-pupil");
      eyes.forEach((eye, i) => {
        const r = eye.getBoundingClientRect();
        const dx = target.x - (r.left + r.width / 2), dy = target.y - (r.top + r.height / 2);
        const d = Math.hypot(dx, dy) || 1, k = Math.min(MAX, d / 18) / d;
        pupils[i].style.transform = `translate(${(dx * k).toFixed(2)}px, ${(dy * k).toFixed(2)}px)`;
      });
      const head = o.querySelector(".owl-head");
      if (head) head.style.transform = `rotate(${Math.max(-8, Math.min(8, (target.x - (box.left + box.width / 2)) / 60)).toFixed(2)}deg)`;
    });
  }

  // Approximate caret position inside a text field so the owl "reads" what is typed
  const TEXTY = "input:not([type=checkbox]):not([type=radio]):not([type=password]):not([type=range]), textarea";
  const ctx = document.createElement("canvas").getContext("2d");
  function caretPoint(input) {
    const r = input.getBoundingClientRect(), cs = getComputedStyle(input);
    ctx.font = `${cs.fontWeight} ${cs.fontSize} ${cs.fontFamily}`;
    let pos = input.value.length;
    try { if (input.selectionStart != null) pos = input.selectionStart; } catch { /* type=email has no selection API */ }
    const x = r.left + parseFloat(cs.paddingLeft) + ctx.measureText(input.value.slice(0, pos)).width - input.scrollLeft;
    return { x: Math.min(x, r.right - 8), y: r.top + Math.min(r.height / 2, 24) };
  }
  const isTyping = () => document.activeElement?.matches?.(TEXTY);
  function followCaret(e) { if (e.target.matches?.(TEXTY)) { const p = caretPoint(e.target); lookAt(p.x, p.y); } }

  document.addEventListener("pointermove", e => { if (!isTyping()) lookAt(e.clientX, e.clientY); }, { passive: true });
  document.addEventListener("pointerdown", e => lookAt(e.clientX, e.clientY), { passive: true });
  document.documentElement.addEventListener("mouseleave", lookAhead);
  ["input", "keyup", "click"].forEach(t => document.addEventListener(t, followCaret));

  /* ---------- Blinking ---------- */
  (function blinkLoop() {
    setTimeout(() => {
      if (!reduce.matches && !document.hidden) {
        owls().forEach(o => {
          if (o.classList.contains("is-sleep")) return;
          setTimeout(() => { o.classList.add("is-blink"); setTimeout(() => o.classList.remove("is-blink"), 140); }, Math.random() * 400);
        });
      }
      blinkLoop();
    }, 2500 + Math.random() * 3500);
  })();

  /* ---------- Shy owl (auth forms) ---------- */
  const pwInputs = Array.from(document.querySelectorAll("input[type=password]"));
  function pwField(el) {
    const group = el && el.closest?.(".input-group");
    return pwInputs.find(i => i === el || (group && group.contains(i)));
  }
  function updateShy() {
    const pw = pwField(document.activeElement);
    document.querySelectorAll("[data-owl-shy]").forEach(owl => {
      const wasShy = owl.classList.contains("is-shy"), wasPeek = owl.classList.contains("is-peek");
      const peek = !!pw && pw.type === "text";
      owl.classList.toggle("is-shy", !!pw);
      owl.classList.toggle("is-peek", peek);
      if (pw && !wasShy) say(owl, owl.dataset.sayShy);
      else if (peek && !wasPeek) say(owl, owl.dataset.sayPeek);
    });
  }
  document.addEventListener("focusin", e => { updateShy(); followCaret(e); });
  document.addEventListener("focusout", () => setTimeout(updateShy, 0));
  const mo = new MutationObserver(updateShy);
  pwInputs.forEach(i => mo.observe(i, { attributes: true, attributeFilter: ["type"] }));
  document.addEventListener("submit", e => {
    document.querySelectorAll("[data-owl-shy]").forEach(owl => {
      if (!owl.closest(".auth-card")?.contains(e.target)) return;
      owl.classList.remove("is-shy", "is-peek");
      owl.classList.add("is-happy");
      hop(owl);
      say(owl, owl.dataset.sayHappy, 4000);
    });
  });

  /* ---------- Clicks: phrases, hop, easter egg ---------- */
  const EGG_KEY = "edu_owl_friend";
  let clicks = 0, lastClick = 0, lastPhrase = -1;
  document.addEventListener("click", e => {
    const owl = e.target.closest?.("[data-owl]");
    if (!owl || e.target.closest(".owl-buddy-close")) return;
    if (owl.classList.contains("is-sleep") || owl.dataset.owlClick === "off") return; // buddy handles waking itself

    const now = Date.now();
    clicks = now - lastClick < 1500 ? clicks + 1 : 1;
    lastClick = now;
    if (clicks >= 10) { clicks = 0; easterEgg(owl); return; }

    hop(owl);
    if (owl.dataset.owlClick === "custom") return; // page script provides its own phrase
    const phrases = owl.dataset.sayClick ? owl.dataset.sayClick.split("|") : (T.click || []);
    if (!phrases.length) return;
    let i = Math.floor(Math.random() * phrases.length);
    if (phrases.length > 1 && i === lastPhrase) i = (i + 1) % phrases.length;
    lastPhrase = i;
    say(owl, phrases[i]);
  });

  function easterEgg(owl) {
    owl.classList.remove("is-roll"); void owl.offsetWidth; owl.classList.add("is-roll");
    setTimeout(() => owl.classList.remove("is-roll"), 950);
    let had = false;
    try { had = !!localStorage.getItem(EGG_KEY); if (!had) localStorage.setItem(EGG_KEY, new Date().toISOString().slice(0, 10)); } catch { }
    say(owl, had ? T.eggAgain : T.eggSay, 3200);
    if (had) return;
    window.eduToast?.(T.eggToast, "sparkles");
    const r = owl.getBoundingClientRect();
    window.eduCelebrate?.({ xp: 100, x: r.left + r.width / 2, y: r.top, big: true, quiet: true });
    document.dispatchEvent(new CustomEvent("edu:secret-unlocked"));
  }

  window.eduOwl = { say, mood, hop, find, pick, fill, eggKey: EGG_KEY };
})();
