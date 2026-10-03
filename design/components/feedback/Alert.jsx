import React from 'react';
import { Icon } from '../core/Icon.jsx';
const TONES = { success: 'var(--cp-success)', danger: 'var(--cp-danger)', warning: 'var(--cp-warning)', info: 'var(--cp-info)' };
const ICONS = { success: 'circle-check', danger: 'circle-alert', warning: 'triangle-alert', info: 'info' };
export function Alert({ tone = 'info', title, details, icon, action, children, style }) {
  return <div role="status" className={'cp-alert cp-alert--' + tone} style={{ '--c': TONES[tone], ...style }}>
    <Icon name={icon || ICONS[tone]} />
    <div style={{ flex: 1, minWidth: 0 }}>{title && <div className="cp-alert__title">{title}</div>}
      {(children || details) && <div className="cp-alert__body">{children}{details && details.length > 0 && <ul>{details.map((d, i) => <li key={i}>{d}</li>)}</ul>}</div>}</div>
    {action}
  </div>;
}
