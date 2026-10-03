import React from 'react';
export function Sparkline({ values, color = 'var(--cp-primary)', width = 96, height = 32, area = true, style }) {
  const max = Math.max(...values), min = Math.min(...values), n = values.length; const uid = React.useId().replace(/:/g, '');
  const pts = values.map((v, i) => [i * (width - 4) / Math.max(1, n - 1) + 2, 2 + (max - v) / ((max - min) || 1) * (height - 4)]);
  const d = 'M' + pts.map(p => p.join(',')).join(' L');
  return <svg width={width} height={height} style={{ display: 'block', overflow: 'visible', ...style }}>
    <defs><linearGradient id={uid} x1="0" x2="0" y1="0" y2="1"><stop offset="0" stopColor={color} stopOpacity="0.25" /><stop offset="1" stopColor={color} stopOpacity="0" /></linearGradient></defs>
    {area && <path d={d + ' L' + pts[n - 1][0] + ',' + height + ' L' + pts[0][0] + ',' + height + ' Z'} fill={'url(#' + uid + ')'} />}
    <path d={d} fill="none" stroke={color} strokeWidth="2" strokeLinejoin="round" strokeLinecap="round" />
    <circle cx={pts[n - 1][0]} cy={pts[n - 1][1]} r="2.5" fill={color} />
  </svg>;
}
