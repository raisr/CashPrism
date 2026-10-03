import React from 'react';
import { Icon } from '../core/Icon.jsx';
import { CategoryIcon } from '../core/CategoryIcon.jsx';
export function Insight({ icon = 'lightbulb', color = 'var(--cp-primary)', title, sub, onClick }) {
  return <div className="cp-insight" onClick={onClick}><CategoryIcon icon={icon} color={color} size={36} /><div className="cp-insight__text"><div className="cp-insight__title">{title}</div>{sub && <div className="cp-insight__sub">{sub}</div>}</div><Icon name="chevron-right" /></div>;
}
