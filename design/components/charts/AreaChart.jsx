import React from 'react';
function useWidth() { const ref = React.useRef(null); const [w, setW] = React.useState(0); React.useLayoutEffect(() => { const el = ref.current; if (!el) return; const ro = new ResizeObserver(e => setW(e[0].contentRect.width)); ro.observe(el); setW(el.getBoundingClientRect().width); return () => ro.disconnect(); }, []); return [ref, w]; }
function niceMax(v) { if (v <= 0) return 1; const p = Math.pow(10, Math.floor(Math.log10(v))); const n = v / p; return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10) * p; }
function smooth(pts) { if (!pts.length) return ''; let d = 'M' + pts[0][0] + ',' + pts[0][1]; for (let i = 0; i < pts.length - 1; i++) { const p0 = pts[i - 1] || pts[i], p1 = pts[i], p2 = pts[i + 1], p3 = pts[i + 2] || p2, t = 0.17; d += ' C' + (p1[0] + (p2[0] - p0[0]) * t) + ',' + (p1[1] + (p2[1] - p0[1]) * t) + ' ' + (p2[0] - (p3[0] - p1[0]) * t) + ',' + (p2[1] - (p3[1] - p1[1]) * t) + ' ' + p2[0] + ',' + p2[1]; } return d; }
export function AreaChart({ labels, series, height = 260, format = v => String(v), formatAxis, tooltipTitle, area = true, style }) {
  const [ref, w] = useWidth(); const [hi, setHi] = React.useState(null); const uid = React.useId().replace(/:/g, '');
  const W = Math.max(w, 240), pl = 52, pr = 12, pt = 12, pb = 28, n = labels.length;
  const all = series.flatMap(s => s.values); const max = niceMax(Math.max(...all, 0) * 1.05); const minRaw = Math.min(0, ...all); const min = minRaw < 0 ? -niceMax(-minRaw) : 0;
  const x = i => pl + (n === 1 ? 0 : i * (W - pl - pr) / (n - 1)); const y = v => pt + (max - v) / (max - min) * (height - pt - pb);
  const ticks = [0, 1, 2, 3, 4].map(k => min + (max - min) * k / 4);
  const step = Math.max(1, Math.ceil(n / Math.max(2, Math.floor((W - pl) / 64))));
  const fa = formatAxis || format;
  const move = e => { const r = e.currentTarget.getBoundingClientRect(); const px = e.clientX - r.left; const i = Math.round((px - pl) / ((W - pl - pr) / Math.max(1, n - 1))); setHi(Math.max(0, Math.min(n - 1, i))); };
  return <div ref={ref} className="cp-chart" style={style}>
    {w > 0 && <svg width={W} height={height} onMouseMove={move} onMouseLeave={() => setHi(null)}>
      <defs>{series.map((s, si) => <linearGradient key={si} id={uid + 'g' + si} x1="0" x2="0" y1="0" y2="1"><stop offset="0" stopColor={s.color} stopOpacity="0.24" /><stop offset="1" stopColor={s.color} stopOpacity="0" /></linearGradient>)}</defs>
      {ticks.map((t, i) => <g key={i}><line x1={pl} x2={W - pr} y1={y(t)} y2={y(t)} stroke="var(--cp-chart-grid)" strokeDasharray={t === 0 ? undefined : '3 4'} /><text x={pl - 10} y={y(t) + 4} textAnchor="end">{fa(t)}</text></g>)}
      {labels.map((l, i) => i % step === 0 || i === n - 1 ? <text key={i} x={x(i)} y={height - 8} textAnchor={i === 0 ? 'start' : i === n - 1 ? 'end' : 'middle'}>{l}</text> : null)}
      {hi != null && <line x1={x(hi)} x2={x(hi)} y1={pt} y2={height - pb} stroke="var(--cp-line-strong)" />}
      {series.map((s, si) => { const pts = s.values.map((v, i) => [x(i), y(v)]); const d = smooth(pts); return <g key={si}>
        {area && s.area !== false && <path d={d + ' L' + x(n - 1) + ',' + y(Math.max(min, 0)) + ' L' + x(0) + ',' + y(Math.max(min, 0)) + ' Z'} fill={'url(#' + uid + 'g' + si + ')'} />}
        <path d={d} fill="none" stroke={s.color} strokeWidth="2.5" strokeLinecap="round" strokeDasharray={s.dashed ? '5 5' : undefined} />
        {hi != null && <circle cx={x(hi)} cy={y(s.values[hi])} r="4.5" fill="var(--cp-surface)" stroke={s.color} strokeWidth="2.5" />}
      </g>; })}
    </svg>}
    {hi != null && w > 0 && <div className="cp-chart__tip" style={{ left: Math.min(Math.max(x(hi) + 12, 0), W - 170), top: 8 }}>
      <div className="cp-chart__tip-title">{tooltipTitle ? tooltipTitle(hi) : labels[hi]}</div>
      {series.map((s, si) => <div key={si} className="cp-chart__tip-row"><span className="cp-legend__swatch cp-legend__swatch--line" style={{ '--c': s.color }} />{s.name}<b>{format(s.values[hi])}</b></div>)}
    </div>}
  </div>;
}
