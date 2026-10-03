import React from 'react';
export function PageHeader({ eyebrow, title, lead, actions, style }) {
  return <div className="cp-pagehead" style={style}>
    <div className="cp-pagehead__text">{eyebrow && <div className="cp-label" style={{ marginBottom: 8 }}>{eyebrow}</div>}<h1 className="cp-h1">{title}</h1>{lead && <p className="cp-pagehead__lead" style={{ margin: '6px 0 0' }}>{lead}</p>}</div>
    {actions && <div className="cp-pagehead__actions">{actions}</div>}
  </div>;
}
