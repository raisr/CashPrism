import React from 'react';
export function Brand({ tagline, style }) {
  return <span className="cp-brand" style={style}><span style={{ display: 'flex', flexDirection: 'column', gap: 2 }}><span className="cp-brand__name">Cash<span>Prism</span></span>{tagline && <span className="cp-brand__tag">{tagline}</span>}</span></span>;
}
