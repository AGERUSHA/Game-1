/* Be the sort — execute bubble sort or selection sort by hand on random crystals. */
(function () {
  "use strict";
  const G = window.EduGames, T = window.SRT_T, $ = (s) => document.querySelector(s);
  const LIVES = 3;
  let algo, a, n, pass, j, sortedFrom, sortedTo, lives, steps, cmp, swp, score, combo, started, busy, review, passSwaps;

  function start() {
    algo = G.option("algo") || "bubble";
    n = +(G.option("size") || 6);
    // distinct random values 1..20 — a brand new array every round
    a = G.sample(Array.from({ length: 20 }, (_, i) => i + 1), n);
    pass = 0; j = 0; passSwaps = 0; sortedFrom = n; sortedTo = 0; lives = LIVES; steps = 0; cmp = 0; swp = 0; score = 0; combo = 0; busy = false; review = [];
    started = Date.now();
    G.hud.score(0); G.hud.lives(lives, LIVES); G.hud.combo(0);
    $("[data-srt-algo]").textContent = T[algo];
    $("[data-srt-task]").textContent = algo === "bubble" ? T.bubbleTask : T.selectionTask;
    $("[data-srt-bubble]").hidden = algo !== "bubble";
    $("[data-srt-note]").textContent = "";
    if (bubbleSorted() && algo === "bubble") a.reverse();   // never start already sorted
    render();
  }

  const bubbleSorted = () => a.every((v, i) => i === 0 || a[i - 1] <= v);

  function render(flash) {
    const max = Math.max(...a);
    $("[data-srt-stage]").innerHTML = a.map((v, i) => {
      const done = algo === "bubble" ? i >= sortedFrom : i < sortedTo;
      const focus = algo === "bubble" && (i === j || i === j + 1) && !done;
      const pickable = algo === "selection" && !done;
      return `<button type="button" class="gem ${done ? "done" : ""} ${focus ? "focus" : ""} ${pickable ? "pickable" : ""} ${flash?.[i] || ""}" data-gem="${i}" style="--h:${Math.round(v / max * 100)}%" ${pickable ? "" : "tabindex='-1'"}>
          <span class="gem-body"></span><b>${v}</b></button>`;
    }).join("");
    $("[data-srt-pass]").textContent = T.pass.replace("{p}", pass + 1);
    $("[data-srt-cmp]").textContent = cmp; $("[data-srt-swp]").textContent = swp;
  }

  function good(points) {
    steps++; combo++; score += points * Math.min(combo, 4); G.hud.score(score); G.hud.combo(combo);
    combo % 6 === 0 ? G.sfx.combo() : G.sfx.ok();
  }
  function bad(text) {
    lives--; combo = 0; G.hud.lives(lives, LIVES); G.hud.combo(0); G.sfx.bad(); G.owl.bad(text);
    G.shake($("[data-srt-stage]"));
    if (review.length < 4) review.push(text);
    if (lives <= 0) setTimeout(() => end(false), 500);
  }

  /* ---- Bubble: decide swap/keep for pair (j, j+1) ---- */
  function bubble(choice) {
    if (busy || lives <= 0 || algo !== "bubble") return;
    cmp++;
    const need = a[j] > a[j + 1];
    if ((choice === "swap") === need) {
      good(10);
      if (need) { [a[j], a[j + 1]] = [a[j + 1], a[j]]; swp++; passSwaps++; render({ [j]: "swapped", [j + 1]: "swapped" }); }
    } else {
      bad((choice === "swap" ? T.wrongSwap : T.wrongKeep).replace("{a}", a[j]).replace("{b}", a[j + 1]));
      if (need) { [a[j], a[j + 1]] = [a[j + 1], a[j]]; swp++; passSwaps++; }   // the algorithm continues correctly
    }
    j++;
    if (j >= sortedFrom - 1) {
      // Optimized bubble sort: a pass without swaps means the array is already sorted
      if (passSwaps === 0) sortedFrom = 0; else sortedFrom--;
      j = 0; pass++; passSwaps = 0;
    }
    if (sortedFrom <= 1) sortedFrom = 0;
    if (lives > 0) { render(); if (sortedFrom === 0) end(true); }
  }

  /* ---- Selection: click the minimum of the unsorted part ---- */
  function select(i) {
    if (busy || lives <= 0 || algo !== "selection" || i < sortedTo) return;
    cmp += n - sortedTo - 1;
    const rest = a.slice(sortedTo), min = Math.min(...rest), mi = sortedTo + rest.indexOf(min);
    if (i === mi) {
      good(15);
      if (mi !== sortedTo) { [a[sortedTo], a[mi]] = [a[mi], a[sortedTo]]; swp++; }
      const flash = { [sortedTo]: "swapped" }; if (mi !== sortedTo) flash[mi] = "swapped";
      sortedTo++; pass++;
      if (sortedTo >= n - 1) { sortedTo = n; render(flash); return end(true); }
      render(flash);
    } else {
      bad(T.wrongMin.replace("{m}", min));
      render({ [mi]: "hint" });
    }
  }

  function end(win) {
    busy = true;
    if (win) { $("[data-srt-stage]").querySelectorAll(".gem").forEach((g, k) => setTimeout(() => g.classList.add("done", "cheer"), k * 60)); G.owl.good(T.sorted); }
    if (algo === "bubble") $("[data-srt-note]").textContent = " · " + T.note;
    const secs = Math.round((Date.now() - started) / 1000);
    setTimeout(() => G.finish("sort", {
      win, score, base: win ? Math.min(90, 20 + steps * 2 + (lives === LIVES ? 20 : 0)) : Math.min(20, steps),
      perfect: win && lives === LIVES, stars: win ? (lives === 3 ? 3 : lives === 2 ? 2 : 1) : 0,
      title: win ? T.sorted : T.fail, text: `${T[algo]} · n = ${n}`,
      stats: [{ label: T.steps, value: steps }, { label: T.time, value: secs + "s" }, { label: T.score, value: score }],
      review,
    }), win ? 900 : 300);
  }

  document.addEventListener("click", e => {
    const b = e.target.closest("[data-srt]"); if (b) bubble(b.dataset.srt);
    const g = e.target.closest("[data-gem]"); if (g) select(+g.dataset.gem);
  });
  document.addEventListener("keydown", e => {
    if ($("[data-game-stage]").hidden || algo !== "bubble") return;
    if (e.key === "ArrowLeft") { e.preventDefault(); bubble("swap"); }
    if (e.key === "ArrowRight") { e.preventDefault(); bubble("keep"); }
  });
  G.intro(start);
})();
