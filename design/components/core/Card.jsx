import React from 'react';
export function Card({ title, subtitle, action, padded = true, flat, className, style, bodyStyle, children }) {
  return <section className={['cp-card', flat && 'cp-card--flat', className].filter(Boolean).join(' ')} style={style}>
    {(title || action) && <header className="cp-card__head"><div className="cp-card__title">{title && <h2 className="cp-h3">{title}</h2>}{subtitle && <div className="cp-card__sub">{subtitle}</div>}</div>{action}</header>}
    {padded ? <div className="cp-card__body" style={bodyStyle}>{children}</div> : children}
  </section>;
}
