import React from 'react';
export function Segmented({ options, value, onChange, style }) {
  const opts = options.map(o => typeof o === 'string' ? { value: o, label: o } : o);
  return <div className="cp-seg" role="tablist" style={style}>{opts.map(o => <button key={o.value} type="button" role="tab" aria-selected={o.value === value} className={'cp-seg__opt' + (o.value === value ? ' cp-seg__opt--on' : '')} onClick={() => onChange && onChange(o.value)}>{o.label}</button>)}</div>;
}
