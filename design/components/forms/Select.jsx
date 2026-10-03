import React from 'react';
import { Icon } from '../core/Icon.jsx';
export function Select({ label, icon, options, value, onChange, size = 'md', style }) {
  const opts = options.map(o => typeof o === 'string' ? { value: o, label: o } : o);
  const box = <div className={'cp-input cp-input--select' + (size === 'sm' ? ' cp-input--sm' : '')} style={label ? undefined : style}>
    {icon && <Icon name={icon} />}
    <select value={value} onChange={e => onChange && onChange(e.target.value)}>{opts.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select>
    <Icon name="chevron-down" />
  </div>;
  return label ? <label className="cp-field" style={style}><span className="cp-field__label">{label}</span>{box}</label> : box;
}
