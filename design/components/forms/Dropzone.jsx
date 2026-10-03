import React from 'react';
import { Icon } from '../core/Icon.jsx';
export function Dropzone({ accept = '.xlsx', title = 'Finanzguru-Export hierher ziehen', hint = 'oder klicken, um eine .xlsx-Datei auszuwählen', icon = 'file-spreadsheet', onFile, disabled }) {
  const ref = React.useRef(null); const [over, setOver] = React.useState(false);
  const take = f => { if (f && onFile && !disabled) onFile(f); };
  return <div className={'cp-dropzone' + (over ? ' cp-dropzone--over' : '')} style={disabled ? { opacity: .5, pointerEvents: 'none' } : undefined}
    onClick={() => ref.current && ref.current.click()} onDragOver={e => { e.preventDefault(); setOver(true); }} onDragLeave={() => setOver(false)}
    onDrop={e => { e.preventDefault(); setOver(false); take(e.dataTransfer.files[0]); }}>
    <input ref={ref} type="file" accept={accept} onChange={e => { take(e.target.files[0]); e.target.value = ''; }} />
    <span className="cp-empty__icon" style={{ marginBottom: 6 }}><Icon name={icon} /></span>
    <div className="cp-h3">{title}</div><div className="cp-small cp-muted">{hint}</div>
  </div>;
}
