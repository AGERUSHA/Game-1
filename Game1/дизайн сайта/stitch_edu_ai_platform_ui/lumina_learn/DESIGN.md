---
name: Lumina Learn
colors:
  surface: '#f9f9ff'
  surface-dim: '#d3daef'
  surface-bright: '#f9f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f1f3ff'
  surface-container: '#e9edff'
  surface-container-high: '#e1e8fd'
  surface-container-highest: '#dce2f7'
  on-surface: '#141b2b'
  on-surface-variant: '#474554'
  inverse-surface: '#293040'
  inverse-on-surface: '#edf0ff'
  outline: '#787586'
  outline-variant: '#c8c4d7'
  surface-tint: '#5847d2'
  primary: '#5341cd'
  on-primary: '#ffffff'
  primary-container: '#6c5ce7'
  on-primary-container: '#faf6ff'
  inverse-primary: '#c6bfff'
  secondary: '#5f5a7c'
  on-secondary: '#ffffff'
  secondary-container: '#dcd5fd'
  on-secondary-container: '#605b7d'
  tertiary: '#006274'
  on-tertiary: '#ffffff'
  tertiary-container: '#007d92'
  on-tertiary-container: '#edfaff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#e4dfff'
  primary-fixed-dim: '#c6bfff'
  on-primary-fixed: '#160066'
  on-primary-fixed-variant: '#4029ba'
  secondary-fixed: '#e5deff'
  secondary-fixed-dim: '#c8c2e9'
  on-secondary-fixed: '#1b1735'
  on-secondary-fixed-variant: '#474363'
  tertiary-fixed: '#acedff'
  tertiary-fixed-dim: '#4cd7f6'
  on-tertiary-fixed: '#001f26'
  on-tertiary-fixed-variant: '#004e5c'
  background: '#f9f9ff'
  on-background: '#141b2b'
  surface-variant: '#dce2f7'
typography:
  display-hero:
    fontFamily: Plus Jakarta Sans
    fontSize: 56px
    fontWeight: '800'
    lineHeight: 64px
    letterSpacing: -0.03em
  display-hero-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 36px
    fontWeight: '800'
    lineHeight: 44px
    letterSpacing: -0.02em
  headline-xl:
    fontFamily: Plus Jakarta Sans
    fontSize: 40px
    fontWeight: '700'
    lineHeight: 48px
    letterSpacing: -0.025em
  headline-xl-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 28px
    fontWeight: '700'
    lineHeight: 36px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
    letterSpacing: -0.015em
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.015em
  headline-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
    letterSpacing: -0.005em
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0em
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
    letterSpacing: 0em
  label-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.02em
  caption:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
    letterSpacing: 0.01em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-mobile: 1rem
  margin: 2rem
  margin-mobile: 1rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2rem
---

## Brand & Style

This design system embodies an intelligent, encouraging, and distraction-free learning ecosystem. It bridges institutional authority with playful gamified motivation, ensuring high clarity without descending into childish visual noise.

### Brand Archetype & Personality
- **Intelligent & Futuristic:** AI features feel seamless, perceptive, and proactive rather than gimmicky.
- **Supportive & Motivating:** Gamification mechanisms (streaks, mastery XP, skill nodes) provide delightful visual affirmations while preserving academic seriousness.
- **Calm & Focused:** Deep focus takes precedence. Interfaces minimize high-energy clutter, offering serene expanses of whitespace and crisp hierarchies.

### Aesthetic Movement: Contemporary Soft-Tech Minimal
The aesthetic blends **Modern Neomorphism-lite** (soft, tinted ambient shadows and elevated pure surfaces) with **Clean Editorial Tech**. High-contrast typography sits over airy, cool-tinted canvases, balanced by rich violet and lavender focal points that highlight progress, achievements, and generative AI feedback loops.

## Colors

The palette leverages high-order contrast ratios (exceeding WCAG AAA for text) anchored by an authoritative violet, accompanied by soft luminous surfaces and disciplined semantic indicators.

### Core Canvas & Structure
- **Base Canvas:** `#F8F9FE` — An ultra-light, cool-tinted white that isolates content effortlessly.
- **Elevated Canvas (Cards, Modals):** `#FFFFFF` — Pure white for crisp layering against the base canvas.
- **Borders & Dividers:** Thin structural delineation using `#E0E7FF` (tinted container borders) and `#E5E7EB` (neutral structural dividers).

### Primary & Accent Roles
- **Primary Brand (`#6C5CE7`):** Primary interactions, hero triggers, streak highlights, and AI synthesis banners.
- **Secondary Accent (`#DDD6FE` / `#F3E8FF`):** Soft lavender pill backings, progress bar tracks, and secondary badge states.
- **Deep Anchors (`#1E1B4B` / `#2E1065`):** High-impact headlines, critical metrics, and deep contrast elements.

### Feedback & Status Tokens
- **Success (Emerald):** `#10B981` (Mastery attained, correct answer, positive progress).
- **Warning (Amber):** `#F59E0B` (Approaching deadline, streak freeze alert).
- **Error (Rose):** `#EF4444` (Incorrect submission, failed connection).
- **Info / AI Synthesis (Cyan):** `#06B6D4` (Active tutor processing, contextual hints).

*Strict Rule: Muddy hues, warm yellow-tinted creams, and uncurated multi-colored gamification badges are prohibited. Gamification palettes must remain strictly unified through the violet and emerald spectrum.*

## Typography

The type system blends the geometric optimism of **Plus Jakarta Sans** for headlines, labels, and gamified progress tallies, with the neutral clarity of **Inter** for sustained reading, lesson transcripts, code examples, and conversational AI exchanges.

### Usage Guidance
- **Headlines:** Use tighter negative tracking (`-0.02em` to `-0.03em`) to deliver modern, compact editorial punch.
- **Body:** Standard neutral letter spacing with generous line heights (`1.5` to `1.55`) to facilitate fatigue-free reading during extensive study modules.
- **Labels & Metrics:** Slightly boosted letter spacing and `600` weight to guarantee legibility at small sizes on interactive dashboards.

## Layout & Spacing

The layout model is anchored on an 8pt spatial grid, ensuring rigorous visual cadence across responsive devices.

### Grid Architecture
- **Desktop (1200px+):** 12-column fluid grid. Max content containment at `1440px`. Column gutter: `24px` (`1.5rem`), outer canvas margin: `32px` (`2rem`).
- **Tablet (768px – 1199px):** 8-column fluid grid. Column gutter: `20px` (`1.25rem`), canvas margin: `24px` (`1.5rem`).
- **Mobile (320px – 767px):** 4-column fluid grid. Column gutter: `16px` (`1rem`), canvas margin: `16px` (`1rem`).

### Spatial Rhythm
- Internal card padding scales strictly between `space-lg` (desktop modules) and `space-md` (mobile surfaces).
- Micro-spacing (`space-xs`, `space-sm`) governs icon-to-label alignment, pill chips, and compact meta counters.

## Elevation & Depth

Visual hierarchy is maintained via crisp perimeter borders and soft, purple-tinted ambient luminescence, avoiding harsh black dropshadows.

### Layering Hierarchy
1. **Canvas Level (Floor):** Tinted base (`#F8F9FE`). Contains non-clickable backgrounds and secondary rail structures.
2. **Elevated Surface (Level 1):** Solid white cards (`#FFFFFF`) with a delicate boundary border `1px solid #E0E7FF` and ambient shadow:
   `box-shadow: 0 4px 20px -2px rgba(108, 92, 231, 0.05);`
3. **Interactive & Floating (Level 2 - Hover / Dropdowns):**
   `box-shadow: 0 10px 25px -5px rgba(108, 92, 231, 0.08), 0 8px 10px -6px rgba(108, 92, 231, 0.04);`
4. **Modals & AI Floating Prompts (Level 3):**
   `box-shadow: 0 20px 35px -10px rgba(30, 27, 75, 0.12), 0 1px 3px 0 rgba(0, 0, 0, 0.05);`

### Glass Accents (Targeted Only)
Sticky headers, filter bars, and contextual tutor heads use translucent white (`rgba(255, 255, 255, 0.82)`) with a `12px` backdrop blur (`backdrop-filter: blur(12px)`) and a bottom boundary `1px solid rgba(224, 231, 255, 0.6)`.

## Shapes

The geometric personality balances approachability with technical polish through intentional corner rounding:

- **Cards & Primary Modules:** `16px` to `24px` border radius, creating clean, friendly content grouping.
- **Interactive Controls (Inputs, Buttons):** `10px` to `14px` border radius, delivering high tactile appeal.
- **Pills & Status Indicators:** Full pill (`9999px`) reserved strictly for user streaks, difficulty tags, live statuses, and quick action chips.

## Components

### Buttons
- **Primary:** Solid `#6C5CE7` fill with white text, `12px` roundedness, font `label-md`. Subtle hover elevation with a shift to `#5B4BD6`. Focused state exhibits a `3px` lavender ring (`#DDD6FE`).
- **Secondary / Soft:** Soft lavender fill (`#F3E8FF`), rich purple text (`#6C5CE7`), zero shadow, transitioning to `#EDE5FF` on hover.
- **Ghost:** Transparent background, text `#1E1B4B`, border `1px solid #E5E7EB`.

### Cards & Learning Containers
- Built on `#FFFFFF` surfaces with `16px` or `20px` border radius.
- Enclosed with a crisp `1px solid #E0E7FF` border.
- Hover states on interactive cards elevate via `0 10px 25px -5px rgba(108, 92, 231, 0.08)` and border shifts to `#C7D2FE`.

### Input Fields & Search Bars
- Background `#FFFFFF` with `#E5E7EB` border, `12px` roundedness, internal padding `12px 16px`.
- Active focus state: border shifts to `#6C5CE7` with an ambient glow (`box-shadow: 0 0 0 4px rgba(108, 92, 231, 0.12)`).

### Chips & Badges
- **XP / Streak Badges:** Pill-shaped (`rounded-full`), `#F3E8FF` background with `#6C5CE7` text and paired micro-icon.
- **Status Tags:** Emerald `#ECFDF5` with `#065F46` label (Completed), Rose `#FEF2F2` with `#991B1B` label (Action Required).

### Checkboxes & Radio Controls
- Base: `20px` geometry with `6px` radius (checkboxes) and full circle (radios).
- Inactive: `#FFFFFF` with `1.5px solid #D1D5DB`.
- Selected: Solid `#6C5CE7` with crisp white SVG check/dot and an ambient micro-ring.

### Specialized EdTech Components
- **AI Tutor Inline Synthesizer:** Distinct gradient border treatment (`1px solid linear-gradient(135deg, #6C5CE7, #06B6D4)`), pale tinted background (`#FAF5FF`), cyan pulse typing-state indicator.
- **Mastery Progress Track:** `8px` container height, `#EEF2FF` track background, animated fill with `#6C5CE7`, ending with rounded caps.