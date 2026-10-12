/* Robo-Hoot — Lightbot-style programming: moves, turns and a function that may call itself (a loop). */
(function () {
  "use strict";
  const G = window.EduGames, T = window.RB_T, $ = (s) => document.querySelector(s);

  // '.' floor, '*' star, 'S' start (facing east), '#' void
  const LEVELS = [
    { map: ["S.*"], main: 5, f1: 0, best: 2 },
    { map: ["S.#", "#.#", "#.*"], main: 8, f1: 0, best: 6 },
    { map: ["S*.*.*"], main: 6, f1: 0, best: 5 },
    { map: ["S.......*"], main: 3, f1: 4, best: 6 },
    { map: ["S*####", "#.*###", "##.*##", "###.*#", "####.*"], main: 3, f1: 5, best: 7 },
    { map: ["S..*", ".##.", ".##.", "*..*"], main: 3, f1: 4, best: 7 },
    { map: ["S.#*", "#.#.", "#..."], main: 8, f1: 3, best: 9 },
  ];
  const DX = [1, 0, -1, 0], DY = [0, 1, 0, -1];   // E, S, W, N
  const ICON = { F: "↑", L: "↺", R: "↻", P: "ƒ1" };
  const MAX_STEPS = 150;

  let lvl, L, prog, active, running, robot, starsLeft;

  function start() {
    const st = G.getStars("robot");
    lvl = LEVELS.findIndex((_, i) => !st[i]); if (lvl < 0) lvl = 0;
    load();
  }
  const unlocked = (i) => { const st = G.getStars("robot"); return i === 0 || !!st[i - 1] || !!st[i]; };

  function load() {
    L = LEVELS[lvl]; prog = { main: [], f1: [] }; active = "main"; running = false;
    renderLevels();
    $("[data-rb-title]").textContent = `${T.level} ${lvl + 1}. ${T.names[lvl]}`;
    $("[data-rb-best]").textContent = T.best.replace("{n}", L.best);
    document.querySelector('[data-prog="f1"]').hidden = L.f1 === 0;
    document.querySelector('[data-cmd="P"]').hidden = L.f1 === 0;
    G.hud.progress(lvl + 1, LEVELS.length);
    reset(); renderCode();
  }

  function renderLevels() {
    const st = G.getStars("robot");
    $("[data-rb-levels]").innerHTML = LEVELS.map((_, i) => `<button type="button" class="rx-lvl ${i === lvl ? "active" : ""} ${st[i] ? "solved" : ""}" data-rlvl="${i}" ${unlocked(i) ? "" : "disabled"}><b>${i + 1}</b><small>${st[i] ? "★".repeat(st[i]) : unlocked(i) ? "·" : "🔒"}</small></button>`).join("");
  }

  function reset() {
    robot = null; starsLeft = 0;
    const rows = L.map.length, cols = Math.max(...L.map.map(r => r.length));
    const board = $("[data-rb-board]");
    board.style.setProperty("--cols", cols); board.style.setProperty("--rows", rows);
    let html = "";
    L.map.forEach((row, y) => [...row.padEnd(cols, "#")].forEach((c, x) => {
      if (c === "S") robot = { x, y, d: 0 };
      if (c === "*") starsLeft++;
      html += `<div class="rb-tile ${c === "#" ? "void" : ""}" data-xy="${x},${y}">${c === "*" ? '<span class="rb-star">⭐</span>' : ""}</div>`;
    }));
    html += `<div class="rb-bot" data-bot><svg viewBox="0 0 40 40" aria-hidden="true"><rect x="7" y="9" width="26" height="22" rx="8" fill="#7C3AED"/><circle cx="15" cy="19" r="5" fill="#fff"/><circle cx="25" cy="19" r="5" fill="#fff"/><circle cx="16.5" cy="19" r="2.2" fill="#1B1530"/><circle cx="26.5" cy="19" r="2.2" fill="#1B1530"/><path d="M18 26 l2 2.5 l2 -2.5z" fill="#F59E0B"/><path d="M20 9 v-5" stroke="#4C1D95" stroke-width="2"/><circle cx="20" cy="3.5" r="2.5" fill="#F59E0B"/><path d="M33 20 l5 0" stroke="#F59E0B" stroke-width="3" stroke-linecap="round"/></svg></div>`;
    board.innerHTML = html;
    place(false);
    status("");
  }

  function place(animate = true) {
    const b = $("[data-bot]"); if (!b) return;
    b.style.transition = animate ? "" : "none";
    b.style.setProperty("--x", robot.x); b.style.setProperty("--y", robot.y); b.style.setProperty("--r", robot.d * 90 + "deg");
  }
  const status = (t, cls = "") => { const s = $("[data-rb-status]"); s.textContent = t; s.className = "rb-status " + cls; };

  function renderCode(hl) {
    ["main", "f1"].forEach(p => {
      const max = p === "main" ? L.main : L.f1;
      $(`[data-slots="${p}"]`).innerHTML = Array.from({ length: max }, (_, i) => {
        const c = prog[p][i];
        return `<button type="button" class="rb-slot ${c ? "filled" : ""} ${c === "P" ? "fn" : ""} ${hl && hl.p === p && hl.i === i ? "running" : ""}" data-slot="${p}:${i}" ${c ? "" : "tabindex='-1'"}>${c ? ICON[c] : ""}</button>`;
      }).join("");
      document.querySelector(`[data-prog="${p}"]`).classList.toggle("active", active === p);
    });
  }

  const wait = (ms) => new Promise(r => setTimeout(r, ms));
  async function run() {
    if (running) return;
    reset(); running = true; $("[data-rb-run]").disabled = true;
    const stack = [{ p: "main", i: 0 }];
    let steps = 0, result = "end";
    while (stack.length) {
      const fr = stack[stack.length - 1];
      if (fr.i >= prog[fr.p].length) { stack.pop(); continue; }
      const c = prog[fr.p][fr.i]; renderCode({ p: fr.p, i: fr.i }); fr.i++;
      if (++steps > MAX_STEPS) { result = "loop"; break; }
      if (c === "P") { if (stack.length > 60) { result = "loop"; break; } stack.push({ p: "f1", i: 0 }); await wait(120); continue; }
      if (c === "L") robot.d = (robot.d + 3) % 4;
      if (c === "R") robot.d = (robot.d + 1) % 4;
      if (c === "F") {
        const nx = robot.x + DX[robot.d], ny = robot.y + DY[robot.d];
        const ch = (L.map[ny] || "")[nx];
        robot.x = nx; robot.y = ny;
        if (!ch || ch === "#") { place(); $("[data-bot]").classList.add("fall"); result = "fell"; break; }
        const tile = document.querySelector(`[data-xy="${nx},${ny}"] .rb-star`);
        if (tile && !tile.classList.contains("got")) { tile.classList.add("got"); starsLeft--; G.sfx.ok(); }
      }
      place(); await wait(330);
      if (starsLeft === 0) { result = "win"; break; }
    }
    renderCode(); running = false; $("[data-rb-run]").disabled = false;
    if (result === "win") win();
    else { G.sfx.bad(); G.owl.bad(false); status(result === "fell" ? T.fell : result === "loop" ? T.loop : T.missing.replace("{n}", starsLeft), "bad"); }
  }

  function win() {
    const used = prog.main.length + prog.f1.length;
    const s = used <= L.best ? 3 : used <= L.best + 2 ? 2 : 1;
    G.setStars("robot", lvl, s);
    renderLevels();   // the next level unlocks right away
    status(T.win.replace("{n}", used), "ok");
    const total = Object.values(G.getStars("robot")).reduce((a, b) => a + b, 0);
    G.finish("robot", {
      win: true, score: total * 10, base: 10 + s * 5 + lvl * 2, perfect: s === 3, stars: s,
      title: `${T.level} ${lvl + 1} ✓`, text: T.win.replace("{n}", used),
      stats: [{ label: T.commands, value: `${used} / ${L.best}` }, { label: T.stars, value: "★ " + total }],
    });
  }

  document.addEventListener("click", e => {
    if (running) return;
    const c = e.target.closest("[data-cmd]");
    if (c) { const max = active === "main" ? L.main : L.f1; if (prog[active].length < max) { prog[active].push(c.dataset.cmd); G.sfx.click(); renderCode(); } else G.shake(document.querySelector(`[data-prog="${active}"]`)); return; }
    const s = e.target.closest("[data-slot]");
    if (s) { const [p, i] = s.dataset.slot.split(":"); active = p; if (prog[p][+i]) prog[p].splice(+i, 1); renderCode(); return; }
    const row = e.target.closest("[data-prog]"); if (row) { active = row.dataset.prog; renderCode(); }
    if (e.target.closest("[data-rb-run]")) run();
    if (e.target.closest("[data-rb-reset]")) reset();
    if (e.target.closest("[data-rb-clear]")) { prog = { main: [], f1: [] }; reset(); renderCode(); }
    const lv = e.target.closest("[data-rlvl]"); if (lv && !lv.disabled) { lvl = +lv.dataset.rlvl; load(); }
  });
  G.intro(start);
  G.onAgain(() => { $("[data-game-intro]").hidden = true; $("[data-game-stage]").hidden = false; const st = G.getStars("robot"); if (st[lvl] && lvl < LEVELS.length - 1) lvl++; load(); });
})();
