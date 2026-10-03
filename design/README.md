# Handoff: CashPrism v2 — visual redesign

## Overview
A full visual redesign of the CashPrism web app (`raisr/CashPrism`, Blazor Server + MudBlazor 9). It replaces the placeholder look of the current pages with a friendly household-finance UI aimed at ordinary people. It also adds the screens the roadmap (M5) plans: dashboard, bookings with search/filter, accounts, analysis/charts and reports.

## About the design files
Everything in this folder is a **design reference built in HTML/React**. The prototypes show the intended look and behaviour. They are **not production code**. Rebuild the designs in the existing Blazor app, using MudBlazor where it fits and small Razor components where it doesn't. Do **not** add React, npm or any CDN. The product rule still holds: *nothing loads from the internet*. Fonts and icons are bundled for that reason.

Open `ui_kits/app/index.html` in a browser to click through the design. It is self-contained apart from React/Babel from unpkg, which only the prototype needs.

## Fidelity
**High-fidelity.** Colours, type, spacing, radii, shadows, copy and interactions are final. Match them closely. Where MudBlazor's internal markup fights a value, override it in `app.css` (matching MudBlazor's selector specificity, as `app.css` already does) instead of dropping the value.

---

## 1. Foundations (do this first)

### 1.1 Fonts — local, no Google request
Copy `assets/fonts/Manrope-Variable.ttf` and `assets/fonts/JetBrainsMono-Variable.ttf` (both SIL OFL; the licence files sit beside them and must ship too, so add them to THIRD-PARTY-NOTICES) to `src/CashPrism.Web/wwwroot/fonts/`.

```css
/* wwwroot/app.css */
@font-face{font-family:"Manrope";src:url("fonts/Manrope-Variable.ttf") format("truetype");font-weight:200 800;font-display:swap}
@font-face{font-family:"JetBrains Mono";src:url("fonts/JetBrainsMono-Variable.ttf") format("truetype");font-weight:100 800;font-display:swap}
```
Then set `Typography.Default.FontFamily = ["Manrope","system-ui","Segoe UI","Helvetica","Arial","sans-serif"]` and `--cp-font-mono: "JetBrains Mono", ui-monospace, Consolas, monospace`.

### 1.2 Icons — Lucide icon font, local
Copy `assets/icons/lucide.css` and `assets/icons/lucide.woff2` (lucide-static 0.469.0, ISC licence) to `wwwroot/icons/` and link `icons/lucide.css` in `App.razor`. Add a tiny component:
```razor
@* Components/CpIcon.razor *@
<i class="cp-icon icon-@Name @Class" style="@(Size is null ? null : $"font-size:{Size}px")" aria-hidden="true"></i>
@code { [Parameter, EditorRequired] public string Name { get; set; } = ""; [Parameter] public int? Size { get; set; } [Parameter] public string? Class { get; set; } }
```
MudBlazor's `Icon=` parameters expect SVG paths. Where a Mud component needs an icon (MudIconButton, MudNavLink, adornments), either use the Lucide SVG path strings (`lucide-static/icons/*.svg`, copy the `<path>` content into a static `LucideIcons.cs`), or replace the component with a plain `<button class="cp-iconbtn">` + `CpIcon`. Pick one approach and use it everywhere. **The Material icons are no longer used.**

### 1.3 Theme — `Theme/CashPrismTheme.cs`
```csharp
PaletteLight = new PaletteLight {
  Primary = "#4a6cf7", PrimaryContrastText = "#ffffff", Secondary = "#5b6478",
  Background = "#f2f4f8", BackgroundGray = "#e9ecf2", Surface = "#ffffff",
  DrawerBackground = "#ffffff", DrawerText = "#5b6478", DrawerIcon = "#5b6478",
  AppbarBackground = "#ffffff", AppbarText = "#131a2a",
  TextPrimary = "#131a2a", TextSecondary = "#5b6478", TextDisabled = "#8b93a7",
  Divider = "#e3e7ee", DividerLight = "#e9ecf2", TableLines = "#e3e7ee", LinesDefault = "#e3e7ee", LinesInputs = "#e3e7ee",
  TableHover = "rgba(19,26,42,0.04)", ActionDefault = "#5b6478",
  Success = "#12a150", Error = "#e5484d", Warning = "#d97f06", Info = "#4a6cf7",
},
PaletteDark = new PaletteDark {
  Primary = "#7b93ff", PrimaryContrastText = "#0a0d16", Secondary = "#a3abc2",
  Background = "#0a0d16", BackgroundGray = "#1d2337", Surface = "#111523",
  DrawerBackground = "#111523", DrawerText = "#a3abc2", DrawerIcon = "#a3abc2",
  AppbarBackground = "#111523", AppbarText = "#eef1f8",
  TextPrimary = "#eef1f8", TextSecondary = "#a3abc2", TextDisabled = "#6f7891",
  Divider = "#252c44", DividerLight = "#1d2337", TableLines = "#252c44", LinesDefault = "#252c44", LinesInputs = "#252c44",
  TableHover = "rgba(255,255,255,0.05)", ActionDefault = "#a3abc2",
  Success = "#3ddc84", Error = "#ff6b6f", Warning = "#f5b544", Info = "#7b93ff",
},
LayoutProperties = new LayoutProperties { DefaultBorderRadius = "8px", AppbarHeight = "68px", DrawerWidthLeft = "248px" },
Typography = new Typography {
  Default = new DefaultTypography { FontFamily = [...Manrope stack...], FontSize = "0.875rem", FontWeight = "500", LineHeight = "1.5" },
  H4 = new H4Typography { FontSize = "1.625rem", FontWeight = "750", LineHeight = "1.2", LetterSpacing = "-0.025em" }, // page h1
  H5 = new H5Typography { FontSize = "1.125rem", FontWeight = "700", LineHeight = "1.3", LetterSpacing = "-0.015em" }, // h2
  H6 = new H6Typography { FontSize = "0.9375rem", FontWeight = "700", LineHeight = "1.4", LetterSpacing = "0" },     // card title
  Body1 = new Body1Typography { FontSize = "0.875rem", FontWeight = "500", LineHeight = "1.5" },
  Body2 = new Body2Typography { FontSize = "0.8125rem", FontWeight = "500", LineHeight = "1.45" },
  Caption = new CaptionTypography { FontSize = "0.75rem", FontWeight = "500", LineHeight = "1.4" },
  Overline = new OverlineTypography { FontSize = "0.6875rem", FontWeight = "700", LetterSpacing = "0.08em", TextTransform = "uppercase" },
  Button = new ButtonTypography { FontSize = "0.875rem", FontWeight = "700", TextTransform = "none", LetterSpacing = "-0.005em" },
},
```
Shadows: set every `Shadows.Elevation[1..3]` to `0 1px 2px rgba(19,26,42,.04), 0 4px 16px -6px rgba(19,26,42,.08)`, and use `Elevation="1"` on cards. In dark mode cards rely on the border, so the shadow can stay (it's nearly invisible on navy) or be cleared.

The theme switch stays as it is (follows the OS until clicked).

### 1.4 Tokens & component CSS
Copy `tokens/*.css` into `app.css`, or as separate files under `wwwroot/css/` linked from App.razor. They define every `--cp-*` variable with light values on `:root` and dark on `[data-theme="dark"]`. **MudBlazor toggles dark mode with its own class, not `data-theme`**. Either set `data-theme` on `<html>` from `MainLayout` via a JS interop one-liner whenever `isDarkMode` changes, or change the selector to `.mud-theme-dark` if your MudBlazor version emits it. Then port `components/cashprism.css`. It holds all component styles as `cp-*` classes. Keep the class names so the prototype and the app stay comparable.

### 1.5 Key values (full list in `tokens/`)
| Token | Light | Dark |
|---|---|---|
| Page bg | `#f2f4f8` | `#0a0d16` |
| Card / sidebar / top bar | `#ffffff` | `#111523` |
| Field fill | `#f7f8fb` | `#171c2d` |
| Line | `#e3e7ee` | `#252c44` |
| Line strong | `#d0d6e1` | `#323a57` |
| Text / 2 / 3 | `#131a2a` / `#5b6478` / `#8b93a7` | `#eef1f8` / `#a3abc2` / `#6f7891` |
| Primary / hover / soft | `#4a6cf7` / `#3b59e0` / `#eef2ff` | `#7b93ff` / `#93a6ff` / 16 % mix |
| Income / spending / warning | `#12a150` / `#e5484d` / `#d97f06` | `#3ddc84` / `#ff6b6f` / `#f5b544` |

**Category ("prism") colours — fixed per category, used everywhere (tiles, chart series, chips):**
Wohnen `#7c5cff` · Mobilität `#3d7bfd` · Verträge & Abos `#12b5d6` · Versicherungen `#14b39a` · Lebensmittel `#4cc35a` · Gesundheit `#f2b01e` · Shopping `#f47b2a` · Freizeit & Essen `#ec4d78` · Sonstiges/Umbuchung `#9aa3b5` · Einkommen = income green.
Icons: house, car, repeat, shield, shopping-basket, heart-pulse, shopping-bag, popcorn, circle-ellipsis / arrow-left-right, banknote. Finanzguru's own category names must be mapped onto these, so add a `CategoryStyle` lookup in Web, with a fallback for unknown names.

Radii: 6 · 8 (controls) · 10 (icon tiles, nav items) · 14 (cards) · 20 (sheet) · 999 (chips, badges).
Spacing: 4-px scale; card padding 20; grid gap 20; page padding 28 (16 under 700 px); content max 1440.
Type: Manrope; display 34/750/−0.03em; h1 26/750/−0.025em; h2 18/700/−0.015em; h3 15/700; body 14/500/1.5; small 13; caption 12; label 11/700/+0.08em uppercase colour text-3. Money always `font-variant-numeric: tabular-nums`.
Motion: 120 ms colour/background (`cubic-bezier(.2,0,0,1)`), 200 ms switch, 320 ms sheet slide-in (`cubic-bezier(.16,1,.3,1)`), tooltip fade 120 ms. Buttons move down 1px (`translateY(1px)`) while pressed. Focus ring `0 0 0 3px` primary at 40 %.

### 1.6 Money formatting (replace `BookingFormat.Amount`)
- Bookings: always signed, two decimals, **€ for EUR**, ISO code for others, real minus `−` (U+2212), NBSP before the symbol: `+4.020,00 €`, `−43,18 €`, `−4,00 USD`.
- Totals, KPIs and chart axes: unsigned, often whole euros: `2.866 €`; axis ≥ 10 000 € as `24 Tsd.`.
- Colour: income green; **spending stays ink in lists** (red only in charts and deltas, or where `colored` is set); KPI values always ink.
- Percent: `22 %` (with space), deltas signed `+8 %` / `−4 %`.
- Dates: `29.09.2026`; list group headers `Heute`, `Gestern`, `Mo, 29. September 2026`.

---

## 2. Component mapping
| Design system | Build in Blazor as | Notes |
|---|---|---|
| Button | `MudButton` Filled/Outlined/Text + `cp-btn` overrides | variants: primary, secondary (outlined, white), soft (primary-soft bg), ghost, danger. Height 40 / 32 sm / 48 lg, radius 8, 700 weight, sentence case |
| IconButton | `<button class="cp-iconbtn">` + CpIcon | 40×40 square radius 8, ghost or outlined; red 8 px `dot` |
| Icon | `CpIcon.razor` | Lucide font |
| Badge | `<span class="cp-badge">` | 24 px pill; tones success/danger/warning/info at 13 % tint |
| Card | `MudPaper Outlined Elevation=1 Class="cp-card"` or plain `<section class="cp-card">` | header: h3 + subtitle + action slot; `padded=false` for tables |
| CategoryIcon | `CategoryIcon.razor` | 36 px default, radius 10, bg = colour 14 % over surface, glyph 50 % size |
| Input / Select | `MudTextField` / `MudSelect` Variant.Outlined, or native + `cp-input` | 40 px, fill `#f7f8fb`, focus → white + ring |
| Segmented | `MudToggleGroup` styled as `cp-seg` | grey track, white selected pill |
| FilterChip | `MudChip` / `MudChipSet` styled `cp-chip` | 32 px pill; active = primary-soft + primary text + × |
| Switch | `MudSwitch` styled `cp-switch` | 38×22 |
| Dropzone | `MudFileUpload` with drag area, `cp-dropzone` | dashed 1.5 px, hover → primary-soft |
| Sidebar + Brand | `MudDrawer` (248 px) + custom nav buttons | sections "Auswerten", "Daten"; booking count pill; privacy note pinned bottom. **Collapsible:** quiet grey `panel-left-close` icon (32 px ghost button, text-3) at the top right beside the wordmark. Collapsing turns the bar into a 72 px icon rail: labels, counts and section names hide (sections become 1 px dividers), labels become tooltips, the footer becomes a lock icon, and the rail shows the "CP" monogram (40 px tile, primary-soft background, "C" in ink and "P" in primary, Manrope 800 16 px, −0.06em; same as `assets/logo/icon-light.svg`) with `panel-left-open` below it. Width animates 200 ms. Remember the choice in localStorage. Applies at ≥ 1040 px only; below that the overlay drawer stays |
| Topbar | `MudAppBar` 68 px | section label (11/700 caps, +0.12em), spacer, search (340 px), theme, bell, Import button |
| Tabs | `MudTabs` styled `cp-tabs` | 44 px, 2 px primary underline, count pill |
| PageHeader | update `PageFrame.razor` | h1 + lead (15 px text-2) + actions; 24 px bottom margin |
| Amount | `Amount.razor` | see 1.6 |
| StatCard | `StatCard.razor` | 44 px round icon, label 13/600, value 26/750, delta row with arrow-up-right / arrow-down-right |
| DataTable + Pager | `MudDataGrid` ServerData (keep server paging) restyled, or custom table | header 40 px uppercase label style; rows 56 px; day group rows 36 px on field fill; hover 4 % wash; pager "1–25 von 6.327 Buchungen" + numbered pages |
| TransactionList | `TransactionList.razor` | tile 38 + name 700 + meta caption + amount right |
| ProgressBar | `MudProgressLinear` restyled | 8 / 5 px, pill, colour per category |
| Legend | `Legend.razor` | 10 px rounded swatch (line swatch 14×3 for line series) |
| AreaChart, BarChart, DonutChart, Sparkline | **Port the SVG components to Razor** (each is ~60 lines in `components/charts/*.jsx`) | MudChart can't match the gradients, tooltips or rounding. Porting keeps it dependency-free and offline. Hover tooltip needs a small JS interop or `@onmousemove` with offsetX |
| Alert | `MudAlert` restyled, or `cp-alert` | tinted 8 % bg, 28 % border, icon in tone colour |
| Insight | `Insight.razor` | tile + title/sub + chevron; rows divided by line |
| EmptyState | `EmptyState.razor` | 56 px soft-primary icon square, h2, text max 380 px |
| Sheet | `MudDrawer Anchor.End Variant.Temporary` or `MudDialog` | floating 12 px from edges, 420 px, radius 20, Esc closes |
| Spinner | `MudProgressCircular` | |

Every component's props, variants and an example are in `components/<group>/<Name>.d.ts` and `<Name>.prompt.md`.

---

## 3. Screens
All screens share the **shell**: sidebar (248 px, white, right border) | column [top bar 68 px | scrolling `main`]. The page is `cp-page` (28 px padding, max 1440, centred). Grids use `repeat(auto-fit, minmax(min(100%, N px), 1fr))` with 20 px gap, so everything reflows down to phone width. Below 1040 px the sidebar is hidden and a menu button in the top bar opens it as an overlay with a scrim.

Navigation (sidebar): Übersicht `/` (layout-dashboard) · Buchungen `/bookings` (receipt-text, count pill) · Konten `/accounts` (wallet) · *Auswerten*: Analyse `/analysis` (chart-pie) · Berichte `/reports` (file-chart-column) · *Daten*: Import `/import` (upload).
Top-bar search: submitting jumps to Buchungen with the query applied.

### 3.1 Übersicht (`Dashboard.jsx`)
- **PageHeader:** "Guten Abend" (greeting by time of day) + lead "So steht es um dein Geld im {Monat}. Du hast **{Betrag}** mehr eingenommen als ausgegeben." (amount coloured income/spending). Action: Segmented 6 Monate / 12 Monate / 2 Jahre (chart span).
- **Row 1, four StatCards (min 230 px):** Vermögen auf allen Konten (wallet, primary, + 150×28 sparkline of 12 month-end totals); Einnahmen im {Monat} (arrow-down-left, income); Ausgaben im {Monat} (arrow-up-right, spending; delta good when lower); Übrig geblieben (piggy-bank, teal; delta = savings rate, label "deiner Einnahmen gespart").
- **Row 2:** AreaChart "Einnahmen und Ausgaben" (spans 2 columns, 280 px, income + spending series, legend in the card action), and a Donut card "Wofür ging dein Geld?": 190 px ring with top 5 categories + "Übrige", legend column beside it, both hover-linked; action "Analyse →".
- **Row 3:** "Letzte Buchungen" (7 rows, day-grouped TransactionList, action "Alle ansehen →") and "Aufgefallen" insights: a category above its 6-month average, contracts that got more expensive, the biggest single expense, and the savings rate compared with the month before.
- "Last month" = the last *complete* month. Transfers between own accounts are excluded from income and spending everywhere.

### 3.2 Buchungen (`Bookings.jsx`)
- Lead: "Alles, was auf deinen Konten passiert ist – {n} Buchungen seit {erster Monat}."
- One card containing: filter row (search input flex 260 px · account Select · period Select [Dieser Monat, Letzte 3 Monate, Dieses Jahr, Alles] · Segmented Alle/Einnahmen/Ausgaben); category chips row (horizontal scroll) with a "Umbuchungen ausblenden" switch on the right (default on); a summary strip on the field fill: "{n} Buchungen · Einnahmen +x · Ausgaben −y"; the table; the pager.
- Columns: Empfänger / Absender (tile + name + payment reference caption, clamped) · Kategorie (badge with a dot in the category colour) · Konto · Datum · Betrag (right). Sortable: name, date, amount. Without a sort, rows are grouped by day.
- A row click opens the **booking sheet**: tile + name + day label; big amount (34 px, faded cents); badges (booking type, "Regelmäßig", "Zwischen deinen Konten"); a key-value list (Datum, Konto, Kategorie, Empfänger/Absender, Verwendungszweck); "Frühere Buchungen bei {name}" (last 5). Footer buttons "Kategorie ändern" and "Ausblenden" are planned and not functional yet.
- No results: EmptyState "Nichts gefunden" + "Filter zurücksetzen".
- **Server-side:** filters and search must go into `IBookingReader` (extend `BookingPageRequest`), as `docs/ui.md` requires. Page size is 25.

### 3.3 Konten (`Accounts.jsx`)
- Lead: "Zusammen hast du **{Summe}** auf {n} Konten."
- Left column: one card per account (tile, name, bank, 56×24 sparkline, balance 20/750). The **selected card is filled primary** with white text and a coloured shadow (taken from the inspiration).
- Right (spans 2): header card with a 52 px round tile, name, bank · masked IBAN in mono, and "Kontostand" right-aligned (28 px). Tabs Überblick / Buchungen (count). Überblick shows the balance AreaChart over 13 months; below it, a "Rein und raus" grouped BarChart (6 months) and "Letzte 30 Tage" (in/out sums + 4 bookings).
- **Data:** needs a balance per account over time. Check `docs/finanzguru-export.md` for whether the export carries a running balance. Otherwise this needs a known start balance, or the page shows flows only.

### 3.4 Analyse (`Analysis.jsx`)
- Lead: "Wohin dein Geld geht – und wie sich das mit der Zeit verändert." Segmented 3 / 6 / 12 Monate.
- Stacked BarChart "Ausgaben nach Kategorie" (280 px) + category chips under it that isolate one category.
- "Kategorien im Vergleich" table (spans 2): category, share (ProgressBar + %), Verlauf sparkline, Veränderung badge (vs. the previous period of the same length; red when spending went up), Ø pro Monat.
- "Hier geht am meisten hin": top 8 recipients, ranked.
- "Monat für Monat übrig": teal bars of income minus spending, plus the average sentence.

### 3.5 Berichte (`Reports.jsx`)
- Actions: period Select (month or year depending on the tab), "Drucken" (window.print, so the page needs print CSS), "Als PDF".
- Tabs: **Monatsbericht** (summary sentence card, 3 StatCards, categories with change % and bars, the five biggest expenses) · **Verträge & Abos** (detects recurring bookings: same recipient + reference pattern, monthly/quarterly/yearly. Shows monthly/yearly cost StatCards, a warning Alert per price increase, and a table with rhythm, next date, a "teurer" badge, amount and yearly cost) · **Jahresrückblick** (StatCards, grouped monthly bars, year donut).

### 3.6 Import (`Import.jsx`)
- Lead: "Hol deine neuesten Buchungen aus Finanzguru. Jeder Import ergänzt, was schon da ist – nichts geht verloren."
- Dropzone card (spans 2) + "So geht's" with 3 numbered steps. While running, the dropzone becomes a spinner with "Wird eingelesen …" (keep the existing full-page lock behaviour from `docs/ui.md`). Results as Alert:
  - success "Fertig! {n} neue Buchungen sind da." + details
  - info "Diese Datei kennen wir schon" (already imported)
  - danger "Das ist keine Finanzguru-Datei" (+ existing reasons list, max 20)
- "Bisherige Importe" table: file (tile + name + "Exportiert am …"/"Datum unbekannt") · Eingelesen · Neu (success badge) · Aktualisiert · Zeilen gesamt.

---

## 4. Copy rules
German, informal "du", no finance jargon (Einnahmen/Ausgaben/übrig geblieben; Empfänger/Absender; Verträge & Abos). Sentence case for buttons. Calm tone: no exclamation marks except "Fertig!", no emoji. All strings go in `Strings.resx`, as before. The exact copy is in the prototype files; full rules are in `DESIGN_SYSTEM.md` → Content fundamentals.

## 5. Suggested build order
1. Fonts, icons, theme, tokens CSS, dark-mode attribute (§1).
2. Shell: sidebar, top bar, PageFrame → PageHeader.
3. Buchungen (data already exists): restyle the grid, add filters/search server-side, add the sheet.
4. Import restyle (behaviour unchanged).
5. Shared Razor parts: Amount, StatCard, CategoryIcon, Legend, charts.
6. Übersicht → Analyse → Konten → Berichte (these need aggregation queries in Application: monthly totals excluding transfers, category totals, recurring detection, balances).

## 6. Assets
- `assets/fonts/`: Manrope, JetBrains Mono (variable TTF, OFL)
- `assets/icons/`: Lucide icon font + CSS (ISC)
- `assets/logo/`: wordmark and "CP" icon as SVG/PNG, `favicon.ico`, `favicon.svg`, `apple-touch-icon.png`, `og-image`. Use `favicon.svg` and `favicon.ico` in App.razor; in the drawer, render the wordmark as text ("Cash" + `<span>Prism</span>`, 19/800/−0.035em) or as `wordmark-color.svg` / `-on-dark.svg`.

## 7. Files
- `design-system.html`: **offline reference of the whole design system** (all foundation, component and page cards, light/dark switch). Open it locally in a browser; only the component demos need internet for React/Babel from unpkg
- `screenshots/`: 12 PNGs of every page (light, plus 3 in dark) at 1440 px layout
- `ui_kits/app/index.html`: whole prototype (also `dashboard.html`, `bookings.html`, `accounts.html`, `analysis.html`, `reports.html`, `import.html`)
- `ui_kits/app/app.compiled.js`: the `.jsx` screens precompiled to plain JS, so the pages open straight from disk (`file://`). Read the `.jsx` files, which are identical in content
- `ui_kits/app/*.jsx`: screen code (layout, copy, behaviour); `data.js` = synthetic data + the aggregation helpers (good reference for the queries)
- `components/**`: component source, `.d.ts` props, `.prompt.md` usage; `components/cashprism.css` = all styles
- `tokens/*.css`, `styles.css`: design tokens
- `DESIGN_SYSTEM.md`: brand, content and visual rules
- `_ds_bundle.js`: compiled components so the prototypes run; not for production
