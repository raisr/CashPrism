// Synthetic household data shaped like Finanzguru exports (Oct 2024 – 1 Oct 2026).
(function () {
  let seed = 4821; const r = () => (seed = (seed * 1664525 + 1013904223) % 4294967296) / 4294967296;
  const ri = (a, b) => Math.floor(a + r() * (b - a + 1)); const pick = a => a[Math.floor(r() * a.length)];
  const TODAY = new Date(2026, 9, 2, 18, 40); const LAST = new Date(2026, 9, 1, 23, 59).getTime();
  const CATS = {
    wohnen: { id: 'wohnen', name: 'Wohnen', icon: 'house', color: 'var(--cp-prism-violet)' },
    lebensmittel: { id: 'lebensmittel', name: 'Lebensmittel', icon: 'shopping-basket', color: 'var(--cp-prism-green)' },
    mobilitaet: { id: 'mobilitaet', name: 'Mobilität', icon: 'car', color: 'var(--cp-prism-blue)' },
    freizeit: { id: 'freizeit', name: 'Freizeit & Essen', icon: 'popcorn', color: 'var(--cp-prism-rose)' },
    shopping: { id: 'shopping', name: 'Shopping', icon: 'shopping-bag', color: 'var(--cp-prism-orange)' },
    vertraege: { id: 'vertraege', name: 'Verträge & Abos', icon: 'repeat', color: 'var(--cp-prism-cyan)' },
    versicherungen: { id: 'versicherungen', name: 'Versicherungen', icon: 'shield', color: 'var(--cp-prism-teal)' },
    gesundheit: { id: 'gesundheit', name: 'Gesundheit', icon: 'heart-pulse', color: 'var(--cp-prism-amber)' },
    sonstiges: { id: 'sonstiges', name: 'Sonstiges', icon: 'circle-ellipsis', color: 'var(--cp-prism-slate)' },
    einkommen: { id: 'einkommen', name: 'Einkommen', icon: 'banknote', color: 'var(--cp-income)' },
    umbuchung: { id: 'umbuchung', name: 'Umbuchung', icon: 'arrow-left-right', color: 'var(--cp-prism-slate)' },
  };
  const SPEND = ['wohnen', 'lebensmittel', 'freizeit', 'shopping', 'mobilitaet', 'vertraege', 'versicherungen', 'gesundheit', 'sonstiges'];
  const ACCOUNTS = [
    { id: 'giro', name: 'Girokonto', bank: 'Sparkasse Leipzig', iban: 'DE12 •••• •••• •••• 4821', icon: 'landmark', color: 'var(--cp-prism-blue)', start: 214000 },
    { id: 'gemeinsam', name: 'Gemeinschaftskonto', bank: 'Sparkasse Leipzig', iban: 'DE47 •••• •••• •••• 1093', icon: 'house', color: 'var(--cp-prism-violet)', start: 180000 },
    { id: 'tagesgeld', name: 'Tagesgeld', bank: 'Trade Republic', iban: 'DE88 •••• •••• •••• 6630', icon: 'piggy-bank', color: 'var(--cp-prism-teal)', start: 850000 },
    { id: 'kredit', name: 'Kreditkarte', bank: 'DKB Visa', iban: '•••• •••• •••• 7712', icon: 'credit-card', color: 'var(--cp-prism-orange)', start: 0 },
  ];
  const B = []; let id = 0;
  const add = (date, account, cat, who, ref, cents, kind, extra) => { if (date.getTime() > LAST) return; B.push({ id: 'b' + (id++), date: date.getTime(), account, cat, who, ref, cents, kind, ...extra }); };
  const transfer = (date, from, to, cents, ref) => { add(date, from, 'umbuchung', ACCOUNTS.find(a => a.id === to).name, ref, -cents, 'Überweisung', { transfer: true }); add(date, to, 'umbuchung', ACCOUNTS.find(a => a.id === from).name, ref, cents, 'Gutschrift', { transfer: true }); };
  const cardByMonth = {};
  for (let m = 0; m <= 24; m++) {
    const y = 2024 + Math.floor((9 + m) / 12), mo = (9 + m) % 12; const D = d => new Date(y, mo, d, ri(8, 20), ri(0, 59));
    const ym = y * 100 + mo + 1; const after = (yy, mm) => ym >= yy * 100 + mm;
    // fixed
    transfer(D(1), 'giro', 'gemeinsam', 140000, 'Haushalt ' + String(mo + 1).padStart(2, '0') + '/' + y);
    transfer(D(2), 'giro', 'tagesgeld', 30000, 'Sparplan Dauerauftrag');
    add(D(1), 'giro', 'versicherungen', 'HUK-COBURG', 'Kfz-Versicherung VS-Nr. 220-118734', -6140, 'Lastschrift', { contract: true });
    add(D(1), 'giro', 'mobilitaet', 'Deutsche Bahn', 'Deutschlandticket Abo', after(2025, 1) ? -5800 : -4900, 'Lastschrift', { contract: true });
    add(D(2), 'giro', 'gesundheit', 'FitX Studios', 'Mitgliedsbeitrag', -2499, 'Lastschrift', { contract: true });
    add(D(3), 'gemeinsam', 'wohnen', 'Hausverwaltung Berger', 'Miete Whg. 3.OG links inkl. NK', -115000, 'Dauerauftrag', { contract: true });
    add(D(5), 'giro', 'vertraege', 'congstar', 'Mobilfunk Rechnung ' + ri(10000000, 99999999), -2000, 'Lastschrift', { contract: true });
    add(D(7), 'kredit', 'vertraege', 'Spotify', 'Spotify Premium Duo', after(2026, 4) ? -1499 : -1299, 'Kartenzahlung', { contract: true });
    add(D(12), 'kredit', 'vertraege', 'Netflix', 'Netflix Standard', -1399, 'Kartenzahlung', { contract: true });
    add(D(15), 'gemeinsam', 'wohnen', 'Stadtwerke Leipzig', 'Abschlag Strom Vertragskonto 2004418833', after(2026, 1) ? -10200 : -9400, 'Lastschrift', { contract: true });
    add(D(20), 'gemeinsam', 'vertraege', 'Telekom', 'Festnetz + Internet MagentaZuhause M', -4495, 'Lastschrift', { contract: true });
    if (mo % 3 === 0) add(D(15), 'gemeinsam', 'wohnen', 'ARD ZDF Deutschlandradio', 'Rundfunkbeitrag Quartal', -5508, 'Lastschrift', { contract: true });
    if (mo === 1) add(D(10), 'giro', 'versicherungen', 'Allianz', 'Privathaftpflicht Jahresbeitrag', -6890, 'Lastschrift', { contract: true });
    if (mo % 3 === 2) add(D(28), 'tagesgeld', 'einkommen', 'Trade Republic', 'Zinsen Quartal', ri(5800, 8400), 'Gutschrift');
    add(new Date(y, mo, 28, 6, 10), 'giro', 'einkommen', 'Lindner Logistik GmbH', 'Lohn/Gehalt ' + String(mo + 1).padStart(2, '0') + '/' + y, after(2026, 3) ? 402000 : 385000, 'Gutschrift');
    if (mo === 10) add(D(28), 'giro', 'einkommen', 'Lindner Logistik GmbH', 'Weihnachtsgeld', 160000, 'Gutschrift');
    if (ym === 202507) add(D(18), 'giro', 'einkommen', 'Finanzamt Leipzig', 'Erstattung Einkommensteuer 2024', 74320, 'Gutschrift');
    // variable
    const groceryBoost = ym >= 202609 ? 1.22 : 1;
    for (let k = 0, n = ri(6, 9); k < n; k++) add(D(ri(1, 28)), 'gemeinsam', 'lebensmittel', 'REWE', 'REWE SAGT DANKE ' + ri(1000, 9999), -Math.round(ri(1800, 8600) * groceryBoost), 'Kartenzahlung');
    for (let k = 0, n = ri(2, 4); k < n; k++) add(D(ri(1, 28)), 'gemeinsam', 'lebensmittel', 'Lidl', 'LIDL DIENSTL. ' + ri(100, 999), -Math.round(ri(1200, 4800) * groceryBoost), 'Kartenzahlung');
    for (let k = 0, n = ri(3, 6); k < n; k++) add(D(ri(1, 28)), 'giro', 'lebensmittel', 'Bäckerei Wendl', 'Kartenzahlung', -ri(260, 980), 'Kartenzahlung');
    for (let k = 0, n = ri(1, 3); k < n; k++) add(D(ri(1, 28)), 'gemeinsam', 'shopping', 'dm-drogerie markt', 'dm Fil. ' + ri(1000, 2999), -ri(690, 3800), 'Kartenzahlung');
    for (let k = 0, n = ri(1, 3); k < n; k++) add(D(ri(1, 28)), 'kredit', 'shopping', 'Amazon', 'AMZN Mktp DE ' + ri(100, 999) + '-' + ri(1000000, 9999999), -ri(999, mo === 11 ? 18900 : 7900), 'Kartenzahlung');
    if (r() < 0.35) add(D(ri(1, 28)), 'kredit', 'shopping', 'Zalando', 'Zalando Bestellung ' + ri(10000000, 99999999), -ri(3995, 14900), 'Kartenzahlung');
    if (r() < 0.15 || mo === 2) add(D(ri(1, 28)), 'gemeinsam', 'shopping', 'IKEA', 'IKEA Leipzig', -ri(4900, 32900), 'Kartenzahlung');
    for (let k = 0, n = ri(2, mo === 7 ? 7 : 4); k < n; k++) { const p = pick([['Trattoria Da Enzo', 3800, 8900], ['Lieferando', 2200, 4600], ['Café Kowalski', 780, 2400], ['CineStar Kino', 2400, 3800]]); add(D(ri(1, 28)), pick(['giro', 'kredit']), 'freizeit', p[0], p[0] === 'Lieferando' ? 'Lieferando.de Bestellung' : 'Kartenzahlung', -ri(p[1], p[2]), 'Kartenzahlung'); }
    if (mo === 7) add(D(4), 'gemeinsam', 'freizeit', 'Ferienhaus Ostseeblick', 'Anzahlung Ferienhaus Zingst', -89000, 'Überweisung');
    for (let k = 0, n = ri(1, 3); k < n; k++) add(D(ri(1, 28)), 'giro', 'mobilitaet', 'Aral', 'ARAL Tankstelle ' + ri(100, 999), -ri(4200, 7800), 'Kartenzahlung');
    if (r() < 0.4) add(D(ri(1, 28)), 'kredit', 'mobilitaet', 'Deutsche Bahn', 'DB Fernverkehr Ticket ' + ri(100000, 999999), -ri(2990, 11990), 'Kartenzahlung');
    if (r() < 0.6) add(D(ri(1, 28)), 'giro', 'gesundheit', 'Apotheke am Markt', 'Kartenzahlung', -ri(450, 3600), 'Kartenzahlung');
    if (r() < 0.7) add(D(ri(1, 28)), 'giro', 'sonstiges', 'Geldautomat', 'Bargeldauszahlung', -pick([5000, 10000, 10000, 15000]), 'Auszahlung');
  }
  // credit card settled on the 22nd of the next month from Girokonto
  B.filter(b => b.account === 'kredit' && !b.transfer).forEach(b => { const d = new Date(b.date); const k = d.getFullYear() * 12 + d.getMonth(); cardByMonth[k] = (cardByMonth[k] || 0) - b.cents; });
  Object.entries(cardByMonth).forEach(([k, v]) => { const y = Math.floor((+k + 1) / 12), mo = (+k + 1) % 12; transfer(new Date(y, mo, 22, 9, 0), 'giro', 'kredit', v, 'Kreditkartenabrechnung'); });
  B.sort((a, b) => b.date - a.date);
  // balances
  ACCOUNTS.forEach(a => { a.balance = a.start + B.filter(b => b.account === a.id).reduce((s, b) => s + b.cents, 0); });

  // ---------- helpers ----------
  const MONTHS = ['Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'];
  const MONTHS_LONG = ['Januar', 'Februar', 'März', 'April', 'Mai', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'];
  const DAYS = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];
  const mk = t => { const d = new Date(t); return d.getFullYear() * 12 + d.getMonth(); };
  const CUR = 2026 * 12 + 8; // September 2026 = last full month
  const monthLabel = k => MONTHS[k % 12] + ' ' + String(Math.floor(k / 12)).slice(2);
  const monthLong = k => MONTHS_LONG[k % 12] + ' ' + Math.floor(k / 12);
  const eur = (c, o = {}) => { const d = o.decimals ?? 2; const s = o.sign ? (c > 0 ? '+' : c < 0 ? '−' : '') : (c < 0 ? '−' : ''); return s + (Math.abs(c) / 100).toLocaleString('de-DE', { minimumFractionDigits: d, maximumFractionDigits: d }) + '\u00a0€'; };
  const eur0 = c => eur(c, { decimals: 0 });
  const axis = c => { const e = c / 100; return Math.abs(e) >= 10000 ? (e / 1000).toLocaleString('de-DE', { maximumFractionDigits: 0 }) + ' Tsd.' : Math.round(e).toLocaleString('de-DE') + ' €'; };
  const pct = v => (v > 0 ? '+' : v < 0 ? '−' : '') + Math.abs(Math.round(v * 100)).toLocaleString('de-DE') + ' %';
  const dayLabel = t => { const d = new Date(t); const t0 = new Date(TODAY.getFullYear(), TODAY.getMonth(), TODAY.getDate()).getTime(); const dd = new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime(); const diff = Math.round((t0 - dd) / 864e5); if (diff === 0) return 'Heute'; if (diff === 1) return 'Gestern'; return DAYS[d.getDay()] + ', ' + d.getDate() + '. ' + MONTHS_LONG[d.getMonth()] + ' ' + d.getFullYear(); };
  const date = t => new Date(t).toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit', year: 'numeric' });
  const spend = B.filter(b => !b.transfer && b.cents < 0); const income = B.filter(b => !b.transfer && b.cents > 0);
  const range = (from, to) => { const a = []; for (let k = from; k <= to; k++) a.push(k); return a; };
  const sumBy = (list, f) => list.reduce((s, b) => s + (f ? f(b) : b.cents), 0);
  const monthTotals = k => ({ income: sumBy(income.filter(b => mk(b.date) === k)), spend: -sumBy(spend.filter(b => mk(b.date) === k)) });
  const catTotals = (k0, k1) => SPEND.map(c => ({ ...CATS[c], total: -sumBy(spend.filter(b => b.cat === c && mk(b.date) >= k0 && mk(b.date) <= k1)) })).sort((a, b) => b.total - a.total);
  const catMonth = (c, k) => -sumBy(spend.filter(b => b.cat === c && mk(b.date) === k));
  const balanceAt = (accId, k) => { const acc = ACCOUNTS.filter(a => !accId || a.id === accId); return acc.reduce((s, a) => s + a.start + sumBy(B.filter(b => b.account === a.id && mk(b.date) <= k)), 0); };
  const toTx = b => ({ id: b.id, title: b.who, meta: CATS[b.cat].name + ' · ' + ACCOUNTS.find(a => a.id === b.account).name, icon: CATS[b.cat].icon, color: CATS[b.cat].color, cents: b.cents, day: dayLabel(b.date), raw: b });
  const contracts = () => { const map = {}; B.filter(b => b.contract).forEach(b => { const key = b.who + '|' + b.ref.replace(/\d{5,}/g, ''); (map[key] = map[key] || []).push(b); });
    return Object.values(map).map(list => { list.sort((a, b) => b.date - a.date); const last = list[0], prev = list.find(b => b.cents !== last.cents); const gaps = list.length > 1 ? (list[0].date - list[list.length - 1].date) / (list.length - 1) / 864e5 : 30; const every = gaps > 300 ? 12 : gaps > 80 ? 3 : 1; const nd = new Date(last.date); nd.setMonth(nd.getMonth() + every);
      return { id: last.id, who: last.who, ref: last.ref, cat: CATS[last.cat], account: ACCOUNTS.find(a => a.id === last.account).name, cents: last.cents, every, monthly: Math.round(last.cents / every), yearly: Math.round(last.cents * 12 / every), next: nd.getTime(), change: prev && list.indexOf(prev) < 8 ? last.cents - prev.cents : 0, since: prev && list.indexOf(prev) < 8 ? list[list.indexOf(prev) - 1].date : null, count: list.length }; })
      .sort((a, b) => a.monthly - b.monthly); };
  const IMPORTS = [
    { id: 'i4', at: new Date(2026, 9, 1, 19, 42).getTime(), file: 'Finanzguru_Alle_Buchungen_20261001.xlsx', exported: new Date(2026, 9, 1).getTime(), rows: B.length, inserted: 46, updated: 3 },
    { id: 'i3', at: new Date(2026, 8, 2, 8, 15).getTime(), file: 'Finanzguru_Alle_Buchungen_20260901.xlsx', exported: new Date(2026, 8, 1).getTime(), rows: B.length - 46, inserted: 51, updated: 0 },
    { id: 'i2', at: new Date(2026, 7, 3, 21, 3).getTime(), file: 'Export August (Kopie).xlsx', exported: new Date(2026, 7, 2).getTime(), rows: B.length - 97, inserted: 49, updated: 7 },
    { id: 'i1', at: new Date(2026, 6, 4, 20, 11).getTime(), file: 'alle_buchungen.xlsx', exported: null, rows: B.length - 146, inserted: B.length - 146, updated: 0 },
  ];
  window.CP = { TODAY, CATS, SPEND, ACCOUNTS, BOOKINGS: B, IMPORTS, CUR, mk, monthLabel, monthLong, MONTHS_LONG, eur, eur0, axis, pct, dayLabel, date, range, sumBy, spend, income, monthTotals, catTotals, catMonth, balanceAt, toTx, contracts };
})();
