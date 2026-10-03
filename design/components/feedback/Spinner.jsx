import React from 'react';
export function Spinner({ size = 20, color, style }) { return <span className="cp-spinner" role="progressbar" style={{ width: size, height: size, ...(color ? { color } : null), ...style }} />; }
