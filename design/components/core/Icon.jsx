import React from 'react';
/** Lucide glyph from the bundled icon font (assets/icons/lucide.css). */
export function Icon({ name, size, color, className, style, title }) {
  return <i className={'cp-icon icon-' + name + (className ? ' ' + className : '')} style={{ ...(size ? { fontSize: size } : null), ...(color ? { color } : null), ...style }} aria-hidden={title ? undefined : true} title={title} />;
}
