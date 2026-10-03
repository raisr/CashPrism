import React from 'react';
export function Switch({ checked, onChange, label, style }) {
  const sw = <button type="button" role="switch" aria-checked={!!checked} className={'cp-switch' + (checked ? ' cp-switch--on' : '')} onClick={() => onChange && onChange(!checked)} />;
  return label ? <label className="cp-switch-row" style={style}>{sw}<span>{label}</span></label> : sw;
}
