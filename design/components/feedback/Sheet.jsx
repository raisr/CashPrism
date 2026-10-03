import React from 'react';
import { IconButton } from '../core/IconButton.jsx';
export function Sheet({ open, onClose, title, subtitle, leading, footer, children }) {
  React.useEffect(() => { if (!open) return; const k = e => e.key === 'Escape' && onClose && onClose(); window.addEventListener('keydown', k); return () => window.removeEventListener('keydown', k); }, [open, onClose]);
  if (!open) return null;
  return <><div className="cp-sheet-scrim" onClick={onClose} /><aside className="cp-sheet" role="dialog" aria-modal="true">
    <header className="cp-sheet__head">{leading}<div style={{ flex: 1, minWidth: 0 }}><div className="cp-h2">{title}</div>{subtitle && <div className="cp-small cp-muted">{subtitle}</div>}</div><IconButton icon="x" aria-label="Schließen" onClick={onClose} /></header>
    <div className="cp-sheet__body">{children}</div>
    {footer && <footer className="cp-sheet__foot">{footer}</footer>}
  </aside></>;
}
