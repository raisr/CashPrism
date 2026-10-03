import React from 'react';
import { Icon } from '../core/Icon.jsx';
export function Input({ label, icon, placeholder, value, onChange, type = 'text', size = 'md', kbd, style, inputStyle }) {
  const box = <div className={'cp-input' + (size === 'sm' ? ' cp-input--sm' : '')} style={label ? undefined : style}>
    {icon && <Icon name={icon} />}
    <input type={type} placeholder={placeholder} value={value} onChange={e => onChange && onChange(e.target.value)} style={inputStyle} />
    {kbd && <span className="cp-input__kbd">{kbd}</span>}
  </div>;
  return label ? <label className="cp-field" style={style}><span className="cp-field__label">{label}</span>{box}</label> : box;
}
