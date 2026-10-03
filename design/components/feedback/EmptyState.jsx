import React from 'react';
import { Icon } from '../core/Icon.jsx';
export function EmptyState({ icon = 'inbox', title, text, actions, style }) {
  return <div className="cp-empty" style={style}><span className="cp-empty__icon"><Icon name={icon} /></span><div className="cp-h2">{title}</div>{text && <div className="cp-empty__text">{text}</div>}{actions && <div className="cp-empty__actions">{actions}</div>}</div>;
}
