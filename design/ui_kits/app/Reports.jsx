(() => {
const { PageHeader, Card, Tabs, Button, StatCard, Amount, DataTable, Badge, CategoryIcon, BarChart, Legend, DonutChart, Select, ProgressBar, Alert } = window.CashPrismDesignSystem_24fa3e;
function MonthReport({ k }) {
  const t = CP.monthTotals(k), p = CP.monthTotals(k - 1); const left = t.income - t.spend;
  const cats = CP.catTotals(k, k).filter(c => c.total > 0);
  const big = CP.spend.filter(b => CP.mk(b.date) === k).sort((a, b) => a.cents - b.cents).slice(0, 5);
  return <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--cp-grid-gap)' }}>
    <Card><div style={{ display: 'flex', gap: 20, alignItems: 'center', flexWrap: 'wrap' }}>
      <CategoryIcon icon="calendar" color="var(--cp-primary)" size={52} shape="round" />
      <div style={{ flex: 1, minWidth: 260 }}><div className="cp-label">Monatsbericht</div><h2 className="cp-h1" style={{ marginTop: 4 }}>{CP.monthLong(k)}</h2>
        <p className="cp-muted" style={{ margin: '8px 0 0', fontSize: 15, textWrap: 'pretty' }}>Du hast <b style={{ color: 'var(--cp-text)' }}>{CP.eur0(t.income)}</b> eingenommen und <b style={{ color: 'var(--cp-text)' }}>{CP.eur0(t.spend)}</b> ausgegeben. Am meisten Geld ging für <b style={{ color: 'var(--cp-text)' }}>{cats[0].name}</b> weg. Übrig geblieben {left >= 0 ? 'sind' : 'ist ein Minus von'} <b style={{ color: left >= 0 ? 'var(--cp-income)' : 'var(--cp-expense)' }}>{CP.eur0(Math.abs(left))}</b>.</p></div>
    </div></Card>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 'var(--cp-grid-gap)' }}>
      <StatCard icon="arrow-down-left" color="var(--cp-income)" label="Einnahmen" value={CP.eur0(t.income)} delta={CP.pct((t.income - p.income) / p.income)} deltaGood={t.income >= p.income} />
      <StatCard icon="arrow-up-right" color="var(--cp-expense)" label="Ausgaben" value={CP.eur0(t.spend)} delta={CP.pct((t.spend - p.spend) / p.spend)} deltaGood={t.spend <= p.spend} />
      <StatCard icon="piggy-bank" color="var(--cp-prism-teal)" label="Übrig" value={CP.eur0(left)} delta={Math.round(left / t.income * 100) + ' %'} deltaGood={left > 0} deltaLabel="gespart" />
    </div>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 340px), 1fr))', gap: 'var(--cp-grid-gap)' }}>
      <Card title="Ausgaben nach Kategorie">
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>{cats.map(c => { const pv = CP.catMonth(c.id, k - 1); const ch = pv ? (c.total - pv) / pv : 0; return <div key={c.id} style={{ display: 'grid', gridTemplateColumns: '28px 1fr auto', gap: '4px 12px', alignItems: 'center' }}>
          <CategoryIcon icon={c.icon} color={c.color} size={28} /><div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}><b className="cp-small">{c.name}</b><span className="cp-caption" style={{ fontWeight: 700, color: Math.abs(ch) < 0.05 ? 'var(--cp-text-3)' : ch > 0 ? 'var(--cp-expense)' : 'var(--cp-income)' }}>{CP.pct(ch)}</span></div><span className="cp-amount cp-small">{CP.eur0(c.total)}</span>
          <span /><ProgressBar value={c.total / cats[0].total} color={c.color} size="sm" /><span /></div>; })}</div>
      </Card>
      <Card title="Die fünf größten Ausgaben" padded={false}>
        <div style={{ marginTop: 10 }}><DataTable compact columns={[{ key: 'who', title: 'Empfänger', render: b => <b>{b.who}</b> }, { key: 'date', title: 'Datum', render: b => <span className="cp-muted cp-num">{CP.date(b.date)}</span> }, { key: 'cents', title: 'Betrag', align: 'right', render: b => <Amount cents={b.cents} /> }]} rows={big} /></div>
      </Card>
    </div>
  </div>;
}
function ContractsReport() {
  const list = CP.contracts(); const monthly = -list.reduce((s, c) => s + c.monthly, 0);
  return <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--cp-grid-gap)' }}>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 'var(--cp-grid-gap)' }}>
      <StatCard icon="repeat" color="var(--cp-prism-cyan)" label="Feste Kosten pro Monat" value={CP.eur(monthly)} />
      <StatCard icon="calendar-range" color="var(--cp-prism-violet)" label="Aufs Jahr gerechnet" value={CP.eur0(monthly * 12)} />
      <StatCard icon="receipt-text" color="var(--cp-prism-blue)" label="Verträge & Abos" value={list.length} />
    </div>
    {list.filter(c => c.change < 0).map(c => <Alert key={c.id} tone="warning" icon="trending-up" title={c.who + ' ist teurer geworden'}>Seit {CP.date(c.since)} zahlst du {CP.eur(c.cents)} statt {CP.eur(c.cents - c.change)} – das sind {CP.eur(-c.change * 12 / c.every)} mehr im Jahr.</Alert>)}
    <Card title="Alle regelmäßigen Zahlungen" subtitle="Von CashPrism an wiederkehrenden Buchungen erkannt" padded={false}>
      <div style={{ marginTop: 12 }}><DataTable columns={[
        { key: 'who', title: 'Vertrag', render: c => <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}><CategoryIcon icon={c.cat.icon} color={c.cat.color} size={34} /><div><b>{c.who}</b><div className="cp-caption cp-muted">{c.cat.name} · {c.account}</div></div></div> },
        { key: 'every', title: 'Rhythmus', render: c => <Badge>{c.every === 1 ? 'monatlich' : c.every === 3 ? 'vierteljährlich' : 'jährlich'}</Badge> },
        { key: 'next', title: 'Nächste Zahlung', render: c => <span className="cp-muted cp-num" style={{ fontWeight: 600 }}>{CP.date(c.next)}</span> },
        { key: 'change', title: '', render: c => c.change < 0 ? <Badge tone="warning" icon="trending-up">teurer</Badge> : null },
        { key: 'cents', title: 'Betrag', align: 'right', render: c => <Amount cents={c.cents} /> },
        { key: 'yearly', title: 'Pro Jahr', align: 'right', render: c => <span className="cp-amount cp-muted">{CP.eur0(-c.yearly)}</span> },
      ]} rows={list} /></div>
    </Card>
  </div>;
}
function YearReport({ y }) {
  const ks = CP.range(y * 12, y * 12 + 11).filter(k => k <= CP.CUR); const tt = ks.map(CP.monthTotals);
  const inc = tt.reduce((s, t) => s + t.income, 0), sp = tt.reduce((s, t) => s + t.spend, 0);
  const cats = CP.catTotals(ks[0], ks[ks.length - 1]).filter(c => c.total > 0);
  return <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--cp-grid-gap)' }}>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 'var(--cp-grid-gap)' }}>
      <StatCard icon="arrow-down-left" color="var(--cp-income)" label={'Einnahmen ' + y} value={CP.eur0(inc)} />
      <StatCard icon="arrow-up-right" color="var(--cp-expense)" label={'Ausgaben ' + y} value={CP.eur0(sp)} />
      <StatCard icon="piggy-bank" color="var(--cp-prism-teal)" label="Gespart" value={CP.eur0(inc - sp)} delta={Math.round((inc - sp) / inc * 100) + ' %'} deltaGood deltaLabel="deiner Einnahmen" />
    </div>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 360px), 1fr))', gap: 'var(--cp-grid-gap)' }}>
      <Card title="Das Jahr Monat für Monat" style={{ gridColumn: 'span 2' }} action={<Legend items={[{ label: 'Einnahmen', color: 'var(--cp-income)' }, { label: 'Ausgaben', color: 'var(--cp-expense)' }]} />}>
        <BarChart height={260} labels={ks.map(k => CP.MONTHS_LONG[k % 12].slice(0, 3))} format={CP.eur0} formatAxis={CP.axis} series={[{ name: 'Einnahmen', color: 'var(--cp-income)', values: tt.map(t => t.income) }, { name: 'Ausgaben', color: 'var(--cp-expense)', values: tt.map(t => t.spend) }]} />
      </Card>
      <Card title="Wofür im ganzen Jahr">
        <div style={{ display: 'flex', justifyContent: 'center' }}><DonutChart size={200} data={cats.map(c => ({ label: c.name, value: c.total, color: c.color }))} centerLabel="Ausgaben" centerValue={CP.eur0(sp)} format={CP.eur0} /></div>
        <Legend style={{ marginTop: 16 }} column items={cats.slice(0, 5).map(c => ({ label: c.name, color: c.color, value: CP.eur0(c.total) }))} />
      </Card>
    </div>
  </div>;
}
function Reports() {
  const [tab, setTab] = React.useState('month'); const [k, setK] = React.useState(String(CP.CUR)); const [y, setY] = React.useState('2025');
  const monthOpts = CP.range(CP.CUR - 11, CP.CUR).reverse().map(m => ({ value: String(m), label: CP.monthLong(m) }));
  return <div className="cp-page">
    <PageHeader title="Berichte" lead="Fertige Zusammenfassungen zum Nachlesen, Ausdrucken oder Weitergeben."
      actions={<>{tab === 'month' && <Select icon="calendar" value={k} onChange={setK} options={monthOpts} />}{tab === 'year' && <Select icon="calendar" value={y} onChange={setY} options={['2026', '2025']} />}<Button variant="secondary" icon="printer" onClick={() => window.print()}>Drucken</Button><Button variant="secondary" icon="download">Als PDF</Button></>} />
    <Tabs style={{ marginBottom: 'var(--cp-grid-gap)' }} value={tab} onChange={setTab} tabs={[{ value: 'month', label: 'Monatsbericht' }, { value: 'contracts', label: 'Verträge & Abos', count: CP.contracts().length }, { value: 'year', label: 'Jahresrückblick' }]} />
    {tab === 'month' ? <MonthReport k={+k} /> : tab === 'contracts' ? <ContractsReport /> : <YearReport y={+y} />}
  </div>;
}
Object.assign(window, { Reports });
})();
