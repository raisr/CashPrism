import React from 'react';
export function Topbar({ section, children, style }) {
  return <header className="cp-topbar" style={style}>{section && <span className="cp-topbar__section">{section}</span>}{children}</header>;
}
