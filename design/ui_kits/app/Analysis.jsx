(() => {
const { PageHeader, Card, BarChart, Legend, Segmented, DataTable, ProgressBar, Sparkline, Badge, CategoryIcon, Amount, AreaChart } = window.CashPrismDesignSystem_24fa3e;
function Analysis({ onNavigate }) {
  const [span, setSpan] = React.useState('6'); const [focus, setFocus] = React.useState(null);
  const K = CP.CUR, months = CP.range(K - (+span) + 1, K);
  const cats = CP.catTotals(months[0], K);
  const total = cats.reduce((s, c) => s + c.total, 0);
  const shown = focus ? cats.filter(c => c.id === focus) : cats;
  const rows = cats.map(c => { const vals = months.map(k => CP.catMonth(c.id, k)); const avg = c.total / months.length; const prevAvg = CP.range(months[0] - months.length, months[0] - 1).reduce((s, k) => s + CP.catMonth(c.id, k), 0) / months.length; return { ...c, id: c.id, vals, avg, share: c.total / total, change: prevAvg ? (avg - prevAvg) / prevAvg : 0 }; });
  const merchants = Object.values(CP.spend.filter(b => CP.mk(b.date) >= months[0]).reduce((m, b) => { (m[b.who] = m[b.who] || { who: b.who, cat: b.cat, total: 0, n: 0 }); m[b.who].total -= b.cents; m[b.who].n++; return m; }, {})).sort((a, b) => b.total - a.total).slice(0, 8);
  const left = months.map(k => { const t = CP.monthTotals(k); return t.income - t.spend; });
  return <div className="cp-page">
    <PageHeader title="Analyse" lead="Wohin dein Geld geht – und wie sich das mit der Zeit verändert." actions={<Segmented value={span} onChange={setSpan} options={[{ value: '3', label: '3 Monate' }, { value: '6', label: '6 Monate' }, { value: '12', label: '12 Monate' }]} />} />
    <Card title="Ausgaben nach Kategorie" subtitle={'Ø ' + CP.eur0(total / months.length) + ' pro Monat · Klick auf eine Kategorie, um nur sie zu sehen'}>
      <BarChart stacked height={280} labels={months.map(CP.monthLabel)} format={CP.eur0} formatAxis={CP.axis} series={shown.map(c => ({ name: c.name, color: c.color, values: months.map(k => CP.catMonth(c.id, k)) }))} />
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, marginTop: 16 }}>
        {cats.map(c => <button key={c.id} type="button" className={'cp-chip' + (focus === c.id ? ' cp-chip--on' : '')} onClick={() => setFocus(focus === c.id ? null : c.id)} style={focus && focus !== c.id ? { opacity: .55 } : undefined}><span style={{ width: 8, height: 8, borderRadius: 2, background: c.color }} />{c.name}</button>)}
      </div>
    </Card>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 360px), 1fr))', gap: 'var(--cp-grid-gap)', marginTop: 'var(--cp-grid-gap)' }}>
      <Card title="Kategorien im Vergleich" subtitle="Durchschnitt pro Monat, verglichen mit dem Zeitraum davor" padded={false} style={{ gridColumn: 'span 2' }}>
        <div style={{ marginTop: 12 }}><DataTable rowKey="id" onRowClick={r => setFocus(r.id)} columns={[
          { key: 'name', title: 'Kategorie', render: r => <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}><CategoryIcon icon={r.icon} color={r.color} size={34} /><b>{r.name}</b></div> },
          { key: 'share', title: 'Anteil', render: r => <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 140 }}><ProgressBar value={r.share} color={r.color} size="sm" style={{ flex: 1 }} /><span className="cp-small cp-muted cp-num" style={{ width: 36, textAlign: 'right', fontWeight: 700 }}>{Math.round(r.share * 100)} %</span></div> },
          { key: 'trend', title: 'Verlauf', render: r => <Sparkline values={r.vals} color={r.color} width={90} height={26} /> },
          { key: 'change', title: 'Veränderung', render: r => <Badge tone={Math.abs(r.change) < 0.05 ? 'neutral' : r.change > 0 ? 'danger' : 'success'} icon={r.change > 0 ? 'trending-up' : 'trending-down'}>{CP.pct(r.change)}</Badge> },
          { key: 'avg', title: 'Ø pro Monat', align: 'right', render: r => <span className="cp-amount">{CP.eur0(r.avg)}</span> },
        ]} rows={rows} /></div>
      </Card>
      <Card title="Hier geht am meisten hin" subtitle={'Deine größten Empfänger, ' + months.length + ' Monate'} padded={false}>
        <div style={{ padding: '8px 0' }}>{merchants.map((m, i) => { const c = CP.CATS[m.cat]; return <div key={m.who} className="cp-tx" style={{ cursor: 'default' }}>
          <span className="cp-small cp-faint cp-num" style={{ width: 16, fontWeight: 700 }}>{i + 1}</span><CategoryIcon icon={c.icon} color={c.color} size={34} />
          <div className="cp-tx__main"><div className="cp-tx__title">{m.who}</div><div className="cp-tx__meta">{m.n} Buchungen · {c.name}</div></div>
          <span className="cp-amount">{CP.eur0(m.total)}</span></div>; })}</div>
      </Card>
      <Card title="Monat für Monat übrig" subtitle="Einnahmen minus Ausgaben">
        <BarChart height={240} labels={months.map(CP.monthLabel)} format={CP.eur0} formatAxis={CP.axis} series={[{ name: 'Übrig', color: 'var(--cp-prism-teal)', values: left.map(v => Math.max(0, v)) }]} />
        <div className="cp-small cp-muted" style={{ marginTop: 12 }}>Im Schnitt bleiben dir <b style={{ color: 'var(--cp-text)' }}>{CP.eur0(left.reduce((a, b) => a + b, 0) / left.length)}</b> pro Monat.</div>
      </Card>
    </div>
  </div>;
}
Object.assign(window, { Analysis });
})();
