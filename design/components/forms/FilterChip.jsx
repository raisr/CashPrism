import React from 'react';
import { Icon } from '../core/Icon.jsx';
export function FilterChip({ active, icon, onClick, onRemove, children }) {
  return <button type="button" className={'cp-chip' + (active ? ' cp-chip--on' : '')} onClick={onClick}>
    {icon && <Icon name={icon} />}{children}
    {onRemove ? <Icon name="x" className="cp-chip__x" size={14} /> : (!active && <Icon name="chevron-down" size={14} />)}
  </button>;
}
