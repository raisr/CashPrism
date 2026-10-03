(() => {
const { PageHeader, Card, StatCard, Amount, AreaChart, Tabs, TransactionList, CategoryIcon, Icon, Badge, Sparkline, BarChart, Legend } = window.CashPrismDesignSystem_24fa3e;
function AccountCard({ a, on, onClick }) {
  const K = CP.CUR; const hist = CP.range(K - 5, K + 1).map(k => CP.balanceAt(a.id, k));
  return <button type="button" onClick={onClick} style={{ all: 'unset', boxSizing: 'border-box', cursor: 'pointer', display: 'block', width: '100%', padding: 16, borderRadius: 'var(--cp-radius-lg)', background: on ? 'var(--cp-primary)' : 'var(--cp-surface)', color: on ? 'var(--cp-on-primary)' : 'var(--cp-text)', border: '1px solid ' + (on ? 'var(--cp-primary)' : 'var(--cp-line)'), boxShadow: on ? '0 10px 24px -10px color-mix(in srgb, var(--cp-primary) 70%, transparent)' : 'var(--cp-shadow-card)', transition: 'all 200ms var(--cp-ease)' }}>
    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
      <CategoryIcon icon={a.icon} color={on ? '#fff' : a.color} size={38} style={on ? { background: 'rgba(255,255,255,.18)', color: 'inherit' } : undefined} />
      <div style={{ flex: 1, minWidth: 0 }}><div style={{ fontWeight: 750 }}>{a.name}</div><div className="cp-caption" style={{ opacity: on ? .8 : 1, color: on ? 'inherit' : 'var(--cp-text-2)', fontWeight: 600 }}>{a.bank}</div></div>
      <Sparkline values={hist} width={56} height={24} area={false} color={on ? 'currentColor' : a.color} />
    </div>
    <div style={{ marginTop: 14, fontSize: 20, fontWeight: 750, letterSpacing: '-0.02em', fontVariantNumeric: 'tabular-nums' }}>{CP.eur(a.balance)}</div>
  </button>;
}
function Accounts({ onOpenBooking }) {
  const [sel, setSel] = React.useState('giro'); const [tab, setTab] = React.useState('over');
  const a = CP.ACCOUNTS.find(x => x.id === sel); const K = CP.CUR; const months = CP.range(K - 11, K + 1);
  const list = CP.BOOKINGS.filter(b => b.account === sel);
  const last30 = list.filter(b => b.date > CP.TODAY.getTime() - 30 * 864e5);
  const total = CP.ACCOUNTS.reduce((s, x) => s + x.balance, 0);
  const flows = CP.range(K - 5, K).map(k => ({ k, inn: CP.sumBy(list.filter(b => CP.mk(b.date) === k && b.cents > 0)), out: -CP.sumBy(list.filter(b => CP.mk(b.date) === k && b.cents < 0)) }));
  return <div className="cp-page">
    <PageHeader title="Konten" lead={<>Zusammen hast du <b>{CP.eur(total)}</b> auf {CP.ACCOUNTS.length} Konten.</>} />
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 280px), 1fr))', gap: 'var(--cp-grid-gap)', alignItems: 'start' }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
        <div className="cp-label" style={{ padding: '0 4px' }}>Deine Konten</div>
        {CP.ACCOUNTS.map(x => <AccountCard key={x.id} a={x} on={x.id === sel} onClick={() => setSel(x.id)} />)}
        <div className="cp-caption cp-muted" style={{ padding: '4px 4px', display: 'flex', gap: 8 }}><Icon name="info" size={15} />Konten kommen aus deinem Finanzguru-Export. Neue Konten erscheinen nach dem nächsten Import.</div>
      </div>
      <div style={{ gridColumn: 'span 2', minWidth: 0, display: 'flex', flexDirection: 'column', gap: 'var(--cp-grid-gap)' }}>
        <Card padded={false}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16, padding: '20px 20px 0', flexWrap: 'wrap' }}>
            <CategoryIcon icon={a.icon} color={a.color} size={52} shape="round" />
            <div style={{ flex: 1, minWidth: 200 }}><h2 className="cp-h2">{a.name}</h2><div className="cp-small cp-muted" style={{ fontWeight: 600 }}>{a.bank} · <span className="cp-mono">{a.iban}</span></div></div>
            <div style={{ textAlign: 'right' }}><div className="cp-label">Kontostand</div><Amount cents={a.balance} sign={false} dimCents size={28} /></div>
          </div>
          <div style={{ padding: '0 20px' }}><Tabs style={{ marginTop: 16 }} value={tab} onChange={setTab} tabs={[{ value: 'over', label: 'Überblick' }, { value: 'tx', label: 'Buchungen', count: list.length.toLocaleString('de-DE') }]} /></div>
          {tab === 'over' ? <div style={{ padding: 20 }}>
            <div className="cp-h3" style={{ marginBottom: 12 }}>Kontostand im Verlauf</div>
            <AreaChart height={240} labels={months.map(CP.monthLabel)} tooltipTitle={i => 'Ende ' + CP.monthLong(months[i])} format={CP.eur} formatAxis={CP.axis} series={[{ name: 'Kontostand', color: a.color, values: months.map(k => CP.balanceAt(sel, k)) }]} />
          </div> : <div style={{ paddingBottom: 8 }}><TransactionList groupByDay items={list.slice(0, 30).map(CP.toTx)} onSelect={t => onOpenBooking(t.raw)} /></div>}
        </Card>
        {tab === 'over' && <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 300px), 1fr))', gap: 'var(--cp-grid-gap)' }}>
          <Card title="Rein und raus" subtitle="Letzte 6 Monate" action={<Legend items={[{ label: 'Rein', color: 'var(--cp-income)' }, { label: 'Raus', color: 'var(--cp-expense)' }]} />}>
            <BarChart height={200} labels={flows.map(f => CP.monthLabel(f.k))} format={CP.eur} formatAxis={CP.axis} series={[{ name: 'Rein', color: 'var(--cp-income)', values: flows.map(f => f.inn) }, { name: 'Raus', color: 'var(--cp-expense)', values: flows.map(f => f.out) }]} />
          </Card>
          <Card title="Letzte 30 Tage" padded={false}>
            <div style={{ padding: '14px 20px 0', display: 'flex', gap: 24 }}>
              <div><div className="cp-caption cp-muted" style={{ fontWeight: 600 }}>Eingänge</div><Amount cents={CP.sumBy(last30.filter(b => b.cents > 0))} size={18} /></div>
              <div><div className="cp-caption cp-muted" style={{ fontWeight: 600 }}>Ausgänge</div><Amount cents={CP.sumBy(last30.filter(b => b.cents < 0))} size={18} /></div>
            </div>
            <div style={{ paddingBottom: 8, paddingTop: 6 }}><TransactionList items={last30.slice(0, 4).map(CP.toTx).map(t => ({ ...t, meta: CP.date(t.raw.date) }))} onSelect={t => onOpenBooking(t.raw)} /></div>
          </Card>
        </div>}
      </div>
    </div>
  </div>;
}
Object.assign(window, { Accounts });
})();
