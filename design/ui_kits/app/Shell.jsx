(() => {
const { Sidebar, Topbar, Input, IconButton, Button, Icon } = window.CashPrismDesignSystem_24fa3e;
const NAV = [
  { href: '/', label: 'Übersicht', icon: 'layout-dashboard' },
  { href: '/bookings', label: 'Buchungen', icon: 'receipt-text' },
  { href: '/accounts', label: 'Konten', icon: 'wallet' },
  { section: 'Auswerten' },
  { href: '/analysis', label: 'Analyse', icon: 'chart-pie' },
  { href: '/reports', label: 'Berichte', icon: 'file-chart-column' },
  { section: 'Daten' },
  { href: '/import', label: 'Import', icon: 'upload' },
];
function PrivacyNote({ lastImport }) {
  return <div style={{ padding: 14, borderRadius: 'var(--cp-radius-lg)', background: 'var(--cp-surface-2)', border: '1px solid var(--cp-line)' }}>
    <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontWeight: 750 }}><Icon name="lock" size={16} color="var(--cp-primary)" />Nur auf diesem Rechner</div>
    <div className="cp-caption cp-muted" style={{ marginTop: 4 }}>Kein Konto, keine Cloud. Deine Daten verlassen dein Heimnetz nicht.</div>
    <div className="cp-caption cp-faint" style={{ marginTop: 10, fontWeight: 600 }}>Letzter Import: {lastImport}</div>
  </div>;
}
function useWide(px) { const q = '(min-width: ' + px + 'px)'; const [w, setW] = React.useState(() => matchMedia(q).matches); React.useEffect(() => { const m = matchMedia(q); const f = () => setW(m.matches); m.addEventListener('change', f); return () => m.removeEventListener('change', f); }, []); return w; }
function Shell({ route, onNavigate, dark, onToggleTheme, onSearch, children }) {
  const wide = useWide(1040) || !!window.CP_FORCE_WIDE; const [open, setOpen] = React.useState(false); const [q, setQ] = React.useState('');
  const [collapsed, setCollapsed] = React.useState(() => localStorage.getItem('cp2-nav') === 'collapsed');
  React.useEffect(() => localStorage.setItem('cp2-nav', collapsed ? 'collapsed' : 'open'), [collapsed]);
  const nav = h => { onNavigate(h); setOpen(false); };
  const items = NAV.map(n => n.href === '/bookings' ? { ...n, count: CP.BOOKINGS.length.toLocaleString('de-DE') } : n);
  const rail = wide && collapsed;
  const sidebar = <Sidebar items={items} active={route} onNavigate={nav} collapsed={rail} onToggleCollapse={wide ? () => setCollapsed(v => !v) : undefined}
    footer={<PrivacyNote lastImport={CP.date(CP.IMPORTS[0].at)} />}
    collapsedFooter={<span title="Nur auf diesem Rechner – kein Konto, keine Cloud" style={{ display: 'flex', width: 40, height: 40, alignItems: 'center', justifyContent: 'center', borderRadius: 'var(--cp-radius-md)', background: 'var(--cp-surface-2)', border: '1px solid var(--cp-line)' }}><Icon name="lock" size={16} color="var(--cp-primary)" /></span>} />;
  const current = NAV.find(n => n.href === route);
  return <div style={{ display: 'flex', height: '100vh', background: 'var(--cp-bg)' }}>
    {wide ? <div style={{ flex: '0 0 auto', height: '100%' }}>{sidebar}</div> : open && <><div className="cp-sheet-scrim" onClick={() => setOpen(false)} /><div style={{ position: 'fixed', inset: '0 auto 0 0', zIndex: 950 }}>{sidebar}</div></>}
    <div style={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column' }}>
      <Topbar section={wide ? current && current.label : null}>
        {!wide && <IconButton icon="menu" aria-label="Navigation öffnen" onClick={() => setOpen(true)} />}
        <span className="cp-topbar__spacer" />
        <form onSubmit={e => { e.preventDefault(); onSearch(q); }} style={{ width: 'min(340px, 40vw)' }}><Input icon="search" placeholder="Buchungen durchsuchen …" value={q} onChange={setQ} kbd="↵" /></form>
        <IconButton icon={dark ? 'sun' : 'moon'} aria-label={dark ? 'Helles Design' : 'Dunkles Design'} onClick={onToggleTheme} />
        <IconButton icon="bell" dot aria-label="Hinweise" onClick={() => onNavigate('/')} />
        <Button icon="upload" size="sm" onClick={() => onNavigate('/import')}>Import</Button>
      </Topbar>
      <main style={{ flex: 1, overflow: 'auto' }} id="cp-main">{children}</main>
    </div>
  </div>;
}
Object.assign(window, { Shell });
})();
