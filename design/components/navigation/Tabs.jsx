import React from 'react';
export function Tabs({ tabs, value, onChange, style }) {
  return <div className="cp-tabs" role="tablist" style={style}>{tabs.map(t => { const o = typeof t === 'string' ? { value: t, label: t } : t; return <button key={o.value} type="button" role="tab" aria-selected={o.value === value} className={'cp-tab' + (o.value === value ? ' cp-tab--on' : '')} onClick={() => onChange && onChange(o.value)}>{o.label}{o.count != null && <span className="cp-tab__count">{o.count}</span>}</button>; })}</div>;
}
