import React from 'react';
import { Icon } from './Icon.jsx';
export function IconButton({ icon, variant = 'ghost', size = 'md', dot, disabled, onClick, 'aria-label': label, className, style }) {
  return <button type="button" className={['cp-iconbtn', variant === 'outlined' && 'cp-iconbtn--outlined', size === 'sm' && 'cp-iconbtn--sm', className].filter(Boolean).join(' ')} style={style} disabled={disabled} onClick={onClick} aria-label={label} title={label}><Icon name={icon} />{dot && <span className="cp-iconbtn__dot" />}</button>;
}
