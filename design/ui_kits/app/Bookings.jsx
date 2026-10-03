(() => {
const { PageHeader, Card, DataTable, Pager, Input, Select, Segmented, FilterChip, Amount, CategoryIcon, Badge, Sheet, Button, EmptyState, Switch, TransactionList } = window.CashPrismDesignSystem_24fa3e;
function BookingSheet({ booking, onClose }) {
  if (!booking) return null;
  const c = CP.CATS[booking.cat], acc = CP.ACCOUNTS.find(a => a.id === booking.account);
  const same = CP.BOOKINGS.filter(b => b.who === booking.who && b.id !== booking.id).slice(0, 5).map(CP.toTx).map(t => ({ ...t, meta: CP.date(t.raw.date), day: undefined }));
  return <Sheet open onClose={onClose} title={booking.who} subtitle={CP.dayLabel(booking.date)} leading={<CategoryIcon icon={c.icon} color={c.color} size={44} />}
    footer={<><Button variant="secondary" icon="tag">Kategorie ändern</Button><Button variant="secondary" icon="eye-off">Ausblenden</Button></>}>
    <div style={{ textAlign: 'center', padding: '8px 0 24px' }}>
      <Amount cents={booking.cents} size={34} dimCents />
      <div style={{ marginTop: 10, display: 'flex', gap: 6, justifyContent: 'center', flexWrap: 'wrap' }}><Badge>{booking.kind}</Badge>{booking.contract && <Badge tone="info" icon="repeat">Regelmäßig</Badge>}{booking.transfer && <Badge icon="arrow-left-right">Zwischen deinen Konten</Badge>}</div>
    </div>
    <dl className="cp-kv">
      <dt>Datum</dt><dd>{CP.date(booking.date)}</dd>
      <dt>Konto</dt><dd>{acc.name}</dd>
      <dt>Kategorie</dt><dd>{c.name}</dd>
      <dt>{booking.cents < 0 ? 'Empfänger' : 'Absender'}</dt><dd>{booking.who}</dd>
      <dt>Verwendungszweck</dt><dd style={{ fontWeight: 600 }}>{booking.ref}</dd>
    </dl>
    {same.length > 0 && <><div className="cp-label" style={{ margin: '28px 0 6px' }}>Frühere Buchungen bei {booking.who}</div><div style={{ margin: '0 -20px' }}><TransactionList items={same} /></div></>}
  </Sheet>;
}
const PERIODS = [{ value: 'm', label: 'Dieser Monat' }, { value: '3', label: 'Letzte 3 Monate' }, { value: 'y', label: 'Dieses Jahr' }, { value: 'all', label: 'Alles' }];
function Bookings({ query, setQuery, onOpenBooking }) {
  const [acc, setAcc] = React.useState('all'); const [period, setPeriod] = React.useState('3'); const [dir, setDir] = React.useState('all');
  const [cat, setCat] = React.useState(null); const [hideTransfers, setHideTransfers] = React.useState(true);
  const [sort, setSort] = React.useState(null); const [page, setPage] = React.useState(0); const size = 25;
  React.useEffect(() => setPage(0), [acc, period, dir, cat, query, hideTransfers, sort]);
  const K = CP.mk(CP.TODAY.getTime());
  const rows = React.useMemo(() => {
    const q = (query || '').toLowerCase();
    let r = CP.BOOKINGS.filter(b => (acc === 'all' || b.account === acc) && (!hideTransfers || !b.transfer) && (dir === 'all' || (dir === 'in' ? b.cents > 0 : b.cents < 0)) && (!cat || b.cat === cat)
      && (period === 'all' || (period === 'm' ? CP.mk(b.date) === K : period === '3' ? CP.mk(b.date) >= K - 2 : new Date(b.date).getFullYear() === 2026))
      && (!q || (b.who + ' ' + b.ref + ' ' + CP.CATS[b.cat].name).toLowerCase().includes(q)));
    if (sort) r = [...r].sort((a, b) => { const x = sort.key === 'cents' ? a.cents - b.cents : sort.key === 'date' ? a.date - b.date : a.who.localeCompare(b.who, 'de'); return sort.desc ? -x : x; });
    return r;
  }, [acc, period, dir, cat, query, hideTransfers, sort]);
  const inSum = CP.sumBy(rows.filter(b => b.cents > 0)), outSum = CP.sumBy(rows.filter(b => b.cents < 0));
  const cols = [
    { key: 'who', title: 'Empfänger / Absender', sortable: true, render: b => { const c = CP.CATS[b.cat]; return <div style={{ display: 'flex', alignItems: 'center', gap: 12, minWidth: 220 }}><CategoryIcon icon={c.icon} color={c.color} size={36} /><div style={{ minWidth: 0 }}><div style={{ fontWeight: 700 }}>{b.who}</div><div className="cp-caption cp-muted" style={{ maxWidth: 320, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{b.ref}</div></div></div>; } },
    { key: 'cat', title: 'Kategorie', render: b => <Badge style={{ '--c': CP.CATS[b.cat].color }}><span style={{ width: 7, height: 7, borderRadius: 9, background: CP.CATS[b.cat].color }} />{CP.CATS[b.cat].name}</Badge> },
    { key: 'account', title: 'Konto', render: b => <span className="cp-muted" style={{ fontWeight: 600 }}>{CP.ACCOUNTS.find(a => a.id === b.account).name}</span> },
    { key: 'date', title: 'Datum', sortable: true, render: b => <span className="cp-muted cp-num" style={{ fontWeight: 600 }}>{CP.date(b.date)}</span> },
    { key: 'cents', title: 'Betrag', align: 'right', sortable: true, render: b => <Amount cents={b.cents} /> },
  ];
  return <div className="cp-page">
    <PageHeader title="Buchungen" lead={'Alles, was auf deinen Konten passiert ist – ' + CP.BOOKINGS.length.toLocaleString('de-DE') + ' Buchungen seit Oktober 2024.'} />
    <Card padded={false}>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 10, padding: 'var(--cp-card-padding)', alignItems: 'center' }}>
        <Input icon="search" placeholder="Name, Verwendungszweck, Kategorie …" value={query} onChange={setQuery} style={{ flex: '1 1 260px' }} />
        <Select icon="landmark" value={acc} onChange={setAcc} options={[{ value: 'all', label: 'Alle Konten' }, ...CP.ACCOUNTS.map(a => ({ value: a.id, label: a.name }))]} style={{ flex: '0 1 200px' }} />
        <Select icon="calendar" value={period} onChange={setPeriod} options={PERIODS} style={{ flex: '0 1 200px' }} />
        <Segmented value={dir} onChange={setDir} options={[{ value: 'all', label: 'Alle' }, { value: 'in', label: 'Einnahmen' }, { value: 'out', label: 'Ausgaben' }]} />
      </div>
      <div style={{ display: 'flex', gap: 8, padding: '0 var(--cp-card-padding) 16px', overflowX: 'auto', alignItems: 'center', scrollbarWidth: 'none' }}>
        {[...CP.SPEND, 'einkommen'].map(id => <FilterChip key={id} active={cat === id} onClick={() => setCat(cat === id ? null : id)} onRemove={cat === id ? () => setCat(null) : undefined}><span style={{ width: 8, height: 8, borderRadius: 9, background: CP.CATS[id].color }} />{CP.CATS[id].name}</FilterChip>)}
        <span style={{ flex: 1 }} /><Switch checked={hideTransfers} onChange={setHideTransfers} label={<span className="cp-small" style={{ whiteSpace: 'nowrap' }}>Umbuchungen ausblenden</span>} />
      </div>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px 24px', padding: '12px var(--cp-card-padding)', borderTop: '1px solid var(--cp-line)', background: 'var(--cp-surface-2)' }} className="cp-small">
        <span><b>{rows.length.toLocaleString('de-DE')}</b> <span className="cp-muted">Buchungen</span></span>
        <span className="cp-muted">Einnahmen <Amount cents={inSum} /></span>
        <span className="cp-muted">Ausgaben <Amount cents={outSum} /></span>
      </div>
      {rows.length === 0 ? <EmptyState icon="search" title="Nichts gefunden" text="Keine Buchung passt zu deiner Suche. Probier einen anderen Begriff oder einen längeren Zeitraum." actions={<Button variant="secondary" onClick={() => { setQuery(''); setCat(null); setPeriod('all'); }}>Filter zurücksetzen</Button>} />
        : <><div style={{ borderTop: '1px solid var(--cp-line)' }}><DataTable columns={cols} rows={rows.slice(page * size, page * size + size)} sort={sort} onSortChange={setSort} onRowClick={onOpenBooking} groupBy={sort ? undefined : b => CP.dayLabel(b.date)} /></div>
          <Pager page={page} pageSize={size} total={rows.length} onPageChange={p => { setPage(p); document.getElementById('cp-main').scrollTo({ top: 0 }); }} noun="Buchungen" /></>}
    </Card>
  </div>;
}
Object.assign(window, { Bookings, BookingSheet });
})();
