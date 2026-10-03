import React from 'react';
import { Icon } from './Icon.jsx';
export function Badge({ tone = 'neutral', dot, icon, className, style, children }) {
  return <span className={['cp-badge', tone !== 'neutral' && 'cp-badge--' + tone, dot && 'cp-badge--dot', className].filter(Boolean).join(' ')} style={style}>{icon && <Icon name={icon} size={13} />}{children}</span>;
}
