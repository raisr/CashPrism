248px left navigation with wordmark, items and a bottom slot.
```jsx
<Sidebar active={route} onNavigate={go} items={[
  {href:'/', label:'Übersicht', icon:'layout-dashboard'},
  {href:'/bookings', label:'Buchungen', icon:'receipt-text', count:'6.327'},
  {section:'Auswerten'},
  {href:'/analysis', label:'Analyse', icon:'chart-pie'},
]} footer={<PrivacyNote/>} />
```

Collapsible: pass `collapsed` + `onToggleCollapse`. The toggle is a small grey panel icon beside the wordmark; collapsed, the bar becomes a 72px icon rail: the "CP" monogram tile (same as assets/logo/icon-light.svg, rendered as text) on top, the toggle below it. Remember the choice (localStorage). On narrow screens (<1040px) use the overlay drawer instead.
```jsx
<Sidebar collapsed={c} onToggleCollapse={() => setC(!c)} collapsedFooter={<Icon name="lock" />} … />
```
