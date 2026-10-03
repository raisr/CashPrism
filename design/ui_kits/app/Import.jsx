(() => {
const { PageHeader, Card, Dropzone, Alert, DataTable, Badge, CategoryIcon, Spinner, Icon } = window.CashPrismDesignSystem_24fa3e;
const STEPS = [['smartphone', 'Finanzguru öffnen', 'In der App unter Profil → Datenexport.'], ['file-spreadsheet', '„Alle Buchungen“ exportieren', 'Als Excel-Datei (.xlsx) speichern oder dir selbst schicken.'], ['upload', 'Hier ablegen', 'CashPrism liest nur Neues ein. Doppeltes wird erkannt.']];
function ImportScreen() {
  const [runs, setRuns] = React.useState(CP.IMPORTS); const [busy, setBusy] = React.useState(false); const [res, setRes] = React.useState(null);
  const onFile = f => {
    if (!/\.xlsx$/i.test(f.name)) { setRes({ tone: 'danger', title: 'Das ist keine Finanzguru-Datei', body: 'Wir brauchen den Export „Alle Buchungen“ als .xlsx-Datei.' }); return; }
    if (runs.some(r => r.file === f.name)) { setRes({ tone: 'info', title: 'Diese Datei kennen wir schon', body: 'Sie wurde bereits eingelesen – es hat sich nichts geändert.' }); return; }
    setBusy(true); setRes(null);
    setTimeout(() => { const ins = 30 + Math.floor(Math.random() * 40), upd = Math.floor(Math.random() * 6); setRuns(r => [{ id: 'n' + Date.now(), at: Date.now(), file: f.name, exported: Date.now(), rows: r[0].rows + ins, inserted: ins, updated: upd }, ...r]);
      setRes({ tone: 'success', title: 'Fertig! ' + ins + ' neue Buchungen sind da.', details: [(r => r)(runs[0].rows + ins).toLocaleString('de-DE') + ' Zeilen gelesen', upd + ' Buchungen wurden in Finanzguru geändert und hier aktualisiert', 'Alles andere war schon bekannt'] }); setBusy(false); }, 1600);
  };
  return <div className="cp-page">
    <PageHeader title="Import" lead="Hol deine neuesten Buchungen aus Finanzguru. Jeder Import ergänzt, was schon da ist – nichts geht verloren." />
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 320px), 1fr))', gap: 'var(--cp-grid-gap)', alignItems: 'start' }}>
      <Card style={{ gridColumn: 'span 2' }}>
        {busy ? <div className="cp-dropzone" style={{ cursor: 'default' }}><Spinner size={34} /><div className="cp-h3" style={{ marginTop: 10 }}>Wird eingelesen …</div><div className="cp-small cp-muted">Das dauert ein paar Sekunden.</div></div> : <Dropzone onFile={onFile} />}
        {res && <Alert tone={res.tone} title={res.title} details={res.details} style={{ marginTop: 16 }}>{res.body}</Alert>}
      </Card>
      <Card title="So geht’s">
        <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>{STEPS.map(([ic, t, s], i) => <div key={i} style={{ display: 'flex', gap: 12 }}>
          <CategoryIcon icon={ic} color="var(--cp-primary)" size={36} shape="round" /><div><div style={{ fontWeight: 750 }}>{i + 1}. {t}</div><div className="cp-small cp-muted">{s}</div></div></div>)}</div>
      </Card>
    </div>
    <Card title="Bisherige Importe" subtitle="Was wann eingelesen wurde" padded={false} style={{ marginTop: 'var(--cp-grid-gap)' }}>
      <div style={{ marginTop: 12 }}><DataTable columns={[
        { key: 'file', title: 'Datei', render: r => <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}><CategoryIcon icon="file-spreadsheet" color="var(--cp-prism-green)" size={34} /><div style={{ minWidth: 0 }}><b style={{ display: 'block', maxWidth: 320, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }} title={r.file}>{r.file}</b><div className="cp-caption cp-muted">Exportiert {r.exported ? 'am ' + CP.date(r.exported) : '– Datum unbekannt'}</div></div></div> },
        { key: 'at', title: 'Eingelesen', render: r => <span className="cp-muted cp-num" style={{ fontWeight: 600 }}>{new Date(r.at).toLocaleString('de-DE', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</span> },
        { key: 'inserted', title: 'Neu', align: 'right', render: r => <Badge tone="success">+{r.inserted.toLocaleString('de-DE')}</Badge> },
        { key: 'updated', title: 'Aktualisiert', align: 'right', render: r => <span className="cp-num" style={{ fontWeight: 700 }}>{r.updated}</span> },
        { key: 'rows', title: 'Zeilen gesamt', align: 'right', render: r => <span className="cp-num cp-muted" style={{ fontWeight: 700 }}>{r.rows.toLocaleString('de-DE')}</span> },
      ]} rows={runs} /></div>
    </Card>
  </div>;
}
Object.assign(window, { ImportScreen });
})();
