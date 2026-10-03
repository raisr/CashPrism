(() => {
const TITLES = { '/': 'Übersicht', '/bookings': 'Buchungen', '/accounts': 'Konten', '/analysis': 'Analyse', '/reports': 'Berichte', '/import': 'Import' };
function App() {
  const [route, setRoute] = React.useState(() => window.CP_ROUTE || localStorage.getItem('cp2-route') || '/');
  const [dark, setDark] = React.useState(() => localStorage.getItem('cp2-theme') === 'dark');
  const [query, setQuery] = React.useState(''); const [open, setOpen] = React.useState(null);
  React.useEffect(() => { document.documentElement.dataset.theme = dark ? 'dark' : 'light'; localStorage.setItem('cp2-theme', dark ? 'dark' : 'light'); }, [dark]);
  React.useEffect(() => { if (!window.CP_ROUTE) localStorage.setItem('cp2-route', route); document.title = 'CashPrism – ' + TITLES[route]; const m = document.getElementById('cp-main'); if (m) m.scrollTo({ top: 0 }); }, [route]);
  const go = r => setRoute(r);
  let screen;
  if (route === '/') screen = <Dashboard onNavigate={go} onOpenBooking={setOpen} />;
  else if (route === '/bookings') screen = <Bookings query={query} setQuery={setQuery} onOpenBooking={setOpen} />;
  else if (route === '/accounts') screen = <Accounts onOpenBooking={setOpen} />;
  else if (route === '/analysis') screen = <Analysis onNavigate={go} />;
  else if (route === '/reports') screen = <Reports />;
  else screen = <ImportScreen />;
  return <Shell route={route} onNavigate={go} dark={dark} onToggleTheme={() => setDark(d => !d)} onSearch={q => { setQuery(q); setRoute('/bookings'); }}>{screen}<BookingSheet booking={open} onClose={() => setOpen(null)} /></Shell>;
}
ReactDOM.createRoot(document.getElementById('root')).render(<App />);
})();
