import React from 'react';
import { Icon } from './Icon.jsx';
export function CategoryIcon({ icon, color = 'var(--cp-prism-slate)', size = 36, shape = 'square', solid, style }) {
  return <span className={'cp-caticon' + (shape === 'round' ? ' cp-caticon--round' : '') + (solid ? ' cp-caticon--solid' : '')} style={{ '--c': color, width: size, height: size, ...style }}><Icon name={icon} size={Math.round(size * 0.5)} /></span>;
}
