(() => {
const { PageHeader, StatCard, Amount, Card, AreaChart, DonutChart, Legend, TransactionList, Insight, Button, Segmented, Sparkline } = window.CashPrismDesignSystem_24fa3e;
function Dashboard({ onNavigate, onOpenBooking }) {
  const [span, setSpan] = React.useState('12');
  const K = CP.CUR, months = CP.range(K - (+span) + 1, K);
  const cur = CP.monthTotals(K), prev = CP.monthTotals(K - 1);
  const net = CP.ACCOUNTS.reduce((s, a) => s + a.balance, 0);
  const netHist = CP.range(K - 11, K).map(k => CP.balanceAt(null, k));
  const left = cur.income - cur.spend, leftPrev = prev.income - prev.spend;
  const cats = CP.catTotals(K, K); const top = cats.slice(0, 5); const rest = cats.slice(5).reduce((s, c) => s + c.total, 0);
  const donut = [...top.map(c => ({ label: c.name, value: c.total, color: c.color })), { label: 'Übrige', value: rest, color: 'var(--cp-prism-slate)' }];
  const [hi, setHi] = React.useState(null);
  const recent = CP.BOOKINGS.filter(b => !b.transfer).slice(0, 7).map(CP.toTx);
  // insights
  const avg = c => CP.range(K - 6, K - 1).reduce((s, k) => s + CP.catMonth(c, k), 0) / 6;
  const groc = CP.catMonth('lebensmittel', K), grocAvg = avg('lebensmittel');
  const priceUp = CP.contracts().filter(c => c.change < 0);
  const biggest = CP.spend.filter(b => CP.mk(b.date) === K).sort((a, b) => a.cents - b.cents)[0];
  return <div className="cp-page">
    <PageHeader title="Guten Abend" lead={<>So steht es um dein Geld im {CP.monthLong(K)}. Du hast <b style={{ color: left >= 0 ? 'var(--cp-income)' : 'var(--cp-expense)' }}>{CP.eur0(left)}</b> mehr eingenommen als ausgegeben.</>}
      actions={<Segmented options={[{ value: '6', label: '6 Monate' }, { value: '12', label: '12 Monate' }, { value: '24', label: '2 Jahre' }]} value={span} onChange={setSpan} />} />
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(230px, 1fr))', gap: 'var(--cp-grid-gap)' }}>
      <StatCard icon="wallet" color="var(--cp-primary)" label="Vermögen auf allen Konten" value={<Amount cents={net} sign={false} dimCents />} delta={CP.pct((net - netHist[10]) / netHist[10])} deltaGood={net >= netHist[10]} footer={<div style={{ marginTop: 10 }}><Sparkline values={netHist} width={150} height={28} /></div>} />
      <StatCard icon="arrow-down-left" color="var(--cp-income)" label={'Einnahmen im ' + CP.MONTHS_LONG[K % 12]} value={<Amount cents={cur.income} sign={false} dimCents />} delta={CP.pct((cur.income - prev.income) / prev.income)} deltaGood={cur.income >= prev.income} />
      <StatCard icon="arrow-up-right" color="var(--cp-expense)" label={'Ausgaben im ' + CP.MONTHS_LONG[K % 12]} value={<Amount cents={cur.spend} sign={false} dimCents />} delta={CP.pct((cur.spend - prev.spend) / prev.spend)} deltaGood={cur.spend <= prev.spend} />
      <StatCard icon="piggy-bank" color="var(--cp-prism-teal)" label="Übrig geblieben" value={<Amount cents={left} sign={false} dimCents />} delta={Math.round(left / cur.income * 100) + ' %'} deltaGood={left > 0} deltaLabel="deiner Einnahmen gespart" />
    </div>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 420px), 1fr))', gap: 'var(--cp-grid-gap)', marginTop: 'var(--cp-grid-gap)' }}>
      <Card title="Einnahmen und Ausgaben" subtitle="Pro Monat, ohne Umbuchungen zwischen deinen Konten" style={{ gridColumn: 'span 2' }} action={<Legend line items={[{ label: 'Einnahmen', color: 'var(--cp-income)' }, { label: 'Ausgaben', color: 'var(--cp-expense)' }]} />}>
        <AreaChart height={280} labels={months.map(CP.monthLabel)} tooltipTitle={i => CP.monthLong(months[i])} format={CP.eur} formatAxis={CP.axis}
          series={[{ name: 'Einnahmen', color: 'var(--cp-income)', values: months.map(k => CP.monthTotals(k).income) }, { name: 'Ausgaben', color: 'var(--cp-expense)', values: months.map(k => CP.monthTotals(k).spend) }]} />
      </Card>
      <Card title="Wofür ging dein Geld?" subtitle={CP.monthLong(K)} action={<Button variant="ghost" size="sm" iconEnd="arrow-right" onClick={() => onNavigate('/analysis')}>Analyse</Button>}>
        <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 24, justifyContent: 'center' }}>
          <DonutChart data={donut} size={190} centerLabel="Ausgaben" centerValue={CP.eur0(cur.spend)} format={CP.eur0} active={hi} onActiveChange={setHi} />
          <Legend column onHover={setHi} style={{ flex: '1 1 180px', minWidth: 180 }} items={donut.map(d => ({ label: d.label, color: d.color, value: CP.eur0(d.value) }))} />
        </div>
      </Card>
    </div>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 380px), 1fr))', gap: 'var(--cp-grid-gap)', marginTop: 'var(--cp-grid-gap)' }}>
      <Card title="Letzte Buchungen" padded={false} action={<Button variant="ghost" size="sm" iconEnd="arrow-right" onClick={() => onNavigate('/bookings')}>Alle ansehen</Button>}>
        <div style={{ paddingBottom: 8 }}><TransactionList groupByDay items={recent} onSelect={t => onOpenBooking(t.raw)} /></div>
      </Card>
      <Card title="Aufgefallen" subtitle="Was sich bei dir verändert hat" padded={false}>
        <div style={{ paddingTop: 10, paddingBottom: 6 }}>
          <Insight icon="shopping-basket" color="var(--cp-prism-green)" title={'Lebensmittel: ' + CP.pct((groc - grocAvg) / grocAvg).replace('+', '') + ' mehr als sonst'} sub={CP.eur0(groc) + ' im ' + CP.MONTHS_LONG[K % 12] + ', sonst etwa ' + CP.eur0(grocAvg) + ' im Monat'} onClick={() => onNavigate('/analysis')} />
          {priceUp.slice(0, 2).map(c => <Insight key={c.id} icon="repeat" color="var(--cp-prism-cyan)" title={c.who + ' kostet jetzt ' + CP.eur(-c.change) + ' mehr'} sub={'Seit ' + CP.MONTHS_LONG[new Date(c.since).getMonth()] + ' ' + new Date(c.since).getFullYear() + ' · ' + CP.eur(-c.yearly) + ' im Jahr'} onClick={() => onNavigate('/reports')} />)}
          {biggest && <Insight icon="receipt-text" color="var(--cp-prism-violet)" title={'Größte Ausgabe: ' + biggest.who} sub={CP.eur(biggest.cents) + ' am ' + CP.date(biggest.date)} onClick={() => onOpenBooking(biggest)} />}
          <Insight icon="piggy-bank" color="var(--cp-prism-teal)" title={'Du sparst ' + Math.round(left / cur.income * 100) + ' % deiner Einnahmen'} sub={'Im Vormonat waren es ' + Math.round(leftPrev / prev.income * 100) + ' %'} onClick={() => onNavigate('/reports')} />
        </div>
      </Card>
    </div>
  </div>;
}
Object.assign(window, { Dashboard });
})();
