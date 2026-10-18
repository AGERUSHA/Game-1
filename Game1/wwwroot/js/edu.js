/* ==========================================================================
   EDU AI — UI behaviour (prototype, no backend)
   ========================================================================== */
(function () {
  "use strict";

  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));
  const css = (name) => getComputedStyle(document.documentElement).getPropertyValue(name).trim();

  /* ---------- Icons ---------- */
  function icons() { if (window.lucide) window.lucide.createIcons(); }
  window.eduIcons = icons;

  /* ---------- Theme ---------- */
  const THEME_KEY = "edu_theme";
  function storedTheme() { try { return localStorage.getItem(THEME_KEY) || "light"; } catch { return "light"; } }
  function applyTheme(mode) {
    const dark = mode === "dark" || (mode === "system" && matchMedia("(prefers-color-scheme: dark)").matches);
    document.documentElement.setAttribute("data-theme", dark ? "dark" : "light");
    $$("[data-theme-set]").forEach(b => b.classList.toggle("active", b.dataset.themeSet === mode));
    $$("[data-theme-toggle]").forEach(b => b.setAttribute("aria-pressed", String(dark)));
    renderCharts();
  }
  function setTheme(mode) { try { localStorage.setItem(THEME_KEY, mode); } catch { } applyTheme(mode); }

  /* ---------- Language (cookie, read by LangService on the server) ---------- */
  function setLang(lang) {
    document.cookie = "edu_lang=" + lang + ";path=/;max-age=31536000;samesite=lax";
    location.reload();
  }

  /* ---------- Toasts ---------- */
  function toast(message, icon = "check-circle-2") {
    let box = $(".toasts");
    if (!box) { box = document.createElement("div"); box.className = "toasts"; document.body.appendChild(box); }
    const t = document.createElement("div");
    t.className = "toast";
    t.setAttribute("role", "status");
    t.innerHTML = `<i data-lucide="${icon}" class="icon"></i><span></span>`;
    t.querySelector("span").textContent = message;
    box.appendChild(t);
    icons();
    setTimeout(() => { t.classList.add("hide"); setTimeout(() => t.remove(), 320); }, 2800);
  }
  window.eduToast = toast;

  /* ---------- Charts ---------- */
  const charts = [];
  function seriesColor(key) {
    return ({ primary: css("--chart-1"), teal: css("--chart-2"), pink: css("--chart-3"), light: css("--primary-300") })[key] || css("--chart-1");
  }
  function hexToRgba(hex, a) {
    const m = hex.replace("#", "").match(/.{2}/g); if (!m) return hex;
    const [r, g, b] = m.map(x => parseInt(x, 16)); return `rgba(${r},${g},${b},${a})`;
  }
  function buildChart(canvas) {
    const spec = JSON.parse(canvas.dataset.chart);
    const ctx = canvas.getContext("2d");
    const grid = css("--chart-grid"), text = css("--text-3"), suffix = spec.suffix || "";
    const isDonut = spec.type === "doughnut";
    const datasets = spec.series.map((s, i) => {
      const color = seriesColor(s.color);
      if (isDonut) {
        return { label: s.label, data: s.data, borderWidth: 3, borderColor: css("--surface"), hoverOffset: 6,
                 backgroundColor: [css("--chart-1"), css("--primary-300"), css("--primary-100")], borderRadius: 6, spacing: 2 };
      }
      if (spec.type === "line") {
        const h = canvas.parentElement.clientHeight || 280;
        const g = ctx.createLinearGradient(0, 0, 0, h);
        g.addColorStop(0, hexToRgba(color, i === 0 ? .22 : 0)); g.addColorStop(1, hexToRgba(color, 0));
        return { label: s.label, data: s.data, borderColor: color, backgroundColor: g, fill: i === 0, tension: .4,
                 borderWidth: 2.5, pointRadius: 0, pointHoverRadius: 6, pointHoverBorderWidth: 3,
                 pointHoverBackgroundColor: color, pointHoverBorderColor: css("--surface"), borderDash: i > 0 ? [6, 5] : [] };
      }
      const max = Math.max(...s.data);
      return { label: s.label, data: s.data, borderRadius: 6, borderSkipped: false, maxBarThickness: 34, barPercentage: .7, categoryPercentage: .7,
               backgroundColor: spec.series.length === 1 ? s.data.map(v => v === max ? color : hexToRgba(color, .32)) : color,
               hoverBackgroundColor: color };
    });

    return new Chart(ctx, {
      type: spec.type,
      data: { labels: spec.labels, datasets },
      options: {
        responsive: true, maintainAspectRatio: false, indexAxis: spec.horizontal ? "y" : "x",
        cutout: isDonut ? "74%" : undefined,
        interaction: { mode: isDonut ? "nearest" : "index", intersect: false },
        animation: { duration: 900, easing: "easeOutQuart" },
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: "#1B1530", titleColor: "#fff", bodyColor: "#E4DEF7", padding: 12, cornerRadius: 10,
            titleFont: { family: "Inter", weight: "600", size: 12 }, bodyFont: { family: "Inter", size: 12 },
            boxPadding: 6, usePointStyle: true,
            callbacks: { label: c => ` ${c.dataset.label}: ${c.formattedValue}${suffix}` }
          }
        },
        scales: isDonut ? {} : {
          x: { grid: { display: false }, border: { display: false }, ticks: { color: text, font: { family: "Inter", size: 12 } }, stacked: spec.stacked },
          y: { grid: { color: grid }, border: { display: false }, beginAtZero: true, stacked: spec.stacked,
               ticks: { color: text, font: { family: "Inter", size: 12 }, padding: 8, maxTicksLimit: 5, callback: v => v + suffix } }
        }
      }
    });
  }
  // Chart.js is 200 KB, and most pages have no chart at all, so it is fetched
  // only when a <canvas data-chart> is on the page. Loading starts once; later
  // calls (theme switch, tab switch) just redraw.
  let chartLib = null;
  function loadChartLib() {
    if (chartLib) return chartLib;
    chartLib = new Promise((resolve, reject) => {
      const s = document.createElement("script");
      s.src = "/lib/chartjs/chart.umd.min.js";
      s.onload = resolve;
      s.onerror = () => { chartLib = null; reject(new Error("chart.js failed to load")); };
      document.head.appendChild(s);
    });
    return chartLib;
  }
  function renderCharts() {
    charts.splice(0).forEach(c => c.destroy());
    const canvases = $$("canvas[data-chart]");
    if (!canvases.length) return;
    if (!window.Chart) { loadChartLib().then(renderCharts).catch(e => console.warn(e)); return; }
    canvases.forEach(c => { try { charts.push(buildChart(c)); } catch (e) { console.warn(e); } });
  }

  /* ---------- On-view animations ---------- */
  function animateOnView() {
    const io = "IntersectionObserver" in window ? new IntersectionObserver(entries => {
      entries.forEach(e => {
        if (!e.isIntersecting) return;
        run(e.target); io.unobserve(e.target);
      });
    }, { threshold: .15 }) : null;
    function run(el) {
      if (el.classList.contains("progress")) el.classList.add("is-ready");
      else if (el.classList.contains("ring")) { const v = el.querySelector(".ring-value"); if (v) v.style.strokeDashoffset = v.dataset.offset; }
      else if (el.dataset.count) countUp(el);
      else el.classList.add("in");
    }
    $$(".progress, .ring, .reveal, [data-count]").forEach(el => io ? io.observe(el) : run(el));
  }
  function countUp(el) {
    const target = parseFloat(el.dataset.count), dur = 1100, start = performance.now();
    const fmt = n => Math.round(n).toLocaleString("ru-RU").replace(/,/g, " ");
    (function tick(now) {
      const p = Math.min(1, (now - start) / dur), eased = 1 - Math.pow(1 - p, 3);
      el.textContent = fmt(target * eased) + (el.dataset.suffix || "");
      if (p < 1) requestAnimationFrame(tick);
    })(start);
  }

  /* ---------- Filters & search ---------- */
  // Several filter groups (chips, segmented) may target the same container — all must match.
  function applyFilter(scope) { applyFilterFor(scope.dataset.filterTarget); }
  function applyFilterFor(sel) {
    const target = $(sel);
    if (!target) return;
    const vals = $$(`[data-filter-target="${sel}"]`).map(s => s.querySelector(".active")?.dataset.filter || "all")
      .concat($$(`select[data-select-filter="${sel}"]`).map(s => s.value));
    const searchInput = $(`[data-search-target="${sel}"]`);
    filterItems(target, vals, searchInput ? searchInput.value.trim().toLowerCase() : "");
  }
  function filterItems(target, vals, q) {
    vals = [].concat(vals || []);
    let visible = 0;
    $$("[data-tags]", target).forEach(item => {
      const tags = item.dataset.tags.split(" ");
      const okTag = vals.every(v => !v || v === "all" || tags.includes(v));
      const okText = !q || item.textContent.toLowerCase().includes(q);
      const show = okTag && okText;
      item.hidden = !show; if (show) visible++;
    });
    const empty = $(`[data-empty-for="#${target.id}"]`);
    if (empty) empty.hidden = visible > 0;
  }

  /* ---------- Table sorting ---------- */
  function sortTable(th) {
    const table = th.closest("table"), tbody = table.tBodies[0];
    const idx = Array.from(th.parentNode.children).indexOf(th);
    const dir = th.dataset.dir === "asc" ? "desc" : "asc";
    $$("th", table).forEach(h => delete h.dataset.dir);
    th.dataset.dir = dir;
    const rows = Array.from(tbody.rows);
    rows.sort((a, b) => {
      const av = a.cells[idx].dataset.value ?? a.cells[idx].textContent.trim();
      const bv = b.cells[idx].dataset.value ?? b.cells[idx].textContent.trim();
      const na = parseFloat(av), nb = parseFloat(bv);
      const r = !isNaN(na) && !isNaN(nb) ? na - nb : av.localeCompare(bv);
      return dir === "asc" ? r : -r;
    });
    rows.forEach(r => tbody.appendChild(r));
  }

  /* ---------- Password strength ---------- */
  function pwScore(v) {
    let s = 0; if (v.length >= 8) s++; if (/[A-ZА-Я]/.test(v) && /[a-zа-я]/.test(v)) s++; if (/\d/.test(v)) s++; if (/[^\w\s]/.test(v)) s++;
    return v ? Math.max(1, s) : 0;
  }

  /* ---------- Global event delegation ---------- */
  document.addEventListener("click", (e) => {
    const t = e.target;

    // Dropdowns
    const ddToggle = t.closest("[data-dropdown]");
    if (ddToggle) {
      const dd = ddToggle.closest(".dropdown");
      const wasOpen = dd.classList.contains("open");
      $$(".dropdown.open").forEach(d => d.classList.remove("open"));
      if (!wasOpen) dd.classList.add("open");
      ddToggle.setAttribute("aria-expanded", String(!wasOpen));
      e.preventDefault(); return;
    }
    if (!t.closest(".menu")) $$(".dropdown.open").forEach(d => d.classList.remove("open"));

    // Language
    const lang = t.closest("[data-lang]");
    if (lang) { e.preventDefault(); setLang(lang.dataset.lang); return; }

    // Theme
    const themeSet = t.closest("[data-theme-set]");
    if (themeSet) { setTheme(themeSet.dataset.themeSet); return; }
    if (t.closest("[data-theme-toggle]")) {
      setTheme(document.documentElement.getAttribute("data-theme") === "dark" ? "light" : "dark"); return;
    }

    // Sidebar (mobile)
    if (t.closest("[data-sidebar-toggle]")) {
      $(".sidebar")?.classList.toggle("open"); $(".sidebar-backdrop")?.classList.toggle("open"); return;
    }
    // Public mobile menu
    if (t.closest("[data-pubmenu-toggle]")) { $(".pub-mobile")?.classList.toggle("open"); return; }

    // Chips / segmented filters
    const chip = t.closest("[data-filter]");
    if (chip) {
      const scope = chip.closest("[data-filter-target]");
      $$("[data-filter]", scope).forEach(c => c.classList.toggle("active", c === chip));
      applyFilter(scope); return;
    }

    // Plain segmented controls (visual state only)
    const seg = t.closest(".segmented > button");
    if (seg && !seg.dataset.tab) { $$("button", seg.parentElement).forEach(b => b.classList.toggle("active", b === seg)); }

    // Generic tabs
    const tab = t.closest("[data-tab]");
    if (tab) {
      const scope = tab.closest("[data-tabs-scope]") || document;
      $$("[data-tab]", scope).forEach(b => b.classList.toggle("active", b.dataset.tab === tab.dataset.tab));
      $$("[data-tab-panel]", scope).forEach(p => p.classList.toggle("active", p.dataset.tabPanel === tab.dataset.tab));
      const steps = $$(".step", scope), cur = steps.findIndex(s => s.classList.contains("active"));
      steps.forEach((s, k) => s.classList.toggle("done", k < cur));
      renderCharts(); return;
    }

    // Accordion modules
    const mh = t.closest(".module-head");
    if (mh) { mh.parentElement.classList.toggle("open"); return; }

    // Toast trigger (mock actions: export, save, etc.). An empty data-toast is ignored:
    // Razor renders data-* attributes even when the value is null, and such a button must still submit.
    const toastBtn = t.closest("[data-toast]:not([data-toast=''])");
    if (toastBtn) { e.preventDefault(); toast(toastBtn.dataset.toast, toastBtn.dataset.toastIcon || "check-circle-2"); }

    // Modal
    const mOpen = t.closest("[data-modal-open]");
    if (mOpen) { e.preventDefault(); $(mOpen.dataset.modalOpen)?.classList.add("open"); return; }
    if (t.closest("[data-modal-close]") || t.classList.contains("modal-backdrop")) {
      t.closest(".modal-backdrop")?.classList.remove("open");
    }

    // Sortable tables
    const th = t.closest("th.sortable");
    if (th) { sortTable(th); return; }

    // Password visibility
    const pw = t.closest("[data-pw-toggle]");
    if (pw) {
      const input = pw.parentElement.querySelector("input");
      input.type = input.type === "password" ? "text" : "password";
      pw.innerHTML = `<i data-lucide="${input.type === "password" ? "eye" : "eye-off"}" class="icon"></i>`; icons(); return;
    }

    // Single-select option groups (cover picker, option cards)
    const pick = t.closest("[data-pick]");
    if (pick) { $$("[data-pick]", pick.parentElement).forEach(p => p.classList.toggle("active", p === pick)); }
  });

  document.addEventListener("input", (e) => {
    const t = e.target;
    if (t.matches("[data-search-target]")) {
      applyFilterFor(t.dataset.searchTarget);
    }
    if (t.matches("[data-pw-meter]")) {
      const m = $(t.dataset.pwMeter); if (m) m.className = "pw-meter s" + pwScore(t.value);
    }
  });

  document.addEventListener("change", (e) => {
    const t = e.target;
    if (t.matches("select[data-select-filter]")) applyFilterFor(t.dataset.selectFilter);
  });

  document.addEventListener("submit", (e) => {
    const f = e.target;
    if (!f.matches("[data-mock-redirect]")) return;
    e.preventDefault();
    const btn = f.querySelector("[type=submit]");
    if (btn) { btn.disabled = true; btn.innerHTML = `<span class="spinner"></span>${btn.dataset.loading || ""}`; }
    const role = f.querySelector("input[name=role]:checked")?.value;
    const url = role === "teacher" && f.dataset.teacherRedirect ? f.dataset.teacherRedirect : f.dataset.mockRedirect;
    setTimeout(() => location.href = url, 650);
  });

  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      $$(".dropdown.open").forEach(d => d.classList.remove("open"));
      $$(".modal-backdrop.open").forEach(m => m.classList.remove("open"));
      $(".sidebar")?.classList.remove("open"); $(".sidebar-backdrop")?.classList.remove("open");
    }
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
      const s = $(".topbar-search input"); if (s) { e.preventDefault(); s.focus(); }
    }
  });

  // Sticky bars get a border once the page scrolls
  const onScroll = () => {
    const y = window.scrollY > 8;
    $(".topbar")?.classList.toggle("scrolled", y);
    $(".pub-nav")?.classList.toggle("scrolled", y);
  };
  window.addEventListener("scroll", onScroll, { passive: true });

  matchMedia("(prefers-color-scheme: dark)").addEventListener?.("change", () => { if (storedTheme() === "system") applyTheme("system"); });

  /* ---------- File dropzone: <label data-dropzone="#list" data-max-mb data-accept> ---------- */
  // Chosen / dropped files are kept in a DataTransfer and mirrored into the zone's <input type=file>,
  // so the surrounding form uploads exactly the files listed (invalid ones are shown but not sent).
  // A form with [data-upload] posts via XHR to show real upload progress, then follows the redirect.
  function fmtSize(b) { return b > 1048576 ? (b / 1048576).toFixed(1) + " MB" : Math.max(1, Math.round(b / 1024)) + " KB"; }
  function fileIcon(name) {
    const ext = name.split(".").pop().toLowerCase();
    return { zip: "file-archive", rar: "file-archive", sql: "database", pdf: "file-text", doc: "file-text", docx: "file-text", png: "image", jpg: "image", jpeg: "image" }[ext] || "file-code-2";
  }
  const zoneFiles = new WeakMap();   // zone → DataTransfer of valid files
  function zoneInput(zone) { return zone.querySelector("input[type=file]"); }
  function addFiles(zone, files) {
    const list = $(zone.dataset.dropzone); if (!list) return;
    const maxMb = +zone.dataset.maxMb || 25;
    const accept = (zone.dataset.accept || "").split(",").map(s => s.trim().toLowerCase()).filter(Boolean);
    const dt = zoneFiles.get(zone) || new DataTransfer(); zoneFiles.set(zone, dt);
    const d = list.dataset;
    list.querySelector(".file-empty")?.remove();
    Array.from(files).forEach(f => {
      if (Array.from(dt.files).some(x => x.name === f.name && x.size === f.size)) return;   // same file twice
      const ext = "." + f.name.split(".").pop().toLowerCase();
      const err = f.size > maxMb * 1048576 ? d.tTooBig : accept.length && !accept.includes(ext) ? d.tBadType : "";
      if (!err) dt.items.add(f);
      const li = document.createElement("li");
      li.className = "file-item" + (err ? " is-error" : "");
      li.dataset.file = err ? "" : f.name + "|" + f.size;
      li.innerHTML = `<span class="ibox ibox-sm"><i data-lucide="${err ? "file-x-2" : fileIcon(f.name)}" class="icon"></i></span>
        <div class="grow"><div class="file-name"></div><small class="subtle" data-meta></small></div>
        <button class="icon-btn" type="button" data-file-remove aria-label="${d.tRemove}"><i data-lucide="x" class="icon"></i></button>`;
      li.querySelector(".file-name").textContent = f.name;
      li.querySelector("[data-meta]").textContent = err ? `${fmtSize(f.size)} · ${err}` : `${fmtSize(f.size)} · ${d.tReady || ""}`;
      list.appendChild(li);
    });
    const input = zoneInput(zone); if (input) input.files = dt.files;
    updateFileCount(list); icons();
  }
  function updateFileCount(list) {
    const n = list.querySelectorAll(".file-item:not(.is-error)").length;
    $$("[data-files-count]").forEach(c => c.textContent = n);
    if (!list.querySelector(".file-item")) list.innerHTML = `<li class="file-empty subtle">${list.dataset.emptyText || ""}</li>`;
  }
  document.addEventListener("change", (e) => {
    const zone = e.target.closest("[data-dropzone]");
    if (zone && e.target.type === "file" && !e.target.dataset.syncing) {
      const picked = Array.from(e.target.files);
      e.target.files = (zoneFiles.get(zone) || new DataTransfer()).files;   // keep earlier picks
      addFiles(zone, picked);
    }
  });
  ["dragenter", "dragover"].forEach(ev => document.addEventListener(ev, (e) => {
    const zone = e.target.closest?.("[data-dropzone]"); if (!zone) return;
    e.preventDefault(); zone.classList.add("is-over");
  }));
  ["dragleave", "drop"].forEach(ev => document.addEventListener(ev, (e) => {
    const zone = e.target.closest?.("[data-dropzone]"); if (!zone) return;
    e.preventDefault(); zone.classList.remove("is-over");
    if (ev === "drop" && e.dataTransfer?.files?.length) addFiles(zone, e.dataTransfer.files);
  }));
  document.addEventListener("click", (e) => {
    const rm = e.target.closest("[data-file-remove]"); if (!rm) return;
    const item = rm.closest(".file-item"), list = rm.closest(".file-list");
    const zone = $$("[data-dropzone]").find(z => $(z.dataset.dropzone) === list);
    if (zone && item.dataset.file) {
      const keep = new DataTransfer();
      Array.from(zoneFiles.get(zone)?.files || []).filter(f => f.name + "|" + f.size !== item.dataset.file).forEach(f => keep.items.add(f));
      zoneFiles.set(zone, keep);
      const input = zoneInput(zone); if (input) input.files = keep.files;
    }
    item.remove(); updateFileCount(list);
  });
  document.addEventListener("submit", (e) => {
    const f = e.target;
    if (!f.matches("[data-upload]") || !window.FormData || !window.XMLHttpRequest) return;
    e.preventDefault();
    const btn = e.submitter || f.querySelector("[type=submit]");
    const bar = $(f.dataset.upload);
    $$("button", f).forEach(b => b.disabled = true);
    const data = new FormData(f);
    if (btn?.name) data.append(btn.name, btn.value);
    const xhr = new XMLHttpRequest();
    xhr.open("POST", f.action);
    xhr.upload.onprogress = (p) => { if (bar && p.lengthComputable) { bar.hidden = false; bar.style.setProperty("--value", Math.round(p.loaded * 100 / p.total) + "%"); } };
    xhr.onload = () => { location.href = xhr.responseURL || location.href; };
    xhr.onerror = () => { $$("button", f).forEach(b => b.disabled = false); toast(f.dataset.uploadError || "Upload failed", "alert-circle"); };
    xhr.send(data);
  });

  /* ---------- Character counter: <textarea maxlength data-counter="#out"> ---------- */
  function updateCounter(el) { const out = $(el.dataset.counter); if (out) out.textContent = `${el.value.length} / ${el.maxLength}`; }
  document.addEventListener("input", (e) => { if (e.target.matches?.("[data-counter]")) updateCounter(e.target); });

  /* ---------- Init ---------- */
  document.addEventListener("DOMContentLoaded", () => {
    icons();
    applyTheme(storedTheme());
    animateOnView();
    onScroll();
    $$("[data-counter]").forEach(updateCounter);
  });
})();
