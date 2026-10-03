import React from 'react';
function useWidth() { const ref = React.useRef(null); const [w, setW] = React.useState(0); React.useLayoutEffect(() => { const el = ref.current; if (!el) return; const ro = new ResizeObserver(e => setW(e[0].contentRect.width)); ro.observe(el); setW(el.getBoundingClientRect().width); return () => ro.disconnect(); }, []); return [ref, w]; }
function niceMax(v) { if (v <= 0) return 1; const p = Math.pow(10, Math.floor(Math.log10(v))); const n = v / p; return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10) * p; }
export function BarChart({ labels, series, stacked, height = 240, format = v => String(v), formatAxis, highlight, onSelect, style }) {
  const [ref, w] = useWidth(); const [hi, setHi] = React.useState(null);
  const W = Math.max(w, 240), pl = 52, pr = 8, pt = 12, pb = 28, n = labels.length;
  const totals = labels.map((_, i) => stacked ? series.reduce((a, s) => a + s.values[i], 0) : Math.max(...series.map(s => s.values[i])));
  const max = niceMax(Math.max(...totals, 0) * 1.05);
  const y = v => pt + (max - v) / max * (height - pt - pb); const band = (W - pl - pr) / n;
  const groupW = Math.min(band * 0.62, stacked ? 36 : 22 * series.length + 4 * (series.length - 1));
  const bw = stacked ? groupW : (groupW - 4 * (series.length - 1)) / series.length;
  const ticks = [0, 1, 2, 3, 4].map(k => max * k / 4); const fa = formatAxis || format;
  const step = Math.max(1, Math.ceil(n / Math.max(2, Math.floor((W - pl) / 48))));
  const act = hi != null ? hi : highlight;
  return <div ref={ref} className="cp-chart" style={style}>
    {w > 0 && <svg width={W} height={height} onMouseLeave={() => setHi(null)}>
      {ticks.map((t, i) => <g key={i}><line x1={pl} x2={W - pr} y1={y(t)} y2={y(t)} stroke="var(--cp-chart-grid)" strokeDasharray={t === 0 ? undefined : '3 4'} /><text x={pl - 10} y={y(t) + 4} textAnchor="end">{fa(t)}</text></g>)}
      {labels.map((l, i) => { const cx = pl + band * i + band / 2; let acc = 0; const dim = act != null && act !== i; return <g key={i} onMouseEnter={() => setHi(i)} onClick={onSelect ? () => onSelect(i) : undefined} style={{ cursor: onSelect ? 'pointer' : 'default' }}>
        <rect x={pl + band * i} y={pt} width={band} height={height - pt - pb} fill={hi === i ? 'var(--cp-hover)' : 'transparent'} rx="6" />
        {series.map((s, si) => { const v = s.values[i]; if (stacked) { const y0 = y(acc), y1 = y(acc + v); acc += v; return <rect key={si} x={cx - bw / 2} y={y1} width={bw} height={Math.max(0, y0 - y1 - (si < series.length - 1 ? 1.5 : 0))} rx={si === series.length - 1 ? 4 : 1} fill={s.color} opacity={dim ? 0.35 : 1} />; }
          const bx = cx - groupW / 2 + si * (bw + 4); return <rect key={si} x={bx} y={y(v)} width={bw} height={Math.max(0, y(0) - y(v))} rx="4" fill={s.color} opacity={dim ? 0.35 : 1} />; })}
        {(i % step === 0) && <text x={cx} y={height - 8} textAnchor="middle" style={act === i ? { fill: 'var(--cp-text)' } : undefined}>{l}</text>}
      </g>; })}
    </svg>}
    {hi != null && w > 0 && <div className="cp-chart__tip" style={{ left: Math.min(pl + band * hi + band / 2 + 14, W - 170), top: 8 }}>
      <div className="cp-chart__tip-title">{labels[hi]}</div>
      {series.map((s, si) => <div key={si} className="cp-chart__tip-row"><span className="cp-legend__swatch" style={{ '--c': s.color }} />{s.name}<b>{format(s.values[hi])}</b></div>)}
      {stacked && series.length > 1 && <div className="cp-chart__tip-row" style={{ marginTop: 4, paddingTop: 6, borderTop: '1px solid var(--cp-line)' }}>Summe<b>{format(totals[hi])}</b></div>}
    </div>}
  </div>;
}
