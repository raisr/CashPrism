# CashPrism app — UI kit (v2 redesign)

Interactive prototype of the full product vision, built from the design-system components and synthetic household data (`data.js`, Oct 2024 – Oct 2026, 4 accounts).

- `index.html` — the whole app; remembers page + theme. `dashboard.html`, `bookings.html`, `accounts.html`, `analysis.html`, `reports.html`, `import.html` open straight on one page.
- `Shell.jsx` sidebar + top bar (global search jumps to Buchungen) · `Dashboard.jsx` · `Bookings.jsx` (+ `BookingSheet`) · `Accounts.jsx` · `Analysis.jsx` · `Reports.jsx` (Monatsbericht, Verträge & Abos, Jahresrückblick) · `Import.jsx` · `App.jsx` router.

Try: hover any chart; click a booking row → detail sheet; filter Buchungen by category chip, account, period; pick another account; switch report tabs; drop an .xlsx on Import; toggle the moon icon.
Planned in the roadmap but not designed: login/password, settings, editing categories (buttons are visual only).
