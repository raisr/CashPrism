import React from 'react';
import { Icon } from './Icon.jsx';
export function Button({ variant = 'primary', size = 'md', icon, iconEnd, block, href, disabled, onClick, type = 'button', className, style, children }) {
  const cls = ['cp-btn', 'cp-btn--' + variant, size !== 'md' && 'cp-btn--' + size, block && 'cp-btn--block', className].filter(Boolean).join(' ');
  const inner = <>{icon && <Icon name={icon} />}{children != null && <span>{children}</span>}{iconEnd && <Icon name={iconEnd} />}</>;
  if (href) return <a className={cls} style={style} href={disabled ? undefined : href} aria-disabled={disabled || undefined} onClick={onClick}>{inner}</a>;
  return <button type={type} className={cls} style={style} disabled={disabled} onClick={onClick}>{inner}</button>;
}
