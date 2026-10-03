import React from 'react';
export function DonutChart({ data, size = 200, thickness = 22, centerLabel, centerValue, format = v => String(v), active, onActiveChange, style }) {
  const [hiState, setHi] = React.useState(null); const hi = active !== undefined ? active : hiState;
  const set = i => { setHi(i); onActiveChange && onActiveChange(i); };
  const total = data.reduce((a, d) => a + d.value, 0) || 1; const r = (size - thickness) / 2; const c = 2 * Math.PI * r; const gap = data.length > 1 ? 3 : 0;
  let acc = 0;
  const cur = hi != null ? data[hi] : null;
  return <div className="cp-chart" style={{ width: size, height: size, flexShrink: 0, ...style }}>
    <svg width={size} height={size} style={{ transform: 'rotate(-90deg)' }} onMouseLeave={() => set(null)}>
      <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke="var(--cp-surface-3)" strokeWidth={thickness} />
      {data.map((d, i) => { const len = d.value / total * c; const off = acc; acc += len; return <circle key={i} cx={size / 2} cy={size / 2} r={r} fill="none" stroke={d.color} strokeWidth={hi === i ? thickness + 6 : thickness}
        strokeDasharray={Math.max(0, len - gap) + ' ' + (c - Math.max(0, len - gap))} strokeDashoffset={-off} opacity={hi != null && hi !== i ? 0.35 : 1}
        style={{ transition: 'stroke-width 160ms, opacity 160ms', cursor: 'pointer' }} onMouseEnter={() => set(i)} />; })}
    </svg>
    <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', textAlign: 'center', pointerEvents: 'none', padding: thickness + 8 }}>
      <div className="cp-caption cp-muted" style={{ fontWeight: 600, maxWidth: '100%', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{cur ? cur.label : centerLabel}</div>
      <div style={{ fontSize: size > 170 ? 22 : 17, fontWeight: 750, letterSpacing: '-0.02em', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' }}>{cur ? format(cur.value) : centerValue}</div>
      {cur && <div className="cp-caption cp-faint" style={{ fontWeight: 650 }}>{Math.round(cur.value / total * 100)} %</div>}
    </div>
  </div>;
}
