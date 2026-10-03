# CashPrism Design System (v2)

**CashPrism — your Finanzguru data on a big screen.**

Finanzguru runs on your phone. Small screen, little visible at once, and you get the analyses Finanzguru decided to give you. CashPrism reads Finanzguru's data exports (`.xlsx`) and turns them into something you can analyse yourself — on a real monitor, with your own filters and charts. Your data stays with you: no cloud, no account, no signing up anywhere.

**Audience:** ordinary households, not finance people. Everything is written and drawn so someone who has never read a bank statement carefully can understand where their money goes.

**Product:** one self-hosted web app (runs on one machine at home, opened from any browser in the home network). Areas: Übersicht (dashboard), Buchungen (bookings), Konten (accounts), Analyse (analysis/charts), Berichte (reports), Import.

## Sources
- GitHub: **https://github.com/raisr/CashPrism** — the product code (Blazor + MudBlazor). Use it for product context: `README.md`, `ROADMAP.md`, `docs/ui.md`, `docs/finanzguru-export.md` (what data exists), `src/CashPrism.Web/Resources/Strings.resx` (current copy). **Its existing pages are placeholders and were deliberately not used as the visual reference** — the owner asked for a full redesign. Explore the repo to learn what data the export contains before designing new screens.
- `uploads/` — inspiration images: a light banking app (white sidebar, grey-blue content, uppercase field labels, soft cards, a selected account card filled in blue) and a dark finance dashboard (KPI tiles with round icon badges, area charts with gradient fills, donut with legend, insight rows). v2 blends both: light by default, dark as a true second theme.
- Fonts: Manrope and JetBrains Mono from https://github.com/google/fonts (SIL OFL, licences in `assets/fonts/`). Icons: Lucide (ISC) via `lucide-static@0.469.0`, bundled as an icon font in `assets/icons/`.

## Index
- `styles.css` — entry (imports only). Link this one file; set `data-theme="dark"` on `<html>` for dark.
- `tokens/` — `fonts.css`, `colors.css`, `typography.css`, `spacing.css`, `effects.css`, `base.css` (resets + text utility classes `cp-h1`, `cp-label`, `cp-muted`, `cp-num` …).
- `components/cashprism.css` — all component classes.
- `components/` — React components (see list). Each has `.jsx`, `.d.ts`, `.prompt.md`; one card per folder.
- `ui_kits/app/` — the interactive app: `index.html` plus one file per page (`dashboard.html`, `bookings.html`, `accounts.html`, `analysis.html`, `reports.html`, `import.html`).
- `guidelines/` — foundation cards. `assets/logo` (wordmark + icon in SVG/PNG/ICO), `assets/fonts`, `assets/icons`. `thumbnail.html`, `SKILL.md`, `github.md`.

### Components
- **core:** Icon, Button, IconButton, Badge, Card, CategoryIcon
- **forms:** Input, Select, Segmented, FilterChip, Switch, Dropzone
- **navigation:** Brand, Sidebar, Topbar, Tabs, PageHeader
- **data:** Amount, StatCard, DataTable, Pager, TransactionList, ProgressBar, Legend
- **charts:** AreaChart, BarChart, DonutChart, Sparkline
- **feedback:** Alert, Insight, EmptyState, Sheet, Spinner

No upstream component library defines this inventory anymore (the redesign replaces MudBlazor's look); the set is sized to what the six pages need.

---

## CONTENT FUNDAMENTALS
- **German UI, informal "du".** Every Finanzguru user reads German. Talk like a helpful friend who's good with numbers: *„So steht es um dein Geld im September.“*, *„Wofür ging dein Geld?“*, *„Hier geht am meisten hin.“*
- **No finance jargon.** Say *Einnahmen / Ausgaben / übrig geblieben*, not *Cashflow, Saldo, Allokation*. *Empfänger / Absender* instead of *Gegenseite*. *Verträge & Abos* instead of *wiederkehrende Lastschriften*. Technical words (Verwendungszweck, Lastschrift) only where the bank uses them, and only as secondary text.
- **Lead with the answer, then the detail.** Page leads are one sentence that already says something: *„Du hast 642 € mehr eingenommen als ausgegeben.“* Insights are concrete: *„Lebensmittel: 22 % mehr als sonst — 612 € im September, sonst etwa 500 € im Monat.“*
- **Calm, never alarming or cheery.** No exclamation marks except a short *„Fertig!“* after an import. No emoji. No guilt ("Du gibst zu viel aus") — state the fact and let the person judge.
- **Errors explain what to do:** *„Das ist keine Finanzguru-Datei. Wir brauchen den Export „Alle Buchungen“ als .xlsx-Datei.“* Duplicates are not errors: *„Diese Datei kennen wir schon.“*
- **Casing:** sentence case everywhere, including buttons (*Export hochladen*, *Alle ansehen*). Only the small field/section labels are uppercase via CSS (*KONTOSTAND*).
- **Numbers:** German formatting. Money `1.234,56 €`; signed when it's a booking (`+4.020,00 €`, `−43,18 €` with a real minus sign); round to whole euros in summaries and charts (`2.866 €`). Percent with a space: `22 %`. Dates `29.09.2026`; lists use friendly day headers (*Heute*, *Gestern*, *Mo, 29. September 2026*).
- **Privacy is a feature, said plainly:** *„Nur auf diesem Rechner. Kein Konto, keine Cloud.“*

## VISUAL FOUNDATIONS
- **Vibe:** calm, bright, modern household finance. Lots of air, rounded cards, one confident blue, and a colourful "prism" of category colours that makes charts readable at a glance.
- **Colour:** neutrals are cool greys (`#f2f4f8` page, white cards, `#e3e7ee` hairlines, ink `#131a2a`). Brand blue `#4a6cf7` for primary actions, active nav, focus, selected states. **Prism spectrum** (violet, blue, cyan, teal, green, amber, orange, rose, slate) is reserved for categories and chart series — each category keeps its colour everywhere. Money: income green `#12a150`, spending red `#e5484d`. Spending amounts in lists stay in ink (red would make every row alarming); red is used in charts and deltas.
- **Dark theme:** deep navy (`#0a0d16` page, `#111523` cards), lighter blue `#7b93ff`, brighter green/red; shadows are replaced by 1px borders.
- **Type:** Manrope everywhere (variable, bundled). Headlines heavy (700–800) with tight negative tracking; body 14/500. Money uses Manrope's tabular figures. JetBrains Mono only for IBANs, hashes, file names. Small uppercase labels (11/700, +0.08em) for field names and table headers — borrowed from the banking-app inspiration.
- **Layout:** 248px white sidebar (wordmark, grouped nav with counts, privacy note pinned bottom) + 68px top bar (section label, search, theme, notifications, Import). Content max 1440px, 28px padding, 20px grid gap; card grids use `repeat(auto-fit, minmax(…))` so they reflow down to phone width. Below 1040px the sidebar becomes an overlay.
- **Cards:** white, 1px line, 14px radius, very soft shadow (`0 1px 2px / 0 4px 16px −6px`). Header = h3 title + muted subtitle + optional right action. Tables and lists sit edge-to-edge inside cards.
- **Radii:** 8 controls, 10 icon tiles & nav items, 14 cards, 20 sheets, pills for chips/badges.
- **Category tiles:** rounded-square (or round for KPIs) tile, 14% tint of the category colour with the glyph in full colour. They anchor every booking row and KPI.
- **Charts:** smooth area lines with a vertical 24%→0 gradient fill; dashed light grid; axis text 11/600 grey; hover shows a crosshair and a floating white tooltip card. Bars have 4px rounded tops; stacked bars use the prism colours. Donuts are thick rings with a total in the middle; hovering a segment or legend row focuses it and dims the rest.
- **Backgrounds:** flat. Gradients appear only inside charts. No photos, illustrations or textures.
- **Hover:** 4% ink wash on rows, nav items, ghost buttons; primary darkens one step. **Press:** buttons nudge down 1px. **Focus:** 3px blue ring at 40% alpha. **Selected:** soft blue background (nav, table rows, chips) or a fully blue card (selected account, from the inspiration).
- **Motion:** quick and quiet — 120ms colour changes, 200ms switches, a 320ms slide-in for the detail sheet, fade-in for tooltips. No bouncing.
- **Transparency/blur:** only the dark scrim behind sheets and the mobile sidebar. No glassmorphism.

## ICONOGRAPHY
- **Lucide** (outline, 2px stroke, rounded joins), shipped as an icon font: `assets/icons/lucide.css` + `lucide.woff2` (lucide-static 0.469.0, ISC licence). Use `<Icon name="wallet"/>` or `<i class="cp-icon icon-wallet"></i>`.
- Default 20px; 18px in buttons and inputs; 16px in small buttons; glyph = 50% of a CategoryIcon tile.
- **Navigation:** layout-dashboard (Übersicht), receipt-text (Buchungen), wallet (Konten), chart-pie (Analyse), file-chart-column (Berichte), upload (Import).
- **Categories:** house Wohnen, shopping-basket Lebensmittel, car Mobilität, popcorn Freizeit & Essen, shopping-bag Shopping, repeat Verträge & Abos, shield Versicherungen, heart-pulse Gesundheit, circle-ellipsis Sonstiges, banknote Einkommen, arrow-left-right Umbuchung.
- **Accounts:** landmark (Giro), house (shared), piggy-bank (savings), credit-card.
- No emoji, no unicode symbols as icons, no hand-drawn SVGs. Charts are the only custom SVG.
- **Logo:** the wordmark *Cash*Prism in Manrope ExtraBold, −0.035em, "Prism" in brand blue; app icon = "CP" monogram on a blue rounded square. Files in `assets/logo/` (text converted to outlines, no font needed):
  - Wordmark SVG: `wordmark-color` (light bg), `wordmark-color-on-dark`, `wordmark-black`, `wordmark-white`, `wordmark-on-blue` (with background)
  - Icon SVG: `icon-blue` (primary), `icon-dark`, `icon-light`, `icon-mono` (transparent), `favicon.svg`
  - `png/`: wordmarks at 400/800/1600 px wide; icons at 16, 32, 48, 64, 180, 192, 512, 1024; `apple-touch-icon.png` (square, iOS masks it); `og-image-1200x630.png`
  - `favicon.ico` (16/32/48)
  - Clear space: at least the height of the "C" around the wordmark. Min size: wordmark 80px wide, icon 16px. Don't recolour "Prism" with anything but the brand blue (or white/black for mono).
