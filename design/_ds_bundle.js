/* @ds-bundle: {"format":4,"namespace":"CashPrismDesignSystem_24fa3e","components":[{"name":"AreaChart","sourcePath":"components/charts/AreaChart.jsx"},{"name":"BarChart","sourcePath":"components/charts/BarChart.jsx"},{"name":"DonutChart","sourcePath":"components/charts/DonutChart.jsx"},{"name":"Sparkline","sourcePath":"components/charts/Sparkline.jsx"},{"name":"Badge","sourcePath":"components/core/Badge.jsx"},{"name":"Button","sourcePath":"components/core/Button.jsx"},{"name":"Card","sourcePath":"components/core/Card.jsx"},{"name":"CategoryIcon","sourcePath":"components/core/CategoryIcon.jsx"},{"name":"Icon","sourcePath":"components/core/Icon.jsx"},{"name":"IconButton","sourcePath":"components/core/IconButton.jsx"},{"name":"Amount","sourcePath":"components/data/Amount.jsx"},{"name":"DataTable","sourcePath":"components/data/DataTable.jsx"},{"name":"Legend","sourcePath":"components/data/Legend.jsx"},{"name":"Pager","sourcePath":"components/data/Pager.jsx"},{"name":"ProgressBar","sourcePath":"components/data/ProgressBar.jsx"},{"name":"StatCard","sourcePath":"components/data/StatCard.jsx"},{"name":"TransactionList","sourcePath":"components/data/TransactionList.jsx"},{"name":"Alert","sourcePath":"components/feedback/Alert.jsx"},{"name":"EmptyState","sourcePath":"components/feedback/EmptyState.jsx"},{"name":"Insight","sourcePath":"components/feedback/Insight.jsx"},{"name":"Sheet","sourcePath":"components/feedback/Sheet.jsx"},{"name":"Spinner","sourcePath":"components/feedback/Spinner.jsx"},{"name":"Dropzone","sourcePath":"components/forms/Dropzone.jsx"},{"name":"FilterChip","sourcePath":"components/forms/FilterChip.jsx"},{"name":"Input","sourcePath":"components/forms/Input.jsx"},{"name":"Segmented","sourcePath":"components/forms/Segmented.jsx"},{"name":"Select","sourcePath":"components/forms/Select.jsx"},{"name":"Switch","sourcePath":"components/forms/Switch.jsx"},{"name":"Brand","sourcePath":"components/navigation/Brand.jsx"},{"name":"PageHeader","sourcePath":"components/navigation/PageHeader.jsx"},{"name":"Sidebar","sourcePath":"components/navigation/Sidebar.jsx"},{"name":"Tabs","sourcePath":"components/navigation/Tabs.jsx"},{"name":"Topbar","sourcePath":"components/navigation/Topbar.jsx"}],"sourceHashes":{"components/charts/AreaChart.jsx":"5d21aa785049","components/charts/BarChart.jsx":"3b32d67bed46","components/charts/DonutChart.jsx":"65d6f2c684d6","components/charts/Sparkline.jsx":"b0fd48d95f0a","components/core/Badge.jsx":"1353fa529151","components/core/Button.jsx":"64a1e578475e","components/core/Card.jsx":"5db514086270","components/core/CategoryIcon.jsx":"0b0772290d17","components/core/Icon.jsx":"2431145b88ec","components/core/IconButton.jsx":"1eea184fe7df","components/data/Amount.jsx":"c6ada2502e91","components/data/DataTable.jsx":"deba2706e694","components/data/Legend.jsx":"bf6208cb6b05","components/data/Pager.jsx":"7e96f59cce8e","components/data/ProgressBar.jsx":"e71cc94ba544","components/data/StatCard.jsx":"4b0426fe2118","components/data/TransactionList.jsx":"b58035ca1db4","components/feedback/Alert.jsx":"4c09b78efb7c","components/feedback/EmptyState.jsx":"d4fdda2f9683","components/feedback/Insight.jsx":"45bb86cff1ec","components/feedback/Sheet.jsx":"672ff56e5e04","components/feedback/Spinner.jsx":"ccae7d6f7012","components/forms/Dropzone.jsx":"66e884807bad","components/forms/FilterChip.jsx":"94af0e350f69","components/forms/Input.jsx":"ca078893ded7","components/forms/Segmented.jsx":"4ada625a9a81","components/forms/Select.jsx":"c40bd57c0f58","components/forms/Switch.jsx":"b98f403efcb4","components/navigation/Brand.jsx":"f9c70940698b","components/navigation/PageHeader.jsx":"96d97bf9a2ff","components/navigation/Sidebar.jsx":"340c6abdf54a","components/navigation/Tabs.jsx":"e8ff684b1681","components/navigation/Topbar.jsx":"beee1f642f1b","ui_kits/app/Accounts.jsx":"9859264dbbf0","ui_kits/app/Analysis.jsx":"6d4328caa80f","ui_kits/app/App.jsx":"de834696be4d","ui_kits/app/Bookings.jsx":"532e5c2ade80","ui_kits/app/Dashboard.jsx":"f82549d5c2b1","ui_kits/app/Import.jsx":"4e753a1c5c23","ui_kits/app/Reports.jsx":"7937be58b323","ui_kits/app/Shell.jsx":"c4d91daeeffc","ui_kits/app/data.js":"666eaa6db9f7"},"inlinedExternals":[],"unexposedExports":[{"name":"formatMoney","sourcePath":"components/data/Amount.jsx"}]} */

(() => {

const __ds_ns = (window.CashPrismDesignSystem_24fa3e = window.CashPrismDesignSystem_24fa3e || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/charts/AreaChart.jsx
try { (() => {
function useWidth() {
  const ref = React.useRef(null);
  const [w, setW] = React.useState(0);
  React.useLayoutEffect(() => {
    const el = ref.current;
    if (!el) return;
    const ro = new ResizeObserver(e => setW(e[0].contentRect.width));
    ro.observe(el);
    setW(el.getBoundingClientRect().width);
    return () => ro.disconnect();
  }, []);
  return [ref, w];
}
function niceMax(v) {
  if (v <= 0) return 1;
  const p = Math.pow(10, Math.floor(Math.log10(v)));
  const n = v / p;
  return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10) * p;
}
function smooth(pts) {
  if (!pts.length) return '';
  let d = 'M' + pts[0][0] + ',' + pts[0][1];
  for (let i = 0; i < pts.length - 1; i++) {
    const p0 = pts[i - 1] || pts[i],
      p1 = pts[i],
      p2 = pts[i + 1],
      p3 = pts[i + 2] || p2,
      t = 0.17;
    d += ' C' + (p1[0] + (p2[0] - p0[0]) * t) + ',' + (p1[1] + (p2[1] - p0[1]) * t) + ' ' + (p2[0] - (p3[0] - p1[0]) * t) + ',' + (p2[1] - (p3[1] - p1[1]) * t) + ' ' + p2[0] + ',' + p2[1];
  }
  return d;
}
function AreaChart({
  labels,
  series,
  height = 260,
  format = v => String(v),
  formatAxis,
  tooltipTitle,
  area = true,
  style
}) {
  const [ref, w] = useWidth();
  const [hi, setHi] = React.useState(null);
  const uid = React.useId().replace(/:/g, '');
  const W = Math.max(w, 240),
    pl = 52,
    pr = 12,
    pt = 12,
    pb = 28,
    n = labels.length;
  const all = series.flatMap(s => s.values);
  const max = niceMax(Math.max(...all, 0) * 1.05);
  const minRaw = Math.min(0, ...all);
  const min = minRaw < 0 ? -niceMax(-minRaw) : 0;
  const x = i => pl + (n === 1 ? 0 : i * (W - pl - pr) / (n - 1));
  const y = v => pt + (max - v) / (max - min) * (height - pt - pb);
  const ticks = [0, 1, 2, 3, 4].map(k => min + (max - min) * k / 4);
  const step = Math.max(1, Math.ceil(n / Math.max(2, Math.floor((W - pl) / 64))));
  const fa = formatAxis || format;
  const move = e => {
    const r = e.currentTarget.getBoundingClientRect();
    const px = e.clientX - r.left;
    const i = Math.round((px - pl) / ((W - pl - pr) / Math.max(1, n - 1)));
    setHi(Math.max(0, Math.min(n - 1, i)));
  };
  return /*#__PURE__*/React.createElement("div", {
    ref: ref,
    className: "cp-chart",
    style: style
  }, w > 0 && /*#__PURE__*/React.createElement("svg", {
    width: W,
    height: height,
    onMouseMove: move,
    onMouseLeave: () => setHi(null)
  }, /*#__PURE__*/React.createElement("defs", null, series.map((s, si) => /*#__PURE__*/React.createElement("linearGradient", {
    key: si,
    id: uid + 'g' + si,
    x1: "0",
    x2: "0",
    y1: "0",
    y2: "1"
  }, /*#__PURE__*/React.createElement("stop", {
    offset: "0",
    stopColor: s.color,
    stopOpacity: "0.24"
  }), /*#__PURE__*/React.createElement("stop", {
    offset: "1",
    stopColor: s.color,
    stopOpacity: "0"
  })))), ticks.map((t, i) => /*#__PURE__*/React.createElement("g", {
    key: i
  }, /*#__PURE__*/React.createElement("line", {
    x1: pl,
    x2: W - pr,
    y1: y(t),
    y2: y(t),
    stroke: "var(--cp-chart-grid)",
    strokeDasharray: t === 0 ? undefined : '3 4'
  }), /*#__PURE__*/React.createElement("text", {
    x: pl - 10,
    y: y(t) + 4,
    textAnchor: "end"
  }, fa(t)))), labels.map((l, i) => i % step === 0 || i === n - 1 ? /*#__PURE__*/React.createElement("text", {
    key: i,
    x: x(i),
    y: height - 8,
    textAnchor: i === 0 ? 'start' : i === n - 1 ? 'end' : 'middle'
  }, l) : null), hi != null && /*#__PURE__*/React.createElement("line", {
    x1: x(hi),
    x2: x(hi),
    y1: pt,
    y2: height - pb,
    stroke: "var(--cp-line-strong)"
  }), series.map((s, si) => {
    const pts = s.values.map((v, i) => [x(i), y(v)]);
    const d = smooth(pts);
    return /*#__PURE__*/React.createElement("g", {
      key: si
    }, area && s.area !== false && /*#__PURE__*/React.createElement("path", {
      d: d + ' L' + x(n - 1) + ',' + y(Math.max(min, 0)) + ' L' + x(0) + ',' + y(Math.max(min, 0)) + ' Z',
      fill: 'url(#' + uid + 'g' + si + ')'
    }), /*#__PURE__*/React.createElement("path", {
      d: d,
      fill: "none",
      stroke: s.color,
      strokeWidth: "2.5",
      strokeLinecap: "round",
      strokeDasharray: s.dashed ? '5 5' : undefined
    }), hi != null && /*#__PURE__*/React.createElement("circle", {
      cx: x(hi),
      cy: y(s.values[hi]),
      r: "4.5",
      fill: "var(--cp-surface)",
      stroke: s.color,
      strokeWidth: "2.5"
    }));
  })), hi != null && w > 0 && /*#__PURE__*/React.createElement("div", {
    className: "cp-chart__tip",
    style: {
      left: Math.min(Math.max(x(hi) + 12, 0), W - 170),
      top: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-chart__tip-title"
  }, tooltipTitle ? tooltipTitle(hi) : labels[hi]), series.map((s, si) => /*#__PURE__*/React.createElement("div", {
    key: si,
    className: "cp-chart__tip-row"
  }, /*#__PURE__*/React.createElement("span", {
    className: "cp-legend__swatch cp-legend__swatch--line",
    style: {
      '--c': s.color
    }
  }), s.name, /*#__PURE__*/React.createElement("b", null, format(s.values[hi]))))));
}
Object.assign(__ds_scope, { AreaChart });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/charts/AreaChart.jsx", error: String((e && e.message) || e) }); }

// components/charts/BarChart.jsx
try { (() => {
function useWidth() {
  const ref = React.useRef(null);
  const [w, setW] = React.useState(0);
  React.useLayoutEffect(() => {
    const el = ref.current;
    if (!el) return;
    const ro = new ResizeObserver(e => setW(e[0].contentRect.width));
    ro.observe(el);
    setW(el.getBoundingClientRect().width);
    return () => ro.disconnect();
  }, []);
  return [ref, w];
}
function niceMax(v) {
  if (v <= 0) return 1;
  const p = Math.pow(10, Math.floor(Math.log10(v)));
  const n = v / p;
  return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10) * p;
}
function BarChart({
  labels,
  series,
  stacked,
  height = 240,
  format = v => String(v),
  formatAxis,
  highlight,
  onSelect,
  style
}) {
  const [ref, w] = useWidth();
  const [hi, setHi] = React.useState(null);
  const W = Math.max(w, 240),
    pl = 52,
    pr = 8,
    pt = 12,
    pb = 28,
    n = labels.length;
  const totals = labels.map((_, i) => stacked ? series.reduce((a, s) => a + s.values[i], 0) : Math.max(...series.map(s => s.values[i])));
  const max = niceMax(Math.max(...totals, 0) * 1.05);
  const y = v => pt + (max - v) / max * (height - pt - pb);
  const band = (W - pl - pr) / n;
  const groupW = Math.min(band * 0.62, stacked ? 36 : 22 * series.length + 4 * (series.length - 1));
  const bw = stacked ? groupW : (groupW - 4 * (series.length - 1)) / series.length;
  const ticks = [0, 1, 2, 3, 4].map(k => max * k / 4);
  const fa = formatAxis || format;
  const step = Math.max(1, Math.ceil(n / Math.max(2, Math.floor((W - pl) / 48))));
  const act = hi != null ? hi : highlight;
  return /*#__PURE__*/React.createElement("div", {
    ref: ref,
    className: "cp-chart",
    style: style
  }, w > 0 && /*#__PURE__*/React.createElement("svg", {
    width: W,
    height: height,
    onMouseLeave: () => setHi(null)
  }, ticks.map((t, i) => /*#__PURE__*/React.createElement("g", {
    key: i
  }, /*#__PURE__*/React.createElement("line", {
    x1: pl,
    x2: W - pr,
    y1: y(t),
    y2: y(t),
    stroke: "var(--cp-chart-grid)",
    strokeDasharray: t === 0 ? undefined : '3 4'
  }), /*#__PURE__*/React.createElement("text", {
    x: pl - 10,
    y: y(t) + 4,
    textAnchor: "end"
  }, fa(t)))), labels.map((l, i) => {
    const cx = pl + band * i + band / 2;
    let acc = 0;
    const dim = act != null && act !== i;
    return /*#__PURE__*/React.createElement("g", {
      key: i,
      onMouseEnter: () => setHi(i),
      onClick: onSelect ? () => onSelect(i) : undefined,
      style: {
        cursor: onSelect ? 'pointer' : 'default'
      }
    }, /*#__PURE__*/React.createElement("rect", {
      x: pl + band * i,
      y: pt,
      width: band,
      height: height - pt - pb,
      fill: hi === i ? 'var(--cp-hover)' : 'transparent',
      rx: "6"
    }), series.map((s, si) => {
      const v = s.values[i];
      if (stacked) {
        const y0 = y(acc),
          y1 = y(acc + v);
        acc += v;
        return /*#__PURE__*/React.createElement("rect", {
          key: si,
          x: cx - bw / 2,
          y: y1,
          width: bw,
          height: Math.max(0, y0 - y1 - (si < series.length - 1 ? 1.5 : 0)),
          rx: si === series.length - 1 ? 4 : 1,
          fill: s.color,
          opacity: dim ? 0.35 : 1
        });
      }
      const bx = cx - groupW / 2 + si * (bw + 4);
      return /*#__PURE__*/React.createElement("rect", {
        key: si,
        x: bx,
        y: y(v),
        width: bw,
        height: Math.max(0, y(0) - y(v)),
        rx: "4",
        fill: s.color,
        opacity: dim ? 0.35 : 1
      });
    }), i % step === 0 && /*#__PURE__*/React.createElement("text", {
      x: cx,
      y: height - 8,
      textAnchor: "middle",
      style: act === i ? {
        fill: 'var(--cp-text)'
      } : undefined
    }, l));
  })), hi != null && w > 0 && /*#__PURE__*/React.createElement("div", {
    className: "cp-chart__tip",
    style: {
      left: Math.min(pl + band * hi + band / 2 + 14, W - 170),
      top: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-chart__tip-title"
  }, labels[hi]), series.map((s, si) => /*#__PURE__*/React.createElement("div", {
    key: si,
    className: "cp-chart__tip-row"
  }, /*#__PURE__*/React.createElement("span", {
    className: "cp-legend__swatch",
    style: {
      '--c': s.color
    }
  }), s.name, /*#__PURE__*/React.createElement("b", null, format(s.values[hi])))), stacked && series.length > 1 && /*#__PURE__*/React.createElement("div", {
    className: "cp-chart__tip-row",
    style: {
      marginTop: 4,
      paddingTop: 6,
      borderTop: '1px solid var(--cp-line)'
    }
  }, "Summe", /*#__PURE__*/React.createElement("b", null, format(totals[hi])))));
}
Object.assign(__ds_scope, { BarChart });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/charts/BarChart.jsx", error: String((e && e.message) || e) }); }

// components/charts/DonutChart.jsx
try { (() => {
function DonutChart({
  data,
  size = 200,
  thickness = 22,
  centerLabel,
  centerValue,
  format = v => String(v),
  active,
  onActiveChange,
  style
}) {
  const [hiState, setHi] = React.useState(null);
  const hi = active !== undefined ? active : hiState;
  const set = i => {
    setHi(i);
    onActiveChange && onActiveChange(i);
  };
  const total = data.reduce((a, d) => a + d.value, 0) || 1;
  const r = (size - thickness) / 2;
  const c = 2 * Math.PI * r;
  const gap = data.length > 1 ? 3 : 0;
  let acc = 0;
  const cur = hi != null ? data[hi] : null;
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-chart",
    style: {
      width: size,
      height: size,
      flexShrink: 0,
      ...style
    }
  }, /*#__PURE__*/React.createElement("svg", {
    width: size,
    height: size,
    style: {
      transform: 'rotate(-90deg)'
    },
    onMouseLeave: () => set(null)
  }, /*#__PURE__*/React.createElement("circle", {
    cx: size / 2,
    cy: size / 2,
    r: r,
    fill: "none",
    stroke: "var(--cp-surface-3)",
    strokeWidth: thickness
  }), data.map((d, i) => {
    const len = d.value / total * c;
    const off = acc;
    acc += len;
    return /*#__PURE__*/React.createElement("circle", {
      key: i,
      cx: size / 2,
      cy: size / 2,
      r: r,
      fill: "none",
      stroke: d.color,
      strokeWidth: hi === i ? thickness + 6 : thickness,
      strokeDasharray: Math.max(0, len - gap) + ' ' + (c - Math.max(0, len - gap)),
      strokeDashoffset: -off,
      opacity: hi != null && hi !== i ? 0.35 : 1,
      style: {
        transition: 'stroke-width 160ms, opacity 160ms',
        cursor: 'pointer'
      },
      onMouseEnter: () => set(i)
    });
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      textAlign: 'center',
      pointerEvents: 'none',
      padding: thickness + 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-caption cp-muted",
    style: {
      fontWeight: 600,
      maxWidth: '100%',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, cur ? cur.label : centerLabel), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: size > 170 ? 22 : 17,
      fontWeight: 750,
      letterSpacing: '-0.02em',
      fontVariantNumeric: 'tabular-nums',
      whiteSpace: 'nowrap'
    }
  }, cur ? format(cur.value) : centerValue), cur && /*#__PURE__*/React.createElement("div", {
    className: "cp-caption cp-faint",
    style: {
      fontWeight: 650
    }
  }, Math.round(cur.value / total * 100), " %")));
}
Object.assign(__ds_scope, { DonutChart });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/charts/DonutChart.jsx", error: String((e && e.message) || e) }); }

// components/charts/Sparkline.jsx
try { (() => {
function Sparkline({
  values,
  color = 'var(--cp-primary)',
  width = 96,
  height = 32,
  area = true,
  style
}) {
  const max = Math.max(...values),
    min = Math.min(...values),
    n = values.length;
  const uid = React.useId().replace(/:/g, '');
  const pts = values.map((v, i) => [i * (width - 4) / Math.max(1, n - 1) + 2, 2 + (max - v) / (max - min || 1) * (height - 4)]);
  const d = 'M' + pts.map(p => p.join(',')).join(' L');
  return /*#__PURE__*/React.createElement("svg", {
    width: width,
    height: height,
    style: {
      display: 'block',
      overflow: 'visible',
      ...style
    }
  }, /*#__PURE__*/React.createElement("defs", null, /*#__PURE__*/React.createElement("linearGradient", {
    id: uid,
    x1: "0",
    x2: "0",
    y1: "0",
    y2: "1"
  }, /*#__PURE__*/React.createElement("stop", {
    offset: "0",
    stopColor: color,
    stopOpacity: "0.25"
  }), /*#__PURE__*/React.createElement("stop", {
    offset: "1",
    stopColor: color,
    stopOpacity: "0"
  }))), area && /*#__PURE__*/React.createElement("path", {
    d: d + ' L' + pts[n - 1][0] + ',' + height + ' L' + pts[0][0] + ',' + height + ' Z',
    fill: 'url(#' + uid + ')'
  }), /*#__PURE__*/React.createElement("path", {
    d: d,
    fill: "none",
    stroke: color,
    strokeWidth: "2",
    strokeLinejoin: "round",
    strokeLinecap: "round"
  }), /*#__PURE__*/React.createElement("circle", {
    cx: pts[n - 1][0],
    cy: pts[n - 1][1],
    r: "2.5",
    fill: color
  }));
}
Object.assign(__ds_scope, { Sparkline });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/charts/Sparkline.jsx", error: String((e && e.message) || e) }); }

// components/core/Card.jsx
try { (() => {
function Card({
  title,
  subtitle,
  action,
  padded = true,
  flat,
  className,
  style,
  bodyStyle,
  children
}) {
  return /*#__PURE__*/React.createElement("section", {
    className: ['cp-card', flat && 'cp-card--flat', className].filter(Boolean).join(' '),
    style: style
  }, (title || action) && /*#__PURE__*/React.createElement("header", {
    className: "cp-card__head"
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-card__title"
  }, title && /*#__PURE__*/React.createElement("h2", {
    className: "cp-h3"
  }, title), subtitle && /*#__PURE__*/React.createElement("div", {
    className: "cp-card__sub"
  }, subtitle)), action), padded ? /*#__PURE__*/React.createElement("div", {
    className: "cp-card__body",
    style: bodyStyle
  }, children) : children);
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Card.jsx", error: String((e && e.message) || e) }); }

// components/core/Icon.jsx
try { (() => {
/** Lucide glyph from the bundled icon font (assets/icons/lucide.css). */
function Icon({
  name,
  size,
  color,
  className,
  style,
  title
}) {
  return /*#__PURE__*/React.createElement("i", {
    className: 'cp-icon icon-' + name + (className ? ' ' + className : ''),
    style: {
      ...(size ? {
        fontSize: size
      } : null),
      ...(color ? {
        color
      } : null),
      ...style
    },
    "aria-hidden": title ? undefined : true,
    title: title
  });
}
Object.assign(__ds_scope, { Icon });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Icon.jsx", error: String((e && e.message) || e) }); }

// components/core/Badge.jsx
try { (() => {
function Badge({
  tone = 'neutral',
  dot,
  icon,
  className,
  style,
  children
}) {
  return /*#__PURE__*/React.createElement("span", {
    className: ['cp-badge', tone !== 'neutral' && 'cp-badge--' + tone, dot && 'cp-badge--dot', className].filter(Boolean).join(' '),
    style: style
  }, icon && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 13
  }), children);
}
Object.assign(__ds_scope, { Badge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Badge.jsx", error: String((e && e.message) || e) }); }

// components/core/Button.jsx
try { (() => {
function Button({
  variant = 'primary',
  size = 'md',
  icon,
  iconEnd,
  block,
  href,
  disabled,
  onClick,
  type = 'button',
  className,
  style,
  children
}) {
  const cls = ['cp-btn', 'cp-btn--' + variant, size !== 'md' && 'cp-btn--' + size, block && 'cp-btn--block', className].filter(Boolean).join(' ');
  const inner = /*#__PURE__*/React.createElement(React.Fragment, null, icon && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  }), children != null && /*#__PURE__*/React.createElement("span", null, children), iconEnd && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: iconEnd
  }));
  if (href) return /*#__PURE__*/React.createElement("a", {
    className: cls,
    style: style,
    href: disabled ? undefined : href,
    "aria-disabled": disabled || undefined,
    onClick: onClick
  }, inner);
  return /*#__PURE__*/React.createElement("button", {
    type: type,
    className: cls,
    style: style,
    disabled: disabled,
    onClick: onClick
  }, inner);
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Button.jsx", error: String((e && e.message) || e) }); }

// components/core/CategoryIcon.jsx
try { (() => {
function CategoryIcon({
  icon,
  color = 'var(--cp-prism-slate)',
  size = 36,
  shape = 'square',
  solid,
  style
}) {
  return /*#__PURE__*/React.createElement("span", {
    className: 'cp-caticon' + (shape === 'round' ? ' cp-caticon--round' : '') + (solid ? ' cp-caticon--solid' : ''),
    style: {
      '--c': color,
      width: size,
      height: size,
      ...style
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: Math.round(size * 0.5)
  }));
}
Object.assign(__ds_scope, { CategoryIcon });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/CategoryIcon.jsx", error: String((e && e.message) || e) }); }

// components/core/IconButton.jsx
try { (() => {
function IconButton({
  icon,
  variant = 'ghost',
  size = 'md',
  dot,
  disabled,
  onClick,
  'aria-label': label,
  className,
  style
}) {
  return /*#__PURE__*/React.createElement("button", {
    type: "button",
    className: ['cp-iconbtn', variant === 'outlined' && 'cp-iconbtn--outlined', size === 'sm' && 'cp-iconbtn--sm', className].filter(Boolean).join(' '),
    style: style,
    disabled: disabled,
    onClick: onClick,
    "aria-label": label,
    title: label
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  }), dot && /*#__PURE__*/React.createElement("span", {
    className: "cp-iconbtn__dot"
  }));
}
Object.assign(__ds_scope, { IconButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/IconButton.jsx", error: String((e && e.message) || e) }); }

// components/data/Amount.jsx
try { (() => {
/** "1.234,56 €" — German grouping, € for EUR, ISO code for other currencies. */
function formatMoney(cents, currency = 'EUR', {
  sign = false,
  decimals = 2
} = {}) {
  const v = Math.abs(cents) / 100;
  const n = v.toLocaleString('de-DE', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals
  });
  const s = sign ? cents > 0 ? '+' : cents < 0 ? '−' : '' : cents < 0 ? '−' : '';
  return s + n + '\u00a0' + (currency === 'EUR' ? '€' : currency);
}
function Amount({
  cents,
  currency = 'EUR',
  sign = true,
  colored = false,
  neutral,
  dimCents,
  decimals = 2,
  size,
  style
}) {
  const cls = 'cp-amount ' + (cents > 0 ? 'cp-amount--in' : cents < 0 ? 'cp-amount--out' : '') + (colored ? ' cp-amount--colored' : '') + (neutral || sign === false ? ' cp-amount--neutral' : '');
  const txt = formatMoney(cents, currency, {
    sign,
    decimals
  });
  const st = {
    ...(size ? {
      fontSize: size
    } : null),
    ...style
  };
  if (dimCents && decimals) {
    const i = txt.lastIndexOf(',');
    return /*#__PURE__*/React.createElement("span", {
      className: cls,
      style: st
    }, txt.slice(0, i), /*#__PURE__*/React.createElement("span", {
      className: "cp-amount__cents"
    }, txt.slice(i)));
  }
  return /*#__PURE__*/React.createElement("span", {
    className: cls,
    style: st
  }, txt);
}
Object.assign(__ds_scope, { formatMoney, Amount });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/Amount.jsx", error: String((e && e.message) || e) }); }

// components/data/DataTable.jsx
try { (() => {
function DataTable({
  columns,
  rows,
  rowKey = 'id',
  sort,
  onSortChange,
  onRowClick,
  selectedKey,
  groupBy,
  compact,
  style
}) {
  let last = null;
  const body = [];
  rows.forEach((row, i) => {
    if (groupBy) {
      const g = groupBy(row);
      if (g !== last) {
        last = g;
        body.push(/*#__PURE__*/React.createElement("tr", {
          key: 'g' + g + i,
          className: "cp-table__group"
        }, /*#__PURE__*/React.createElement("td", {
          colSpan: columns.length
        }, g)));
      }
    }
    const k = row[rowKey] ?? i;
    body.push(/*#__PURE__*/React.createElement("tr", {
      key: k,
      className: selectedKey === k ? 'cp-row--selected' : undefined,
      onClick: onRowClick ? () => onRowClick(row) : undefined
    }, columns.map(c => /*#__PURE__*/React.createElement("td", {
      key: c.key,
      className: c.align === 'right' ? 'cp-right' : undefined,
      style: c.width ? {
        width: c.width
      } : undefined
    }, c.render ? c.render(row) : row[c.key]))));
  });
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-table-wrap",
    style: style
  }, /*#__PURE__*/React.createElement("table", {
    className: 'cp-table' + (onRowClick ? ' cp-table--hover' : '') + (compact ? ' cp-table--compact' : '')
  }, /*#__PURE__*/React.createElement("thead", null, /*#__PURE__*/React.createElement("tr", null, columns.map(c => {
    const on = sort && sort.key === c.key;
    return /*#__PURE__*/React.createElement("th", {
      key: c.key,
      className: c.align === 'right' ? 'cp-right' : undefined
    }, c.sortable ? /*#__PURE__*/React.createElement("span", {
      className: 'cp-sort' + (on ? ' cp-sort--on' : ''),
      onClick: () => onSortChange && onSortChange({
        key: c.key,
        desc: on ? !sort.desc : true
      })
    }, c.title, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
      name: on && !sort.desc ? 'arrow-up' : 'arrow-down'
    })) : c.title);
  }))), /*#__PURE__*/React.createElement("tbody", null, body)));
}
Object.assign(__ds_scope, { DataTable });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/DataTable.jsx", error: String((e && e.message) || e) }); }

// components/data/Legend.jsx
try { (() => {
function Legend({
  items,
  column,
  line,
  onHover,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: 'cp-legend' + (column ? ' cp-legend--column' : ''),
    style: style
  }, items.map((it, i) => /*#__PURE__*/React.createElement("div", {
    key: it.label,
    className: "cp-legend__item",
    onMouseEnter: onHover ? () => onHover(i) : undefined,
    onMouseLeave: onHover ? () => onHover(null) : undefined
  }, /*#__PURE__*/React.createElement("span", {
    className: 'cp-legend__swatch' + (line ? ' cp-legend__swatch--line' : ''),
    style: {
      '--c': it.color
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, it.label), it.value != null && /*#__PURE__*/React.createElement("span", {
    className: "cp-legend__value"
  }, it.value, it.sub && /*#__PURE__*/React.createElement("span", {
    className: "cp-legend__sub"
  }, it.sub)))));
}
Object.assign(__ds_scope, { Legend });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/Legend.jsx", error: String((e && e.message) || e) }); }

// components/data/Pager.jsx
try { (() => {
function Pager({
  page,
  pageSize,
  total,
  onPageChange,
  noun = 'Einträge'
}) {
  const pages = Math.max(1, Math.ceil(total / pageSize));
  const go = p => onPageChange && onPageChange(Math.max(0, Math.min(pages - 1, p)));
  const from = total ? page * pageSize + 1 : 0,
    to = Math.min(total, (page + 1) * pageSize);
  const near = [];
  for (let p = Math.max(0, page - 2); p <= Math.min(pages - 1, page + 2); p++) near.push(p);
  const de = n => n.toLocaleString('de-DE');
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-pager"
  }, /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("b", {
    style: {
      color: 'var(--cp-text)'
    }
  }, de(from), "\u2013", de(to)), " von ", de(total), " ", noun), /*#__PURE__*/React.createElement("div", {
    className: "cp-pager__nav"
  }, /*#__PURE__*/React.createElement(__ds_scope.IconButton, {
    size: "sm",
    icon: "chevron-left",
    "aria-label": "Vorherige Seite",
    disabled: page === 0,
    onClick: () => go(page - 1)
  }), near[0] > 0 && /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("button", {
    className: "cp-pager__page",
    onClick: () => go(0)
  }, "1"), near[0] > 1 && /*#__PURE__*/React.createElement("span", null, "\u2026")), near.map(p => /*#__PURE__*/React.createElement("button", {
    key: p,
    className: 'cp-pager__page' + (p === page ? ' cp-pager__page--on' : ''),
    onClick: () => go(p)
  }, p + 1)), near[near.length - 1] < pages - 1 && /*#__PURE__*/React.createElement(React.Fragment, null, near[near.length - 1] < pages - 2 && /*#__PURE__*/React.createElement("span", null, "\u2026"), /*#__PURE__*/React.createElement("button", {
    className: "cp-pager__page",
    onClick: () => go(pages - 1)
  }, de(pages))), /*#__PURE__*/React.createElement(__ds_scope.IconButton, {
    size: "sm",
    icon: "chevron-right",
    "aria-label": "N\xE4chste Seite",
    disabled: page >= pages - 1,
    onClick: () => go(page + 1)
  })));
}
Object.assign(__ds_scope, { Pager });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/Pager.jsx", error: String((e && e.message) || e) }); }

// components/data/ProgressBar.jsx
try { (() => {
function ProgressBar({
  value,
  color,
  size = 'md',
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: 'cp-progress' + (size === 'sm' ? ' cp-progress--sm' : ''),
    style: style,
    role: "progressbar",
    "aria-valuenow": Math.round(value * 100)
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-progress__bar",
    style: {
      width: Math.max(0, Math.min(1, value)) * 100 + '%',
      ...(color ? {
        '--c': color
      } : null)
    }
  }));
}
Object.assign(__ds_scope, { ProgressBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/ProgressBar.jsx", error: String((e && e.message) || e) }); }

// components/data/StatCard.jsx
try { (() => {
function StatCard({
  label,
  value,
  icon,
  color = 'var(--cp-primary)',
  delta,
  deltaGood,
  deltaLabel = 'ggü. Vormonat',
  footer,
  style
}) {
  const up = typeof delta === 'string' ? !delta.trim().startsWith('−') && !delta.trim().startsWith('-') : true;
  return /*#__PURE__*/React.createElement("section", {
    className: "cp-card",
    style: style
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-stat"
  }, icon && /*#__PURE__*/React.createElement(__ds_scope.CategoryIcon, {
    icon: icon,
    color: color,
    size: 44,
    shape: "round"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 0,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-stat__label"
  }, label), /*#__PURE__*/React.createElement("div", {
    className: "cp-stat__value"
  }, value), delta && /*#__PURE__*/React.createElement("div", {
    className: "cp-stat__delta"
  }, /*#__PURE__*/React.createElement("b", {
    className: deltaGood === undefined ? '' : deltaGood ? 'cp-delta--good' : 'cp-delta--bad'
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: up ? 'arrow-up-right' : 'arrow-down-right'
  }), delta), deltaLabel), footer)));
}
Object.assign(__ds_scope, { StatCard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/StatCard.jsx", error: String((e && e.message) || e) }); }

// components/data/TransactionList.jsx
try { (() => {
function TransactionList({
  items,
  groupByDay,
  onSelect,
  style
}) {
  let last = null;
  const out = [];
  items.forEach(it => {
    if (groupByDay && it.day !== last) {
      last = it.day;
      out.push(/*#__PURE__*/React.createElement("div", {
        key: 'd' + it.day + it.id,
        className: "cp-txlist__day"
      }, it.day));
    }
    out.push(/*#__PURE__*/React.createElement("div", {
      key: it.id,
      className: "cp-tx",
      onClick: onSelect ? () => onSelect(it) : undefined
    }, /*#__PURE__*/React.createElement(__ds_scope.CategoryIcon, {
      icon: it.icon,
      color: it.color,
      size: 38
    }), /*#__PURE__*/React.createElement("div", {
      className: "cp-tx__main"
    }, /*#__PURE__*/React.createElement("div", {
      className: "cp-tx__title"
    }, it.title), /*#__PURE__*/React.createElement("div", {
      className: "cp-tx__meta"
    }, it.meta)), /*#__PURE__*/React.createElement("div", {
      className: "cp-tx__amount"
    }, /*#__PURE__*/React.createElement(__ds_scope.Amount, {
      cents: it.cents,
      currency: it.currency
    }), it.note && /*#__PURE__*/React.createElement("div", {
      className: "cp-caption cp-faint",
      style: {
        fontWeight: 600
      }
    }, it.note))));
  });
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-txlist",
    style: style
  }, out);
}
Object.assign(__ds_scope, { TransactionList });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/TransactionList.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Alert.jsx
try { (() => {
const TONES = {
  success: 'var(--cp-success)',
  danger: 'var(--cp-danger)',
  warning: 'var(--cp-warning)',
  info: 'var(--cp-info)'
};
const ICONS = {
  success: 'circle-check',
  danger: 'circle-alert',
  warning: 'triangle-alert',
  info: 'info'
};
function Alert({
  tone = 'info',
  title,
  details,
  icon,
  action,
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    role: "status",
    className: 'cp-alert cp-alert--' + tone,
    style: {
      '--c': TONES[tone],
      ...style
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon || ICONS[tone]
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, title && /*#__PURE__*/React.createElement("div", {
    className: "cp-alert__title"
  }, title), (children || details) && /*#__PURE__*/React.createElement("div", {
    className: "cp-alert__body"
  }, children, details && details.length > 0 && /*#__PURE__*/React.createElement("ul", null, details.map((d, i) => /*#__PURE__*/React.createElement("li", {
    key: i
  }, d))))), action);
}
Object.assign(__ds_scope, { Alert });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Alert.jsx", error: String((e && e.message) || e) }); }

// components/feedback/EmptyState.jsx
try { (() => {
function EmptyState({
  icon = 'inbox',
  title,
  text,
  actions,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-empty",
    style: style
  }, /*#__PURE__*/React.createElement("span", {
    className: "cp-empty__icon"
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  })), /*#__PURE__*/React.createElement("div", {
    className: "cp-h2"
  }, title), text && /*#__PURE__*/React.createElement("div", {
    className: "cp-empty__text"
  }, text), actions && /*#__PURE__*/React.createElement("div", {
    className: "cp-empty__actions"
  }, actions));
}
Object.assign(__ds_scope, { EmptyState });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/EmptyState.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Insight.jsx
try { (() => {
function Insight({
  icon = 'lightbulb',
  color = 'var(--cp-primary)',
  title,
  sub,
  onClick
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-insight",
    onClick: onClick
  }, /*#__PURE__*/React.createElement(__ds_scope.CategoryIcon, {
    icon: icon,
    color: color,
    size: 36
  }), /*#__PURE__*/React.createElement("div", {
    className: "cp-insight__text"
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-insight__title"
  }, title), sub && /*#__PURE__*/React.createElement("div", {
    className: "cp-insight__sub"
  }, sub)), /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "chevron-right"
  }));
}
Object.assign(__ds_scope, { Insight });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Insight.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Sheet.jsx
try { (() => {
function Sheet({
  open,
  onClose,
  title,
  subtitle,
  leading,
  footer,
  children
}) {
  React.useEffect(() => {
    if (!open) return;
    const k = e => e.key === 'Escape' && onClose && onClose();
    window.addEventListener('keydown', k);
    return () => window.removeEventListener('keydown', k);
  }, [open, onClose]);
  if (!open) return null;
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
    className: "cp-sheet-scrim",
    onClick: onClose
  }), /*#__PURE__*/React.createElement("aside", {
    className: "cp-sheet",
    role: "dialog",
    "aria-modal": "true"
  }, /*#__PURE__*/React.createElement("header", {
    className: "cp-sheet__head"
  }, leading, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-h2"
  }, title), subtitle && /*#__PURE__*/React.createElement("div", {
    className: "cp-small cp-muted"
  }, subtitle)), /*#__PURE__*/React.createElement(__ds_scope.IconButton, {
    icon: "x",
    "aria-label": "Schlie\xDFen",
    onClick: onClose
  })), /*#__PURE__*/React.createElement("div", {
    className: "cp-sheet__body"
  }, children), footer && /*#__PURE__*/React.createElement("footer", {
    className: "cp-sheet__foot"
  }, footer)));
}
Object.assign(__ds_scope, { Sheet });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Sheet.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Spinner.jsx
try { (() => {
function Spinner({
  size = 20,
  color,
  style
}) {
  return /*#__PURE__*/React.createElement("span", {
    className: "cp-spinner",
    role: "progressbar",
    style: {
      width: size,
      height: size,
      ...(color ? {
        color
      } : null),
      ...style
    }
  });
}
Object.assign(__ds_scope, { Spinner });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Spinner.jsx", error: String((e && e.message) || e) }); }

// components/forms/Dropzone.jsx
try { (() => {
function Dropzone({
  accept = '.xlsx',
  title = 'Finanzguru-Export hierher ziehen',
  hint = 'oder klicken, um eine .xlsx-Datei auszuwählen',
  icon = 'file-spreadsheet',
  onFile,
  disabled
}) {
  const ref = React.useRef(null);
  const [over, setOver] = React.useState(false);
  const take = f => {
    if (f && onFile && !disabled) onFile(f);
  };
  return /*#__PURE__*/React.createElement("div", {
    className: 'cp-dropzone' + (over ? ' cp-dropzone--over' : ''),
    style: disabled ? {
      opacity: .5,
      pointerEvents: 'none'
    } : undefined,
    onClick: () => ref.current && ref.current.click(),
    onDragOver: e => {
      e.preventDefault();
      setOver(true);
    },
    onDragLeave: () => setOver(false),
    onDrop: e => {
      e.preventDefault();
      setOver(false);
      take(e.dataTransfer.files[0]);
    }
  }, /*#__PURE__*/React.createElement("input", {
    ref: ref,
    type: "file",
    accept: accept,
    onChange: e => {
      take(e.target.files[0]);
      e.target.value = '';
    }
  }), /*#__PURE__*/React.createElement("span", {
    className: "cp-empty__icon",
    style: {
      marginBottom: 6
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  })), /*#__PURE__*/React.createElement("div", {
    className: "cp-h3"
  }, title), /*#__PURE__*/React.createElement("div", {
    className: "cp-small cp-muted"
  }, hint));
}
Object.assign(__ds_scope, { Dropzone });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Dropzone.jsx", error: String((e && e.message) || e) }); }

// components/forms/FilterChip.jsx
try { (() => {
function FilterChip({
  active,
  icon,
  onClick,
  onRemove,
  children
}) {
  return /*#__PURE__*/React.createElement("button", {
    type: "button",
    className: 'cp-chip' + (active ? ' cp-chip--on' : ''),
    onClick: onClick
  }, icon && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  }), children, onRemove ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "x",
    className: "cp-chip__x",
    size: 14
  }) : !active && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "chevron-down",
    size: 14
  }));
}
Object.assign(__ds_scope, { FilterChip });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/FilterChip.jsx", error: String((e && e.message) || e) }); }

// components/forms/Input.jsx
try { (() => {
function Input({
  label,
  icon,
  placeholder,
  value,
  onChange,
  type = 'text',
  size = 'md',
  kbd,
  style,
  inputStyle
}) {
  const box = /*#__PURE__*/React.createElement("div", {
    className: 'cp-input' + (size === 'sm' ? ' cp-input--sm' : ''),
    style: label ? undefined : style
  }, icon && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  }), /*#__PURE__*/React.createElement("input", {
    type: type,
    placeholder: placeholder,
    value: value,
    onChange: e => onChange && onChange(e.target.value),
    style: inputStyle
  }), kbd && /*#__PURE__*/React.createElement("span", {
    className: "cp-input__kbd"
  }, kbd));
  return label ? /*#__PURE__*/React.createElement("label", {
    className: "cp-field",
    style: style
  }, /*#__PURE__*/React.createElement("span", {
    className: "cp-field__label"
  }, label), box) : box;
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Input.jsx", error: String((e && e.message) || e) }); }

// components/forms/Segmented.jsx
try { (() => {
function Segmented({
  options,
  value,
  onChange,
  style
}) {
  const opts = options.map(o => typeof o === 'string' ? {
    value: o,
    label: o
  } : o);
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-seg",
    role: "tablist",
    style: style
  }, opts.map(o => /*#__PURE__*/React.createElement("button", {
    key: o.value,
    type: "button",
    role: "tab",
    "aria-selected": o.value === value,
    className: 'cp-seg__opt' + (o.value === value ? ' cp-seg__opt--on' : ''),
    onClick: () => onChange && onChange(o.value)
  }, o.label)));
}
Object.assign(__ds_scope, { Segmented });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Segmented.jsx", error: String((e && e.message) || e) }); }

// components/forms/Select.jsx
try { (() => {
function Select({
  label,
  icon,
  options,
  value,
  onChange,
  size = 'md',
  style
}) {
  const opts = options.map(o => typeof o === 'string' ? {
    value: o,
    label: o
  } : o);
  const box = /*#__PURE__*/React.createElement("div", {
    className: 'cp-input cp-input--select' + (size === 'sm' ? ' cp-input--sm' : ''),
    style: label ? undefined : style
  }, icon && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon
  }), /*#__PURE__*/React.createElement("select", {
    value: value,
    onChange: e => onChange && onChange(e.target.value)
  }, opts.map(o => /*#__PURE__*/React.createElement("option", {
    key: o.value,
    value: o.value
  }, o.label))), /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "chevron-down"
  }));
  return label ? /*#__PURE__*/React.createElement("label", {
    className: "cp-field",
    style: style
  }, /*#__PURE__*/React.createElement("span", {
    className: "cp-field__label"
  }, label), box) : box;
}
Object.assign(__ds_scope, { Select });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Select.jsx", error: String((e && e.message) || e) }); }

// components/forms/Switch.jsx
try { (() => {
function Switch({
  checked,
  onChange,
  label,
  style
}) {
  const sw = /*#__PURE__*/React.createElement("button", {
    type: "button",
    role: "switch",
    "aria-checked": !!checked,
    className: 'cp-switch' + (checked ? ' cp-switch--on' : ''),
    onClick: () => onChange && onChange(!checked)
  });
  return label ? /*#__PURE__*/React.createElement("label", {
    className: "cp-switch-row",
    style: style
  }, sw, /*#__PURE__*/React.createElement("span", null, label)) : sw;
}
Object.assign(__ds_scope, { Switch });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Switch.jsx", error: String((e && e.message) || e) }); }

// components/navigation/Brand.jsx
try { (() => {
function Brand({
  tagline,
  style
}) {
  return /*#__PURE__*/React.createElement("span", {
    className: "cp-brand",
    style: style
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 2
    }
  }, /*#__PURE__*/React.createElement("span", {
    className: "cp-brand__name"
  }, "Cash", /*#__PURE__*/React.createElement("span", null, "Prism")), tagline && /*#__PURE__*/React.createElement("span", {
    className: "cp-brand__tag"
  }, tagline)));
}
Object.assign(__ds_scope, { Brand });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/Brand.jsx", error: String((e && e.message) || e) }); }

// components/navigation/PageHeader.jsx
try { (() => {
function PageHeader({
  eyebrow,
  title,
  lead,
  actions,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-pagehead",
    style: style
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-pagehead__text"
  }, eyebrow && /*#__PURE__*/React.createElement("div", {
    className: "cp-label",
    style: {
      marginBottom: 8
    }
  }, eyebrow), /*#__PURE__*/React.createElement("h1", {
    className: "cp-h1"
  }, title), lead && /*#__PURE__*/React.createElement("p", {
    className: "cp-pagehead__lead",
    style: {
      margin: '6px 0 0'
    }
  }, lead)), actions && /*#__PURE__*/React.createElement("div", {
    className: "cp-pagehead__actions"
  }, actions));
}
Object.assign(__ds_scope, { PageHeader });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/PageHeader.jsx", error: String((e && e.message) || e) }); }

// components/navigation/Sidebar.jsx
try { (() => {
function Sidebar({
  items,
  active,
  onNavigate,
  tagline = 'Deine Finanzen, lokal',
  footer,
  collapsedFooter,
  collapsed,
  onToggleCollapse,
  style
}) {
  const toggle = onToggleCollapse && /*#__PURE__*/React.createElement("button", {
    type: "button",
    className: "cp-iconbtn cp-iconbtn--sm cp-sidebar__toggle",
    onClick: onToggleCollapse,
    "aria-label": collapsed ? 'Navigation einblenden' : 'Navigation ausblenden',
    title: collapsed ? 'Navigation einblenden' : 'Navigation ausblenden',
    "aria-expanded": !collapsed
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: collapsed ? 'panel-left-open' : 'panel-left-close'
  }));
  return /*#__PURE__*/React.createElement("aside", {
    className: 'cp-sidebar' + (collapsed ? ' cp-sidebar--collapsed' : ''),
    style: style
  }, /*#__PURE__*/React.createElement("div", {
    className: "cp-sidebar__brand"
  }, collapsed ? /*#__PURE__*/React.createElement("span", {
    className: "cp-sidebar__mono",
    "aria-label": "CashPrism",
    title: "CashPrism"
  }, "C", /*#__PURE__*/React.createElement("span", null, "P")) : /*#__PURE__*/React.createElement(__ds_scope.Brand, {
    tagline: tagline
  }), toggle), /*#__PURE__*/React.createElement("nav", {
    className: "cp-nav"
  }, items.map((it, i) => it.section ? /*#__PURE__*/React.createElement("div", {
    key: 's' + i,
    className: "cp-sidebar__section cp-label"
  }, it.section) : /*#__PURE__*/React.createElement("button", {
    key: it.href,
    type: "button",
    className: 'cp-nav__item' + (active === it.href ? ' cp-nav__item--on' : ''),
    "aria-current": active === it.href ? 'page' : undefined,
    title: collapsed ? it.label : undefined,
    "aria-label": collapsed ? it.label : undefined,
    onClick: () => onNavigate && onNavigate(it.href)
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: it.icon
  }), /*#__PURE__*/React.createElement("span", null, it.label), it.count != null && /*#__PURE__*/React.createElement("span", {
    className: "cp-nav__count"
  }, it.count)))), (collapsed ? collapsedFooter : footer) && /*#__PURE__*/React.createElement("div", {
    className: "cp-sidebar__foot"
  }, collapsed ? collapsedFooter : footer));
}
Object.assign(__ds_scope, { Sidebar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/Sidebar.jsx", error: String((e && e.message) || e) }); }

// components/navigation/Tabs.jsx
try { (() => {
function Tabs({
  tabs,
  value,
  onChange,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: "cp-tabs",
    role: "tablist",
    style: style
  }, tabs.map(t => {
    const o = typeof t === 'string' ? {
      value: t,
      label: t
    } : t;
    return /*#__PURE__*/React.createElement("button", {
      key: o.value,
      type: "button",
      role: "tab",
      "aria-selected": o.value === value,
      className: 'cp-tab' + (o.value === value ? ' cp-tab--on' : ''),
      onClick: () => onChange && onChange(o.value)
    }, o.label, o.count != null && /*#__PURE__*/React.createElement("span", {
      className: "cp-tab__count"
    }, o.count));
  }));
}
Object.assign(__ds_scope, { Tabs });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/Tabs.jsx", error: String((e && e.message) || e) }); }

// components/navigation/Topbar.jsx
try { (() => {
function Topbar({
  section,
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("header", {
    className: "cp-topbar",
    style: style
  }, section && /*#__PURE__*/React.createElement("span", {
    className: "cp-topbar__section"
  }, section), children);
}
Object.assign(__ds_scope, { Topbar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/Topbar.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Accounts.jsx
try { (() => {
(() => {
  const {
    PageHeader,
    Card,
    StatCard,
    Amount,
    AreaChart,
    Tabs,
    TransactionList,
    CategoryIcon,
    Icon,
    Badge,
    Sparkline,
    BarChart,
    Legend
  } = window.CashPrismDesignSystem_24fa3e;
  function AccountCard({
    a,
    on,
    onClick
  }) {
    const K = CP.CUR;
    const hist = CP.range(K - 5, K + 1).map(k => CP.balanceAt(a.id, k));
    return /*#__PURE__*/React.createElement("button", {
      type: "button",
      onClick: onClick,
      style: {
        all: 'unset',
        boxSizing: 'border-box',
        cursor: 'pointer',
        display: 'block',
        width: '100%',
        padding: 16,
        borderRadius: 'var(--cp-radius-lg)',
        background: on ? 'var(--cp-primary)' : 'var(--cp-surface)',
        color: on ? 'var(--cp-on-primary)' : 'var(--cp-text)',
        border: '1px solid ' + (on ? 'var(--cp-primary)' : 'var(--cp-line)'),
        boxShadow: on ? '0 10px 24px -10px color-mix(in srgb, var(--cp-primary) 70%, transparent)' : 'var(--cp-shadow-card)',
        transition: 'all 200ms var(--cp-ease)'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        alignItems: 'center',
        gap: 12
      }
    }, /*#__PURE__*/React.createElement(CategoryIcon, {
      icon: a.icon,
      color: on ? '#fff' : a.color,
      size: 38,
      style: on ? {
        background: 'rgba(255,255,255,.18)',
        color: 'inherit'
      } : undefined
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        flex: 1,
        minWidth: 0
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        fontWeight: 750
      }
    }, a.name), /*#__PURE__*/React.createElement("div", {
      className: "cp-caption",
      style: {
        opacity: on ? .8 : 1,
        color: on ? 'inherit' : 'var(--cp-text-2)',
        fontWeight: 600
      }
    }, a.bank)), /*#__PURE__*/React.createElement(Sparkline, {
      values: hist,
      width: 56,
      height: 24,
      area: false,
      color: on ? 'currentColor' : a.color
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 14,
        fontSize: 20,
        fontWeight: 750,
        letterSpacing: '-0.02em',
        fontVariantNumeric: 'tabular-nums'
      }
    }, CP.eur(a.balance)));
  }
  function Accounts({
    onOpenBooking
  }) {
    const [sel, setSel] = React.useState('giro');
    const [tab, setTab] = React.useState('over');
    const a = CP.ACCOUNTS.find(x => x.id === sel);
    const K = CP.CUR;
    const months = CP.range(K - 11, K + 1);
    const list = CP.BOOKINGS.filter(b => b.account === sel);
    const last30 = list.filter(b => b.date > CP.TODAY.getTime() - 30 * 864e5);
    const total = CP.ACCOUNTS.reduce((s, x) => s + x.balance, 0);
    const flows = CP.range(K - 5, K).map(k => ({
      k,
      inn: CP.sumBy(list.filter(b => CP.mk(b.date) === k && b.cents > 0)),
      out: -CP.sumBy(list.filter(b => CP.mk(b.date) === k && b.cents < 0))
    }));
    return /*#__PURE__*/React.createElement("div", {
      className: "cp-page"
    }, /*#__PURE__*/React.createElement(PageHeader, {
      title: "Konten",
      lead: /*#__PURE__*/React.createElement(React.Fragment, null, "Zusammen hast du ", /*#__PURE__*/React.createElement("b", null, CP.eur(total)), " auf ", CP.ACCOUNTS.length, " Konten.")
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 280px), 1fr))',
        gap: 'var(--cp-grid-gap)',
        alignItems: 'start'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexDirection: 'column',
        gap: 12
      }
    }, /*#__PURE__*/React.createElement("div", {
      className: "cp-label",
      style: {
        padding: '0 4px'
      }
    }, "Deine Konten"), CP.ACCOUNTS.map(x => /*#__PURE__*/React.createElement(AccountCard, {
      key: x.id,
      a: x,
      on: x.id === sel,
      onClick: () => setSel(x.id)
    })), /*#__PURE__*/React.createElement("div", {
      className: "cp-caption cp-muted",
      style: {
        padding: '4px 4px',
        display: 'flex',
        gap: 8
      }
    }, /*#__PURE__*/React.createElement(Icon, {
      name: "info",
      size: 15
    }), "Konten kommen aus deinem Finanzguru-Export. Neue Konten erscheinen nach dem n\xE4chsten Import.")), /*#__PURE__*/React.createElement("div", {
      style: {
        gridColumn: 'span 2',
        minWidth: 0,
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        alignItems: 'center',
        gap: 16,
        padding: '20px 20px 0',
        flexWrap: 'wrap'
      }
    }, /*#__PURE__*/React.createElement(CategoryIcon, {
      icon: a.icon,
      color: a.color,
      size: 52,
      shape: "round"
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        flex: 1,
        minWidth: 200
      }
    }, /*#__PURE__*/React.createElement("h2", {
      className: "cp-h2"
    }, a.name), /*#__PURE__*/React.createElement("div", {
      className: "cp-small cp-muted",
      style: {
        fontWeight: 600
      }
    }, a.bank, " \xB7 ", /*#__PURE__*/React.createElement("span", {
      className: "cp-mono"
    }, a.iban))), /*#__PURE__*/React.createElement("div", {
      style: {
        textAlign: 'right'
      }
    }, /*#__PURE__*/React.createElement("div", {
      className: "cp-label"
    }, "Kontostand"), /*#__PURE__*/React.createElement(Amount, {
      cents: a.balance,
      sign: false,
      dimCents: true,
      size: 28
    }))), /*#__PURE__*/React.createElement("div", {
      style: {
        padding: '0 20px'
      }
    }, /*#__PURE__*/React.createElement(Tabs, {
      style: {
        marginTop: 16
      },
      value: tab,
      onChange: setTab,
      tabs: [{
        value: 'over',
        label: 'Überblick'
      }, {
        value: 'tx',
        label: 'Buchungen',
        count: list.length.toLocaleString('de-DE')
      }]
    })), tab === 'over' ? /*#__PURE__*/React.createElement("div", {
      style: {
        padding: 20
      }
    }, /*#__PURE__*/React.createElement("div", {
      className: "cp-h3",
      style: {
        marginBottom: 12
      }
    }, "Kontostand im Verlauf"), /*#__PURE__*/React.createElement(AreaChart, {
      height: 240,
      labels: months.map(CP.monthLabel),
      tooltipTitle: i => 'Ende ' + CP.monthLong(months[i]),
      format: CP.eur,
      formatAxis: CP.axis,
      series: [{
        name: 'Kontostand',
        color: a.color,
        values: months.map(k => CP.balanceAt(sel, k))
      }]
    })) : /*#__PURE__*/React.createElement("div", {
      style: {
        paddingBottom: 8
      }
    }, /*#__PURE__*/React.createElement(TransactionList, {
      groupByDay: true,
      items: list.slice(0, 30).map(CP.toTx),
      onSelect: t => onOpenBooking(t.raw)
    }))), tab === 'over' && /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 300px), 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      title: "Rein und raus",
      subtitle: "Letzte 6 Monate",
      action: /*#__PURE__*/React.createElement(Legend, {
        items: [{
          label: 'Rein',
          color: 'var(--cp-income)'
        }, {
          label: 'Raus',
          color: 'var(--cp-expense)'
        }]
      })
    }, /*#__PURE__*/React.createElement(BarChart, {
      height: 200,
      labels: flows.map(f => CP.monthLabel(f.k)),
      format: CP.eur,
      formatAxis: CP.axis,
      series: [{
        name: 'Rein',
        color: 'var(--cp-income)',
        values: flows.map(f => f.inn)
      }, {
        name: 'Raus',
        color: 'var(--cp-expense)',
        values: flows.map(f => f.out)
      }]
    })), /*#__PURE__*/React.createElement(Card, {
      title: "Letzte 30 Tage",
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        padding: '14px 20px 0',
        display: 'flex',
        gap: 24
      }
    }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
      className: "cp-caption cp-muted",
      style: {
        fontWeight: 600
      }
    }, "Eing\xE4nge"), /*#__PURE__*/React.createElement(Amount, {
      cents: CP.sumBy(last30.filter(b => b.cents > 0)),
      size: 18
    })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
      className: "cp-caption cp-muted",
      style: {
        fontWeight: 600
      }
    }, "Ausg\xE4nge"), /*#__PURE__*/React.createElement(Amount, {
      cents: CP.sumBy(last30.filter(b => b.cents < 0)),
      size: 18
    }))), /*#__PURE__*/React.createElement("div", {
      style: {
        paddingBottom: 8,
        paddingTop: 6
      }
    }, /*#__PURE__*/React.createElement(TransactionList, {
      items: last30.slice(0, 4).map(CP.toTx).map(t => ({
        ...t,
        meta: CP.date(t.raw.date)
      })),
      onSelect: t => onOpenBooking(t.raw)
    })))))));
  }
  Object.assign(window, {
    Accounts
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Accounts.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Analysis.jsx
try { (() => {
(() => {
  const {
    PageHeader,
    Card,
    BarChart,
    Legend,
    Segmented,
    DataTable,
    ProgressBar,
    Sparkline,
    Badge,
    CategoryIcon,
    Amount,
    AreaChart
  } = window.CashPrismDesignSystem_24fa3e;
  function Analysis({
    onNavigate
  }) {
    const [span, setSpan] = React.useState('6');
    const [focus, setFocus] = React.useState(null);
    const K = CP.CUR,
      months = CP.range(K - +span + 1, K);
    const cats = CP.catTotals(months[0], K);
    const total = cats.reduce((s, c) => s + c.total, 0);
    const shown = focus ? cats.filter(c => c.id === focus) : cats;
    const rows = cats.map(c => {
      const vals = months.map(k => CP.catMonth(c.id, k));
      const avg = c.total / months.length;
      const prevAvg = CP.range(months[0] - months.length, months[0] - 1).reduce((s, k) => s + CP.catMonth(c.id, k), 0) / months.length;
      return {
        ...c,
        id: c.id,
        vals,
        avg,
        share: c.total / total,
        change: prevAvg ? (avg - prevAvg) / prevAvg : 0
      };
    });
    const merchants = Object.values(CP.spend.filter(b => CP.mk(b.date) >= months[0]).reduce((m, b) => {
      m[b.who] = m[b.who] || {
        who: b.who,
        cat: b.cat,
        total: 0,
        n: 0
      };
      m[b.who].total -= b.cents;
      m[b.who].n++;
      return m;
    }, {})).sort((a, b) => b.total - a.total).slice(0, 8);
    const left = months.map(k => {
      const t = CP.monthTotals(k);
      return t.income - t.spend;
    });
    return /*#__PURE__*/React.createElement("div", {
      className: "cp-page"
    }, /*#__PURE__*/React.createElement(PageHeader, {
      title: "Analyse",
      lead: "Wohin dein Geld geht \u2013 und wie sich das mit der Zeit ver\xE4ndert.",
      actions: /*#__PURE__*/React.createElement(Segmented, {
        value: span,
        onChange: setSpan,
        options: [{
          value: '3',
          label: '3 Monate'
        }, {
          value: '6',
          label: '6 Monate'
        }, {
          value: '12',
          label: '12 Monate'
        }]
      })
    }), /*#__PURE__*/React.createElement(Card, {
      title: "Ausgaben nach Kategorie",
      subtitle: 'Ø ' + CP.eur0(total / months.length) + ' pro Monat · Klick auf eine Kategorie, um nur sie zu sehen'
    }, /*#__PURE__*/React.createElement(BarChart, {
      stacked: true,
      height: 280,
      labels: months.map(CP.monthLabel),
      format: CP.eur0,
      formatAxis: CP.axis,
      series: shown.map(c => ({
        name: c.name,
        color: c.color,
        values: months.map(k => CP.catMonth(c.id, k))
      }))
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexWrap: 'wrap',
        gap: 8,
        marginTop: 16
      }
    }, cats.map(c => /*#__PURE__*/React.createElement("button", {
      key: c.id,
      type: "button",
      className: 'cp-chip' + (focus === c.id ? ' cp-chip--on' : ''),
      onClick: () => setFocus(focus === c.id ? null : c.id),
      style: focus && focus !== c.id ? {
        opacity: .55
      } : undefined
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        width: 8,
        height: 8,
        borderRadius: 2,
        background: c.color
      }
    }), c.name)))), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 360px), 1fr))',
        gap: 'var(--cp-grid-gap)',
        marginTop: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      title: "Kategorien im Vergleich",
      subtitle: "Durchschnitt pro Monat, verglichen mit dem Zeitraum davor",
      padded: false,
      style: {
        gridColumn: 'span 2'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 12
      }
    }, /*#__PURE__*/React.createElement(DataTable, {
      rowKey: "id",
      onRowClick: r => setFocus(r.id),
      columns: [{
        key: 'name',
        title: 'Kategorie',
        render: r => /*#__PURE__*/React.createElement("div", {
          style: {
            display: 'flex',
            alignItems: 'center',
            gap: 12
          }
        }, /*#__PURE__*/React.createElement(CategoryIcon, {
          icon: r.icon,
          color: r.color,
          size: 34
        }), /*#__PURE__*/React.createElement("b", null, r.name))
      }, {
        key: 'share',
        title: 'Anteil',
        render: r => /*#__PURE__*/React.createElement("div", {
          style: {
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            minWidth: 140
          }
        }, /*#__PURE__*/React.createElement(ProgressBar, {
          value: r.share,
          color: r.color,
          size: "sm",
          style: {
            flex: 1
          }
        }), /*#__PURE__*/React.createElement("span", {
          className: "cp-small cp-muted cp-num",
          style: {
            width: 36,
            textAlign: 'right',
            fontWeight: 700
          }
        }, Math.round(r.share * 100), " %"))
      }, {
        key: 'trend',
        title: 'Verlauf',
        render: r => /*#__PURE__*/React.createElement(Sparkline, {
          values: r.vals,
          color: r.color,
          width: 90,
          height: 26
        })
      }, {
        key: 'change',
        title: 'Veränderung',
        render: r => /*#__PURE__*/React.createElement(Badge, {
          tone: Math.abs(r.change) < 0.05 ? 'neutral' : r.change > 0 ? 'danger' : 'success',
          icon: r.change > 0 ? 'trending-up' : 'trending-down'
        }, CP.pct(r.change))
      }, {
        key: 'avg',
        title: 'Ø pro Monat',
        align: 'right',
        render: r => /*#__PURE__*/React.createElement("span", {
          className: "cp-amount"
        }, CP.eur0(r.avg))
      }],
      rows: rows
    }))), /*#__PURE__*/React.createElement(Card, {
      title: "Hier geht am meisten hin",
      subtitle: 'Deine größten Empfänger, ' + months.length + ' Monate',
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        padding: '8px 0'
      }
    }, merchants.map((m, i) => {
      const c = CP.CATS[m.cat];
      return /*#__PURE__*/React.createElement("div", {
        key: m.who,
        className: "cp-tx",
        style: {
          cursor: 'default'
        }
      }, /*#__PURE__*/React.createElement("span", {
        className: "cp-small cp-faint cp-num",
        style: {
          width: 16,
          fontWeight: 700
        }
      }, i + 1), /*#__PURE__*/React.createElement(CategoryIcon, {
        icon: c.icon,
        color: c.color,
        size: 34
      }), /*#__PURE__*/React.createElement("div", {
        className: "cp-tx__main"
      }, /*#__PURE__*/React.createElement("div", {
        className: "cp-tx__title"
      }, m.who), /*#__PURE__*/React.createElement("div", {
        className: "cp-tx__meta"
      }, m.n, " Buchungen \xB7 ", c.name)), /*#__PURE__*/React.createElement("span", {
        className: "cp-amount"
      }, CP.eur0(m.total)));
    }))), /*#__PURE__*/React.createElement(Card, {
      title: "Monat f\xFCr Monat \xFCbrig",
      subtitle: "Einnahmen minus Ausgaben"
    }, /*#__PURE__*/React.createElement(BarChart, {
      height: 240,
      labels: months.map(CP.monthLabel),
      format: CP.eur0,
      formatAxis: CP.axis,
      series: [{
        name: 'Übrig',
        color: 'var(--cp-prism-teal)',
        values: left.map(v => Math.max(0, v))
      }]
    }), /*#__PURE__*/React.createElement("div", {
      className: "cp-small cp-muted",
      style: {
        marginTop: 12
      }
    }, "Im Schnitt bleiben dir ", /*#__PURE__*/React.createElement("b", {
      style: {
        color: 'var(--cp-text)'
      }
    }, CP.eur0(left.reduce((a, b) => a + b, 0) / left.length)), " pro Monat."))));
  }
  Object.assign(window, {
    Analysis
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Analysis.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/App.jsx
try { (() => {
(() => {
  const TITLES = {
    '/': 'Übersicht',
    '/bookings': 'Buchungen',
    '/accounts': 'Konten',
    '/analysis': 'Analyse',
    '/reports': 'Berichte',
    '/import': 'Import'
  };
  function App() {
    const [route, setRoute] = React.useState(() => window.CP_ROUTE || localStorage.getItem('cp2-route') || '/');
    const [dark, setDark] = React.useState(() => localStorage.getItem('cp2-theme') === 'dark');
    const [query, setQuery] = React.useState('');
    const [open, setOpen] = React.useState(null);
    React.useEffect(() => {
      document.documentElement.dataset.theme = dark ? 'dark' : 'light';
      localStorage.setItem('cp2-theme', dark ? 'dark' : 'light');
    }, [dark]);
    React.useEffect(() => {
      if (!window.CP_ROUTE) localStorage.setItem('cp2-route', route);
      document.title = 'CashPrism – ' + TITLES[route];
      const m = document.getElementById('cp-main');
      if (m) m.scrollTo({
        top: 0
      });
    }, [route]);
    const go = r => setRoute(r);
    let screen;
    if (route === '/') screen = /*#__PURE__*/React.createElement(Dashboard, {
      onNavigate: go,
      onOpenBooking: setOpen
    });else if (route === '/bookings') screen = /*#__PURE__*/React.createElement(Bookings, {
      query: query,
      setQuery: setQuery,
      onOpenBooking: setOpen
    });else if (route === '/accounts') screen = /*#__PURE__*/React.createElement(Accounts, {
      onOpenBooking: setOpen
    });else if (route === '/analysis') screen = /*#__PURE__*/React.createElement(Analysis, {
      onNavigate: go
    });else if (route === '/reports') screen = /*#__PURE__*/React.createElement(Reports, null);else screen = /*#__PURE__*/React.createElement(ImportScreen, null);
    return /*#__PURE__*/React.createElement(Shell, {
      route: route,
      onNavigate: go,
      dark: dark,
      onToggleTheme: () => setDark(d => !d),
      onSearch: q => {
        setQuery(q);
        setRoute('/bookings');
      }
    }, screen, /*#__PURE__*/React.createElement(BookingSheet, {
      booking: open,
      onClose: () => setOpen(null)
    }));
  }
  ReactDOM.createRoot(document.getElementById('root')).render(/*#__PURE__*/React.createElement(App, null));
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/App.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Bookings.jsx
try { (() => {
(() => {
  const {
    PageHeader,
    Card,
    DataTable,
    Pager,
    Input,
    Select,
    Segmented,
    FilterChip,
    Amount,
    CategoryIcon,
    Badge,
    Sheet,
    Button,
    EmptyState,
    Switch,
    TransactionList
  } = window.CashPrismDesignSystem_24fa3e;
  function BookingSheet({
    booking,
    onClose
  }) {
    if (!booking) return null;
    const c = CP.CATS[booking.cat],
      acc = CP.ACCOUNTS.find(a => a.id === booking.account);
    const same = CP.BOOKINGS.filter(b => b.who === booking.who && b.id !== booking.id).slice(0, 5).map(CP.toTx).map(t => ({
      ...t,
      meta: CP.date(t.raw.date),
      day: undefined
    }));
    return /*#__PURE__*/React.createElement(Sheet, {
      open: true,
      onClose: onClose,
      title: booking.who,
      subtitle: CP.dayLabel(booking.date),
      leading: /*#__PURE__*/React.createElement(CategoryIcon, {
        icon: c.icon,
        color: c.color,
        size: 44
      }),
      footer: /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(Button, {
        variant: "secondary",
        icon: "tag"
      }, "Kategorie \xE4ndern"), /*#__PURE__*/React.createElement(Button, {
        variant: "secondary",
        icon: "eye-off"
      }, "Ausblenden"))
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        textAlign: 'center',
        padding: '8px 0 24px'
      }
    }, /*#__PURE__*/React.createElement(Amount, {
      cents: booking.cents,
      size: 34,
      dimCents: true
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 10,
        display: 'flex',
        gap: 6,
        justifyContent: 'center',
        flexWrap: 'wrap'
      }
    }, /*#__PURE__*/React.createElement(Badge, null, booking.kind), booking.contract && /*#__PURE__*/React.createElement(Badge, {
      tone: "info",
      icon: "repeat"
    }, "Regelm\xE4\xDFig"), booking.transfer && /*#__PURE__*/React.createElement(Badge, {
      icon: "arrow-left-right"
    }, "Zwischen deinen Konten"))), /*#__PURE__*/React.createElement("dl", {
      className: "cp-kv"
    }, /*#__PURE__*/React.createElement("dt", null, "Datum"), /*#__PURE__*/React.createElement("dd", null, CP.date(booking.date)), /*#__PURE__*/React.createElement("dt", null, "Konto"), /*#__PURE__*/React.createElement("dd", null, acc.name), /*#__PURE__*/React.createElement("dt", null, "Kategorie"), /*#__PURE__*/React.createElement("dd", null, c.name), /*#__PURE__*/React.createElement("dt", null, booking.cents < 0 ? 'Empfänger' : 'Absender'), /*#__PURE__*/React.createElement("dd", null, booking.who), /*#__PURE__*/React.createElement("dt", null, "Verwendungszweck"), /*#__PURE__*/React.createElement("dd", {
      style: {
        fontWeight: 600
      }
    }, booking.ref)), same.length > 0 && /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
      className: "cp-label",
      style: {
        margin: '28px 0 6px'
      }
    }, "Fr\xFChere Buchungen bei ", booking.who), /*#__PURE__*/React.createElement("div", {
      style: {
        margin: '0 -20px'
      }
    }, /*#__PURE__*/React.createElement(TransactionList, {
      items: same
    }))));
  }
  const PERIODS = [{
    value: 'm',
    label: 'Dieser Monat'
  }, {
    value: '3',
    label: 'Letzte 3 Monate'
  }, {
    value: 'y',
    label: 'Dieses Jahr'
  }, {
    value: 'all',
    label: 'Alles'
  }];
  function Bookings({
    query,
    setQuery,
    onOpenBooking
  }) {
    const [acc, setAcc] = React.useState('all');
    const [period, setPeriod] = React.useState('3');
    const [dir, setDir] = React.useState('all');
    const [cat, setCat] = React.useState(null);
    const [hideTransfers, setHideTransfers] = React.useState(true);
    const [sort, setSort] = React.useState(null);
    const [page, setPage] = React.useState(0);
    const size = 25;
    React.useEffect(() => setPage(0), [acc, period, dir, cat, query, hideTransfers, sort]);
    const K = CP.mk(CP.TODAY.getTime());
    const rows = React.useMemo(() => {
      const q = (query || '').toLowerCase();
      let r = CP.BOOKINGS.filter(b => (acc === 'all' || b.account === acc) && (!hideTransfers || !b.transfer) && (dir === 'all' || (dir === 'in' ? b.cents > 0 : b.cents < 0)) && (!cat || b.cat === cat) && (period === 'all' || (period === 'm' ? CP.mk(b.date) === K : period === '3' ? CP.mk(b.date) >= K - 2 : new Date(b.date).getFullYear() === 2026)) && (!q || (b.who + ' ' + b.ref + ' ' + CP.CATS[b.cat].name).toLowerCase().includes(q)));
      if (sort) r = [...r].sort((a, b) => {
        const x = sort.key === 'cents' ? a.cents - b.cents : sort.key === 'date' ? a.date - b.date : a.who.localeCompare(b.who, 'de');
        return sort.desc ? -x : x;
      });
      return r;
    }, [acc, period, dir, cat, query, hideTransfers, sort]);
    const inSum = CP.sumBy(rows.filter(b => b.cents > 0)),
      outSum = CP.sumBy(rows.filter(b => b.cents < 0));
    const cols = [{
      key: 'who',
      title: 'Empfänger / Absender',
      sortable: true,
      render: b => {
        const c = CP.CATS[b.cat];
        return /*#__PURE__*/React.createElement("div", {
          style: {
            display: 'flex',
            alignItems: 'center',
            gap: 12,
            minWidth: 220
          }
        }, /*#__PURE__*/React.createElement(CategoryIcon, {
          icon: c.icon,
          color: c.color,
          size: 36
        }), /*#__PURE__*/React.createElement("div", {
          style: {
            minWidth: 0
          }
        }, /*#__PURE__*/React.createElement("div", {
          style: {
            fontWeight: 700
          }
        }, b.who), /*#__PURE__*/React.createElement("div", {
          className: "cp-caption cp-muted",
          style: {
            maxWidth: 320,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap'
          }
        }, b.ref)));
      }
    }, {
      key: 'cat',
      title: 'Kategorie',
      render: b => /*#__PURE__*/React.createElement(Badge, {
        style: {
          '--c': CP.CATS[b.cat].color
        }
      }, /*#__PURE__*/React.createElement("span", {
        style: {
          width: 7,
          height: 7,
          borderRadius: 9,
          background: CP.CATS[b.cat].color
        }
      }), CP.CATS[b.cat].name)
    }, {
      key: 'account',
      title: 'Konto',
      render: b => /*#__PURE__*/React.createElement("span", {
        className: "cp-muted",
        style: {
          fontWeight: 600
        }
      }, CP.ACCOUNTS.find(a => a.id === b.account).name)
    }, {
      key: 'date',
      title: 'Datum',
      sortable: true,
      render: b => /*#__PURE__*/React.createElement("span", {
        className: "cp-muted cp-num",
        style: {
          fontWeight: 600
        }
      }, CP.date(b.date))
    }, {
      key: 'cents',
      title: 'Betrag',
      align: 'right',
      sortable: true,
      render: b => /*#__PURE__*/React.createElement(Amount, {
        cents: b.cents
      })
    }];
    return /*#__PURE__*/React.createElement("div", {
      className: "cp-page"
    }, /*#__PURE__*/React.createElement(PageHeader, {
      title: "Buchungen",
      lead: 'Alles, was auf deinen Konten passiert ist – ' + CP.BOOKINGS.length.toLocaleString('de-DE') + ' Buchungen seit Oktober 2024.'
    }), /*#__PURE__*/React.createElement(Card, {
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexWrap: 'wrap',
        gap: 10,
        padding: 'var(--cp-card-padding)',
        alignItems: 'center'
      }
    }, /*#__PURE__*/React.createElement(Input, {
      icon: "search",
      placeholder: "Name, Verwendungszweck, Kategorie \u2026",
      value: query,
      onChange: setQuery,
      style: {
        flex: '1 1 260px'
      }
    }), /*#__PURE__*/React.createElement(Select, {
      icon: "landmark",
      value: acc,
      onChange: setAcc,
      options: [{
        value: 'all',
        label: 'Alle Konten'
      }, ...CP.ACCOUNTS.map(a => ({
        value: a.id,
        label: a.name
      }))],
      style: {
        flex: '0 1 200px'
      }
    }), /*#__PURE__*/React.createElement(Select, {
      icon: "calendar",
      value: period,
      onChange: setPeriod,
      options: PERIODS,
      style: {
        flex: '0 1 200px'
      }
    }), /*#__PURE__*/React.createElement(Segmented, {
      value: dir,
      onChange: setDir,
      options: [{
        value: 'all',
        label: 'Alle'
      }, {
        value: 'in',
        label: 'Einnahmen'
      }, {
        value: 'out',
        label: 'Ausgaben'
      }]
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        gap: 8,
        padding: '0 var(--cp-card-padding) 16px',
        overflowX: 'auto',
        alignItems: 'center',
        scrollbarWidth: 'none'
      }
    }, [...CP.SPEND, 'einkommen'].map(id => /*#__PURE__*/React.createElement(FilterChip, {
      key: id,
      active: cat === id,
      onClick: () => setCat(cat === id ? null : id),
      onRemove: cat === id ? () => setCat(null) : undefined
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        width: 8,
        height: 8,
        borderRadius: 9,
        background: CP.CATS[id].color
      }
    }), CP.CATS[id].name)), /*#__PURE__*/React.createElement("span", {
      style: {
        flex: 1
      }
    }), /*#__PURE__*/React.createElement(Switch, {
      checked: hideTransfers,
      onChange: setHideTransfers,
      label: /*#__PURE__*/React.createElement("span", {
        className: "cp-small",
        style: {
          whiteSpace: 'nowrap'
        }
      }, "Umbuchungen ausblenden")
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexWrap: 'wrap',
        gap: '6px 24px',
        padding: '12px var(--cp-card-padding)',
        borderTop: '1px solid var(--cp-line)',
        background: 'var(--cp-surface-2)'
      },
      className: "cp-small"
    }, /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("b", null, rows.length.toLocaleString('de-DE')), " ", /*#__PURE__*/React.createElement("span", {
      className: "cp-muted"
    }, "Buchungen")), /*#__PURE__*/React.createElement("span", {
      className: "cp-muted"
    }, "Einnahmen ", /*#__PURE__*/React.createElement(Amount, {
      cents: inSum
    })), /*#__PURE__*/React.createElement("span", {
      className: "cp-muted"
    }, "Ausgaben ", /*#__PURE__*/React.createElement(Amount, {
      cents: outSum
    }))), rows.length === 0 ? /*#__PURE__*/React.createElement(EmptyState, {
      icon: "search",
      title: "Nichts gefunden",
      text: "Keine Buchung passt zu deiner Suche. Probier einen anderen Begriff oder einen l\xE4ngeren Zeitraum.",
      actions: /*#__PURE__*/React.createElement(Button, {
        variant: "secondary",
        onClick: () => {
          setQuery('');
          setCat(null);
          setPeriod('all');
        }
      }, "Filter zur\xFCcksetzen")
    }) : /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
      style: {
        borderTop: '1px solid var(--cp-line)'
      }
    }, /*#__PURE__*/React.createElement(DataTable, {
      columns: cols,
      rows: rows.slice(page * size, page * size + size),
      sort: sort,
      onSortChange: setSort,
      onRowClick: onOpenBooking,
      groupBy: sort ? undefined : b => CP.dayLabel(b.date)
    })), /*#__PURE__*/React.createElement(Pager, {
      page: page,
      pageSize: size,
      total: rows.length,
      onPageChange: p => {
        setPage(p);
        document.getElementById('cp-main').scrollTo({
          top: 0
        });
      },
      noun: "Buchungen"
    }))));
  }
  Object.assign(window, {
    Bookings,
    BookingSheet
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Bookings.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Dashboard.jsx
try { (() => {
(() => {
  const {
    PageHeader,
    StatCard,
    Amount,
    Card,
    AreaChart,
    DonutChart,
    Legend,
    TransactionList,
    Insight,
    Button,
    Segmented,
    Sparkline
  } = window.CashPrismDesignSystem_24fa3e;
  function Dashboard({
    onNavigate,
    onOpenBooking
  }) {
    const [span, setSpan] = React.useState('12');
    const K = CP.CUR,
      months = CP.range(K - +span + 1, K);
    const cur = CP.monthTotals(K),
      prev = CP.monthTotals(K - 1);
    const net = CP.ACCOUNTS.reduce((s, a) => s + a.balance, 0);
    const netHist = CP.range(K - 11, K).map(k => CP.balanceAt(null, k));
    const left = cur.income - cur.spend,
      leftPrev = prev.income - prev.spend;
    const cats = CP.catTotals(K, K);
    const top = cats.slice(0, 5);
    const rest = cats.slice(5).reduce((s, c) => s + c.total, 0);
    const donut = [...top.map(c => ({
      label: c.name,
      value: c.total,
      color: c.color
    })), {
      label: 'Übrige',
      value: rest,
      color: 'var(--cp-prism-slate)'
    }];
    const [hi, setHi] = React.useState(null);
    const recent = CP.BOOKINGS.filter(b => !b.transfer).slice(0, 7).map(CP.toTx);
    // insights
    const avg = c => CP.range(K - 6, K - 1).reduce((s, k) => s + CP.catMonth(c, k), 0) / 6;
    const groc = CP.catMonth('lebensmittel', K),
      grocAvg = avg('lebensmittel');
    const priceUp = CP.contracts().filter(c => c.change < 0);
    const biggest = CP.spend.filter(b => CP.mk(b.date) === K).sort((a, b) => a.cents - b.cents)[0];
    return /*#__PURE__*/React.createElement("div", {
      className: "cp-page"
    }, /*#__PURE__*/React.createElement(PageHeader, {
      title: "Guten Abend",
      lead: /*#__PURE__*/React.createElement(React.Fragment, null, "So steht es um dein Geld im ", CP.monthLong(K), ". Du hast ", /*#__PURE__*/React.createElement("b", {
        style: {
          color: left >= 0 ? 'var(--cp-income)' : 'var(--cp-expense)'
        }
      }, CP.eur0(left)), " mehr eingenommen als ausgegeben."),
      actions: /*#__PURE__*/React.createElement(Segmented, {
        options: [{
          value: '6',
          label: '6 Monate'
        }, {
          value: '12',
          label: '12 Monate'
        }, {
          value: '24',
          label: '2 Jahre'
        }],
        value: span,
        onChange: setSpan
      })
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(230px, 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(StatCard, {
      icon: "wallet",
      color: "var(--cp-primary)",
      label: "Verm\xF6gen auf allen Konten",
      value: /*#__PURE__*/React.createElement(Amount, {
        cents: net,
        sign: false,
        dimCents: true
      }),
      delta: CP.pct((net - netHist[10]) / netHist[10]),
      deltaGood: net >= netHist[10],
      footer: /*#__PURE__*/React.createElement("div", {
        style: {
          marginTop: 10
        }
      }, /*#__PURE__*/React.createElement(Sparkline, {
        values: netHist,
        width: 150,
        height: 28
      }))
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "arrow-down-left",
      color: "var(--cp-income)",
      label: 'Einnahmen im ' + CP.MONTHS_LONG[K % 12],
      value: /*#__PURE__*/React.createElement(Amount, {
        cents: cur.income,
        sign: false,
        dimCents: true
      }),
      delta: CP.pct((cur.income - prev.income) / prev.income),
      deltaGood: cur.income >= prev.income
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "arrow-up-right",
      color: "var(--cp-expense)",
      label: 'Ausgaben im ' + CP.MONTHS_LONG[K % 12],
      value: /*#__PURE__*/React.createElement(Amount, {
        cents: cur.spend,
        sign: false,
        dimCents: true
      }),
      delta: CP.pct((cur.spend - prev.spend) / prev.spend),
      deltaGood: cur.spend <= prev.spend
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "piggy-bank",
      color: "var(--cp-prism-teal)",
      label: "\xDCbrig geblieben",
      value: /*#__PURE__*/React.createElement(Amount, {
        cents: left,
        sign: false,
        dimCents: true
      }),
      delta: Math.round(left / cur.income * 100) + ' %',
      deltaGood: left > 0,
      deltaLabel: "deiner Einnahmen gespart"
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 420px), 1fr))',
        gap: 'var(--cp-grid-gap)',
        marginTop: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      title: "Einnahmen und Ausgaben",
      subtitle: "Pro Monat, ohne Umbuchungen zwischen deinen Konten",
      style: {
        gridColumn: 'span 2'
      },
      action: /*#__PURE__*/React.createElement(Legend, {
        line: true,
        items: [{
          label: 'Einnahmen',
          color: 'var(--cp-income)'
        }, {
          label: 'Ausgaben',
          color: 'var(--cp-expense)'
        }]
      })
    }, /*#__PURE__*/React.createElement(AreaChart, {
      height: 280,
      labels: months.map(CP.monthLabel),
      tooltipTitle: i => CP.monthLong(months[i]),
      format: CP.eur,
      formatAxis: CP.axis,
      series: [{
        name: 'Einnahmen',
        color: 'var(--cp-income)',
        values: months.map(k => CP.monthTotals(k).income)
      }, {
        name: 'Ausgaben',
        color: 'var(--cp-expense)',
        values: months.map(k => CP.monthTotals(k).spend)
      }]
    })), /*#__PURE__*/React.createElement(Card, {
      title: "Wof\xFCr ging dein Geld?",
      subtitle: CP.monthLong(K),
      action: /*#__PURE__*/React.createElement(Button, {
        variant: "ghost",
        size: "sm",
        iconEnd: "arrow-right",
        onClick: () => onNavigate('/analysis')
      }, "Analyse")
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        gap: 24,
        justifyContent: 'center'
      }
    }, /*#__PURE__*/React.createElement(DonutChart, {
      data: donut,
      size: 190,
      centerLabel: "Ausgaben",
      centerValue: CP.eur0(cur.spend),
      format: CP.eur0,
      active: hi,
      onActiveChange: setHi
    }), /*#__PURE__*/React.createElement(Legend, {
      column: true,
      onHover: setHi,
      style: {
        flex: '1 1 180px',
        minWidth: 180
      },
      items: donut.map(d => ({
        label: d.label,
        color: d.color,
        value: CP.eur0(d.value)
      }))
    })))), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 380px), 1fr))',
        gap: 'var(--cp-grid-gap)',
        marginTop: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      title: "Letzte Buchungen",
      padded: false,
      action: /*#__PURE__*/React.createElement(Button, {
        variant: "ghost",
        size: "sm",
        iconEnd: "arrow-right",
        onClick: () => onNavigate('/bookings')
      }, "Alle ansehen")
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        paddingBottom: 8
      }
    }, /*#__PURE__*/React.createElement(TransactionList, {
      groupByDay: true,
      items: recent,
      onSelect: t => onOpenBooking(t.raw)
    }))), /*#__PURE__*/React.createElement(Card, {
      title: "Aufgefallen",
      subtitle: "Was sich bei dir ver\xE4ndert hat",
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        paddingTop: 10,
        paddingBottom: 6
      }
    }, /*#__PURE__*/React.createElement(Insight, {
      icon: "shopping-basket",
      color: "var(--cp-prism-green)",
      title: 'Lebensmittel: ' + CP.pct((groc - grocAvg) / grocAvg).replace('+', '') + ' mehr als sonst',
      sub: CP.eur0(groc) + ' im ' + CP.MONTHS_LONG[K % 12] + ', sonst etwa ' + CP.eur0(grocAvg) + ' im Monat',
      onClick: () => onNavigate('/analysis')
    }), priceUp.slice(0, 2).map(c => /*#__PURE__*/React.createElement(Insight, {
      key: c.id,
      icon: "repeat",
      color: "var(--cp-prism-cyan)",
      title: c.who + ' kostet jetzt ' + CP.eur(-c.change) + ' mehr',
      sub: 'Seit ' + CP.MONTHS_LONG[new Date(c.since).getMonth()] + ' ' + new Date(c.since).getFullYear() + ' · ' + CP.eur(-c.yearly) + ' im Jahr',
      onClick: () => onNavigate('/reports')
    })), biggest && /*#__PURE__*/React.createElement(Insight, {
      icon: "receipt-text",
      color: "var(--cp-prism-violet)",
      title: 'Größte Ausgabe: ' + biggest.who,
      sub: CP.eur(biggest.cents) + ' am ' + CP.date(biggest.date),
      onClick: () => onOpenBooking(biggest)
    }), /*#__PURE__*/React.createElement(Insight, {
      icon: "piggy-bank",
      color: "var(--cp-prism-teal)",
      title: 'Du sparst ' + Math.round(left / cur.income * 100) + ' % deiner Einnahmen',
      sub: 'Im Vormonat waren es ' + Math.round(leftPrev / prev.income * 100) + ' %',
      onClick: () => onNavigate('/reports')
    })))));
  }
  Object.assign(window, {
    Dashboard
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Dashboard.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Import.jsx
try { (() => {
(() => {
  const {
    PageHeader,
    Card,
    Dropzone,
    Alert,
    DataTable,
    Badge,
    CategoryIcon,
    Spinner,
    Icon
  } = window.CashPrismDesignSystem_24fa3e;
  const STEPS = [['smartphone', 'Finanzguru öffnen', 'In der App unter Profil → Datenexport.'], ['file-spreadsheet', '„Alle Buchungen“ exportieren', 'Als Excel-Datei (.xlsx) speichern oder dir selbst schicken.'], ['upload', 'Hier ablegen', 'CashPrism liest nur Neues ein. Doppeltes wird erkannt.']];
  function ImportScreen() {
    const [runs, setRuns] = React.useState(CP.IMPORTS);
    const [busy, setBusy] = React.useState(false);
    const [res, setRes] = React.useState(null);
    const onFile = f => {
      if (!/\.xlsx$/i.test(f.name)) {
        setRes({
          tone: 'danger',
          title: 'Das ist keine Finanzguru-Datei',
          body: 'Wir brauchen den Export „Alle Buchungen“ als .xlsx-Datei.'
        });
        return;
      }
      if (runs.some(r => r.file === f.name)) {
        setRes({
          tone: 'info',
          title: 'Diese Datei kennen wir schon',
          body: 'Sie wurde bereits eingelesen – es hat sich nichts geändert.'
        });
        return;
      }
      setBusy(true);
      setRes(null);
      setTimeout(() => {
        const ins = 30 + Math.floor(Math.random() * 40),
          upd = Math.floor(Math.random() * 6);
        setRuns(r => [{
          id: 'n' + Date.now(),
          at: Date.now(),
          file: f.name,
          exported: Date.now(),
          rows: r[0].rows + ins,
          inserted: ins,
          updated: upd
        }, ...r]);
        setRes({
          tone: 'success',
          title: 'Fertig! ' + ins + ' neue Buchungen sind da.',
          details: [(r => r)(runs[0].rows + ins).toLocaleString('de-DE') + ' Zeilen gelesen', upd + ' Buchungen wurden in Finanzguru geändert und hier aktualisiert', 'Alles andere war schon bekannt']
        });
        setBusy(false);
      }, 1600);
    };
    return /*#__PURE__*/React.createElement("div", {
      className: "cp-page"
    }, /*#__PURE__*/React.createElement(PageHeader, {
      title: "Import",
      lead: "Hol deine neuesten Buchungen aus Finanzguru. Jeder Import erg\xE4nzt, was schon da ist \u2013 nichts geht verloren."
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 320px), 1fr))',
        gap: 'var(--cp-grid-gap)',
        alignItems: 'start'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      style: {
        gridColumn: 'span 2'
      }
    }, busy ? /*#__PURE__*/React.createElement("div", {
      className: "cp-dropzone",
      style: {
        cursor: 'default'
      }
    }, /*#__PURE__*/React.createElement(Spinner, {
      size: 34
    }), /*#__PURE__*/React.createElement("div", {
      className: "cp-h3",
      style: {
        marginTop: 10
      }
    }, "Wird eingelesen \u2026"), /*#__PURE__*/React.createElement("div", {
      className: "cp-small cp-muted"
    }, "Das dauert ein paar Sekunden.")) : /*#__PURE__*/React.createElement(Dropzone, {
      onFile: onFile
    }), res && /*#__PURE__*/React.createElement(Alert, {
      tone: res.tone,
      title: res.title,
      details: res.details,
      style: {
        marginTop: 16
      }
    }, res.body)), /*#__PURE__*/React.createElement(Card, {
      title: "So geht\u2019s"
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexDirection: 'column',
        gap: 18
      }
    }, STEPS.map(([ic, t, s], i) => /*#__PURE__*/React.createElement("div", {
      key: i,
      style: {
        display: 'flex',
        gap: 12
      }
    }, /*#__PURE__*/React.createElement(CategoryIcon, {
      icon: ic,
      color: "var(--cp-primary)",
      size: 36,
      shape: "round"
    }), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
      style: {
        fontWeight: 750
      }
    }, i + 1, ". ", t), /*#__PURE__*/React.createElement("div", {
      className: "cp-small cp-muted"
    }, s))))))), /*#__PURE__*/React.createElement(Card, {
      title: "Bisherige Importe",
      subtitle: "Was wann eingelesen wurde",
      padded: false,
      style: {
        marginTop: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 12
      }
    }, /*#__PURE__*/React.createElement(DataTable, {
      columns: [{
        key: 'file',
        title: 'Datei',
        render: r => /*#__PURE__*/React.createElement("div", {
          style: {
            display: 'flex',
            alignItems: 'center',
            gap: 12
          }
        }, /*#__PURE__*/React.createElement(CategoryIcon, {
          icon: "file-spreadsheet",
          color: "var(--cp-prism-green)",
          size: 34
        }), /*#__PURE__*/React.createElement("div", {
          style: {
            minWidth: 0
          }
        }, /*#__PURE__*/React.createElement("b", {
          style: {
            display: 'block',
            maxWidth: 320,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap'
          },
          title: r.file
        }, r.file), /*#__PURE__*/React.createElement("div", {
          className: "cp-caption cp-muted"
        }, "Exportiert ", r.exported ? 'am ' + CP.date(r.exported) : '– Datum unbekannt')))
      }, {
        key: 'at',
        title: 'Eingelesen',
        render: r => /*#__PURE__*/React.createElement("span", {
          className: "cp-muted cp-num",
          style: {
            fontWeight: 600
          }
        }, new Date(r.at).toLocaleString('de-DE', {
          day: '2-digit',
          month: '2-digit',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        }))
      }, {
        key: 'inserted',
        title: 'Neu',
        align: 'right',
        render: r => /*#__PURE__*/React.createElement(Badge, {
          tone: "success"
        }, "+", r.inserted.toLocaleString('de-DE'))
      }, {
        key: 'updated',
        title: 'Aktualisiert',
        align: 'right',
        render: r => /*#__PURE__*/React.createElement("span", {
          className: "cp-num",
          style: {
            fontWeight: 700
          }
        }, r.updated)
      }, {
        key: 'rows',
        title: 'Zeilen gesamt',
        align: 'right',
        render: r => /*#__PURE__*/React.createElement("span", {
          className: "cp-num cp-muted",
          style: {
            fontWeight: 700
          }
        }, r.rows.toLocaleString('de-DE'))
      }],
      rows: runs
    }))));
  }
  Object.assign(window, {
    ImportScreen
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Import.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Reports.jsx
try { (() => {
(() => {
  const {
    PageHeader,
    Card,
    Tabs,
    Button,
    StatCard,
    Amount,
    DataTable,
    Badge,
    CategoryIcon,
    BarChart,
    Legend,
    DonutChart,
    Select,
    ProgressBar,
    Alert
  } = window.CashPrismDesignSystem_24fa3e;
  function MonthReport({
    k
  }) {
    const t = CP.monthTotals(k),
      p = CP.monthTotals(k - 1);
    const left = t.income - t.spend;
    const cats = CP.catTotals(k, k).filter(c => c.total > 0);
    const big = CP.spend.filter(b => CP.mk(b.date) === k).sort((a, b) => a.cents - b.cents).slice(0, 5);
    return /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, null, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        gap: 20,
        alignItems: 'center',
        flexWrap: 'wrap'
      }
    }, /*#__PURE__*/React.createElement(CategoryIcon, {
      icon: "calendar",
      color: "var(--cp-primary)",
      size: 52,
      shape: "round"
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        flex: 1,
        minWidth: 260
      }
    }, /*#__PURE__*/React.createElement("div", {
      className: "cp-label"
    }, "Monatsbericht"), /*#__PURE__*/React.createElement("h2", {
      className: "cp-h1",
      style: {
        marginTop: 4
      }
    }, CP.monthLong(k)), /*#__PURE__*/React.createElement("p", {
      className: "cp-muted",
      style: {
        margin: '8px 0 0',
        fontSize: 15,
        textWrap: 'pretty'
      }
    }, "Du hast ", /*#__PURE__*/React.createElement("b", {
      style: {
        color: 'var(--cp-text)'
      }
    }, CP.eur0(t.income)), " eingenommen und ", /*#__PURE__*/React.createElement("b", {
      style: {
        color: 'var(--cp-text)'
      }
    }, CP.eur0(t.spend)), " ausgegeben. Am meisten Geld ging f\xFCr ", /*#__PURE__*/React.createElement("b", {
      style: {
        color: 'var(--cp-text)'
      }
    }, cats[0].name), " weg. \xDCbrig geblieben ", left >= 0 ? 'sind' : 'ist ein Minus von', " ", /*#__PURE__*/React.createElement("b", {
      style: {
        color: left >= 0 ? 'var(--cp-income)' : 'var(--cp-expense)'
      }
    }, CP.eur0(Math.abs(left))), ".")))), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(StatCard, {
      icon: "arrow-down-left",
      color: "var(--cp-income)",
      label: "Einnahmen",
      value: CP.eur0(t.income),
      delta: CP.pct((t.income - p.income) / p.income),
      deltaGood: t.income >= p.income
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "arrow-up-right",
      color: "var(--cp-expense)",
      label: "Ausgaben",
      value: CP.eur0(t.spend),
      delta: CP.pct((t.spend - p.spend) / p.spend),
      deltaGood: t.spend <= p.spend
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "piggy-bank",
      color: "var(--cp-prism-teal)",
      label: "\xDCbrig",
      value: CP.eur0(left),
      delta: Math.round(left / t.income * 100) + ' %',
      deltaGood: left > 0,
      deltaLabel: "gespart"
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 340px), 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      title: "Ausgaben nach Kategorie"
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexDirection: 'column',
        gap: 14
      }
    }, cats.map(c => {
      const pv = CP.catMonth(c.id, k - 1);
      const ch = pv ? (c.total - pv) / pv : 0;
      return /*#__PURE__*/React.createElement("div", {
        key: c.id,
        style: {
          display: 'grid',
          gridTemplateColumns: '28px 1fr auto',
          gap: '4px 12px',
          alignItems: 'center'
        }
      }, /*#__PURE__*/React.createElement(CategoryIcon, {
        icon: c.icon,
        color: c.color,
        size: 28
      }), /*#__PURE__*/React.createElement("div", {
        style: {
          display: 'flex',
          justifyContent: 'space-between',
          gap: 8
        }
      }, /*#__PURE__*/React.createElement("b", {
        className: "cp-small"
      }, c.name), /*#__PURE__*/React.createElement("span", {
        className: "cp-caption",
        style: {
          fontWeight: 700,
          color: Math.abs(ch) < 0.05 ? 'var(--cp-text-3)' : ch > 0 ? 'var(--cp-expense)' : 'var(--cp-income)'
        }
      }, CP.pct(ch))), /*#__PURE__*/React.createElement("span", {
        className: "cp-amount cp-small"
      }, CP.eur0(c.total)), /*#__PURE__*/React.createElement("span", null), /*#__PURE__*/React.createElement(ProgressBar, {
        value: c.total / cats[0].total,
        color: c.color,
        size: "sm"
      }), /*#__PURE__*/React.createElement("span", null));
    }))), /*#__PURE__*/React.createElement(Card, {
      title: "Die f\xFCnf gr\xF6\xDFten Ausgaben",
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 10
      }
    }, /*#__PURE__*/React.createElement(DataTable, {
      compact: true,
      columns: [{
        key: 'who',
        title: 'Empfänger',
        render: b => /*#__PURE__*/React.createElement("b", null, b.who)
      }, {
        key: 'date',
        title: 'Datum',
        render: b => /*#__PURE__*/React.createElement("span", {
          className: "cp-muted cp-num"
        }, CP.date(b.date))
      }, {
        key: 'cents',
        title: 'Betrag',
        align: 'right',
        render: b => /*#__PURE__*/React.createElement(Amount, {
          cents: b.cents
        })
      }],
      rows: big
    })))));
  }
  function ContractsReport() {
    const list = CP.contracts();
    const monthly = -list.reduce((s, c) => s + c.monthly, 0);
    return /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(StatCard, {
      icon: "repeat",
      color: "var(--cp-prism-cyan)",
      label: "Feste Kosten pro Monat",
      value: CP.eur(monthly)
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "calendar-range",
      color: "var(--cp-prism-violet)",
      label: "Aufs Jahr gerechnet",
      value: CP.eur0(monthly * 12)
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "receipt-text",
      color: "var(--cp-prism-blue)",
      label: "Vertr\xE4ge & Abos",
      value: list.length
    })), list.filter(c => c.change < 0).map(c => /*#__PURE__*/React.createElement(Alert, {
      key: c.id,
      tone: "warning",
      icon: "trending-up",
      title: c.who + ' ist teurer geworden'
    }, "Seit ", CP.date(c.since), " zahlst du ", CP.eur(c.cents), " statt ", CP.eur(c.cents - c.change), " \u2013 das sind ", CP.eur(-c.change * 12 / c.every), " mehr im Jahr.")), /*#__PURE__*/React.createElement(Card, {
      title: "Alle regelm\xE4\xDFigen Zahlungen",
      subtitle: "Von CashPrism an wiederkehrenden Buchungen erkannt",
      padded: false
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 12
      }
    }, /*#__PURE__*/React.createElement(DataTable, {
      columns: [{
        key: 'who',
        title: 'Vertrag',
        render: c => /*#__PURE__*/React.createElement("div", {
          style: {
            display: 'flex',
            alignItems: 'center',
            gap: 12
          }
        }, /*#__PURE__*/React.createElement(CategoryIcon, {
          icon: c.cat.icon,
          color: c.cat.color,
          size: 34
        }), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("b", null, c.who), /*#__PURE__*/React.createElement("div", {
          className: "cp-caption cp-muted"
        }, c.cat.name, " \xB7 ", c.account)))
      }, {
        key: 'every',
        title: 'Rhythmus',
        render: c => /*#__PURE__*/React.createElement(Badge, null, c.every === 1 ? 'monatlich' : c.every === 3 ? 'vierteljährlich' : 'jährlich')
      }, {
        key: 'next',
        title: 'Nächste Zahlung',
        render: c => /*#__PURE__*/React.createElement("span", {
          className: "cp-muted cp-num",
          style: {
            fontWeight: 600
          }
        }, CP.date(c.next))
      }, {
        key: 'change',
        title: '',
        render: c => c.change < 0 ? /*#__PURE__*/React.createElement(Badge, {
          tone: "warning",
          icon: "trending-up"
        }, "teurer") : null
      }, {
        key: 'cents',
        title: 'Betrag',
        align: 'right',
        render: c => /*#__PURE__*/React.createElement(Amount, {
          cents: c.cents
        })
      }, {
        key: 'yearly',
        title: 'Pro Jahr',
        align: 'right',
        render: c => /*#__PURE__*/React.createElement("span", {
          className: "cp-amount cp-muted"
        }, CP.eur0(-c.yearly))
      }],
      rows: list
    }))));
  }
  function YearReport({
    y
  }) {
    const ks = CP.range(y * 12, y * 12 + 11).filter(k => k <= CP.CUR);
    const tt = ks.map(CP.monthTotals);
    const inc = tt.reduce((s, t) => s + t.income, 0),
      sp = tt.reduce((s, t) => s + t.spend, 0);
    const cats = CP.catTotals(ks[0], ks[ks.length - 1]).filter(c => c.total > 0);
    return /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(StatCard, {
      icon: "arrow-down-left",
      color: "var(--cp-income)",
      label: 'Einnahmen ' + y,
      value: CP.eur0(inc)
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "arrow-up-right",
      color: "var(--cp-expense)",
      label: 'Ausgaben ' + y,
      value: CP.eur0(sp)
    }), /*#__PURE__*/React.createElement(StatCard, {
      icon: "piggy-bank",
      color: "var(--cp-prism-teal)",
      label: "Gespart",
      value: CP.eur0(inc - sp),
      delta: Math.round((inc - sp) / inc * 100) + ' %',
      deltaGood: true,
      deltaLabel: "deiner Einnahmen"
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 360px), 1fr))',
        gap: 'var(--cp-grid-gap)'
      }
    }, /*#__PURE__*/React.createElement(Card, {
      title: "Das Jahr Monat f\xFCr Monat",
      style: {
        gridColumn: 'span 2'
      },
      action: /*#__PURE__*/React.createElement(Legend, {
        items: [{
          label: 'Einnahmen',
          color: 'var(--cp-income)'
        }, {
          label: 'Ausgaben',
          color: 'var(--cp-expense)'
        }]
      })
    }, /*#__PURE__*/React.createElement(BarChart, {
      height: 260,
      labels: ks.map(k => CP.MONTHS_LONG[k % 12].slice(0, 3)),
      format: CP.eur0,
      formatAxis: CP.axis,
      series: [{
        name: 'Einnahmen',
        color: 'var(--cp-income)',
        values: tt.map(t => t.income)
      }, {
        name: 'Ausgaben',
        color: 'var(--cp-expense)',
        values: tt.map(t => t.spend)
      }]
    })), /*#__PURE__*/React.createElement(Card, {
      title: "Wof\xFCr im ganzen Jahr"
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        justifyContent: 'center'
      }
    }, /*#__PURE__*/React.createElement(DonutChart, {
      size: 200,
      data: cats.map(c => ({
        label: c.name,
        value: c.total,
        color: c.color
      })),
      centerLabel: "Ausgaben",
      centerValue: CP.eur0(sp),
      format: CP.eur0
    })), /*#__PURE__*/React.createElement(Legend, {
      style: {
        marginTop: 16
      },
      column: true,
      items: cats.slice(0, 5).map(c => ({
        label: c.name,
        color: c.color,
        value: CP.eur0(c.total)
      }))
    }))));
  }
  function Reports() {
    const [tab, setTab] = React.useState('month');
    const [k, setK] = React.useState(String(CP.CUR));
    const [y, setY] = React.useState('2025');
    const monthOpts = CP.range(CP.CUR - 11, CP.CUR).reverse().map(m => ({
      value: String(m),
      label: CP.monthLong(m)
    }));
    return /*#__PURE__*/React.createElement("div", {
      className: "cp-page"
    }, /*#__PURE__*/React.createElement(PageHeader, {
      title: "Berichte",
      lead: "Fertige Zusammenfassungen zum Nachlesen, Ausdrucken oder Weitergeben.",
      actions: /*#__PURE__*/React.createElement(React.Fragment, null, tab === 'month' && /*#__PURE__*/React.createElement(Select, {
        icon: "calendar",
        value: k,
        onChange: setK,
        options: monthOpts
      }), tab === 'year' && /*#__PURE__*/React.createElement(Select, {
        icon: "calendar",
        value: y,
        onChange: setY,
        options: ['2026', '2025']
      }), /*#__PURE__*/React.createElement(Button, {
        variant: "secondary",
        icon: "printer",
        onClick: () => window.print()
      }, "Drucken"), /*#__PURE__*/React.createElement(Button, {
        variant: "secondary",
        icon: "download"
      }, "Als PDF"))
    }), /*#__PURE__*/React.createElement(Tabs, {
      style: {
        marginBottom: 'var(--cp-grid-gap)'
      },
      value: tab,
      onChange: setTab,
      tabs: [{
        value: 'month',
        label: 'Monatsbericht'
      }, {
        value: 'contracts',
        label: 'Verträge & Abos',
        count: CP.contracts().length
      }, {
        value: 'year',
        label: 'Jahresrückblick'
      }]
    }), tab === 'month' ? /*#__PURE__*/React.createElement(MonthReport, {
      k: +k
    }) : tab === 'contracts' ? /*#__PURE__*/React.createElement(ContractsReport, null) : /*#__PURE__*/React.createElement(YearReport, {
      y: +y
    }));
  }
  Object.assign(window, {
    Reports
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Reports.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Shell.jsx
try { (() => {
(() => {
  const {
    Sidebar,
    Topbar,
    Input,
    IconButton,
    Button,
    Icon
  } = window.CashPrismDesignSystem_24fa3e;
  const NAV = [{
    href: '/',
    label: 'Übersicht',
    icon: 'layout-dashboard'
  }, {
    href: '/bookings',
    label: 'Buchungen',
    icon: 'receipt-text'
  }, {
    href: '/accounts',
    label: 'Konten',
    icon: 'wallet'
  }, {
    section: 'Auswerten'
  }, {
    href: '/analysis',
    label: 'Analyse',
    icon: 'chart-pie'
  }, {
    href: '/reports',
    label: 'Berichte',
    icon: 'file-chart-column'
  }, {
    section: 'Daten'
  }, {
    href: '/import',
    label: 'Import',
    icon: 'upload'
  }];
  function PrivacyNote({
    lastImport
  }) {
    return /*#__PURE__*/React.createElement("div", {
      style: {
        padding: 14,
        borderRadius: 'var(--cp-radius-lg)',
        background: 'var(--cp-surface-2)',
        border: '1px solid var(--cp-line)'
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        alignItems: 'center',
        gap: 8,
        fontWeight: 750
      }
    }, /*#__PURE__*/React.createElement(Icon, {
      name: "lock",
      size: 16,
      color: "var(--cp-primary)"
    }), "Nur auf diesem Rechner"), /*#__PURE__*/React.createElement("div", {
      className: "cp-caption cp-muted",
      style: {
        marginTop: 4
      }
    }, "Kein Konto, keine Cloud. Deine Daten verlassen dein Heimnetz nicht."), /*#__PURE__*/React.createElement("div", {
      className: "cp-caption cp-faint",
      style: {
        marginTop: 10,
        fontWeight: 600
      }
    }, "Letzter Import: ", lastImport));
  }
  function useWide(px) {
    const q = '(min-width: ' + px + 'px)';
    const [w, setW] = React.useState(() => matchMedia(q).matches);
    React.useEffect(() => {
      const m = matchMedia(q);
      const f = () => setW(m.matches);
      m.addEventListener('change', f);
      return () => m.removeEventListener('change', f);
    }, []);
    return w;
  }
  function Shell({
    route,
    onNavigate,
    dark,
    onToggleTheme,
    onSearch,
    children
  }) {
    const wide = useWide(1040) || !!window.CP_FORCE_WIDE;
    const [open, setOpen] = React.useState(false);
    const [q, setQ] = React.useState('');
    const [collapsed, setCollapsed] = React.useState(() => localStorage.getItem('cp2-nav') === 'collapsed');
    React.useEffect(() => localStorage.setItem('cp2-nav', collapsed ? 'collapsed' : 'open'), [collapsed]);
    const nav = h => {
      onNavigate(h);
      setOpen(false);
    };
    const items = NAV.map(n => n.href === '/bookings' ? {
      ...n,
      count: CP.BOOKINGS.length.toLocaleString('de-DE')
    } : n);
    const rail = wide && collapsed;
    const sidebar = /*#__PURE__*/React.createElement(Sidebar, {
      items: items,
      active: route,
      onNavigate: nav,
      collapsed: rail,
      onToggleCollapse: wide ? () => setCollapsed(v => !v) : undefined,
      footer: /*#__PURE__*/React.createElement(PrivacyNote, {
        lastImport: CP.date(CP.IMPORTS[0].at)
      }),
      collapsedFooter: /*#__PURE__*/React.createElement("span", {
        title: "Nur auf diesem Rechner \u2013 kein Konto, keine Cloud",
        style: {
          display: 'flex',
          width: 40,
          height: 40,
          alignItems: 'center',
          justifyContent: 'center',
          borderRadius: 'var(--cp-radius-md)',
          background: 'var(--cp-surface-2)',
          border: '1px solid var(--cp-line)'
        }
      }, /*#__PURE__*/React.createElement(Icon, {
        name: "lock",
        size: 16,
        color: "var(--cp-primary)"
      }))
    });
    const current = NAV.find(n => n.href === route);
    return /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        height: '100vh',
        background: 'var(--cp-bg)'
      }
    }, wide ? /*#__PURE__*/React.createElement("div", {
      style: {
        flex: '0 0 auto',
        height: '100%'
      }
    }, sidebar) : open && /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
      className: "cp-sheet-scrim",
      onClick: () => setOpen(false)
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        position: 'fixed',
        inset: '0 auto 0 0',
        zIndex: 950
      }
    }, sidebar)), /*#__PURE__*/React.createElement("div", {
      style: {
        flex: 1,
        minWidth: 0,
        display: 'flex',
        flexDirection: 'column'
      }
    }, /*#__PURE__*/React.createElement(Topbar, {
      section: wide ? current && current.label : null
    }, !wide && /*#__PURE__*/React.createElement(IconButton, {
      icon: "menu",
      "aria-label": "Navigation \xF6ffnen",
      onClick: () => setOpen(true)
    }), /*#__PURE__*/React.createElement("span", {
      className: "cp-topbar__spacer"
    }), /*#__PURE__*/React.createElement("form", {
      onSubmit: e => {
        e.preventDefault();
        onSearch(q);
      },
      style: {
        width: 'min(340px, 40vw)'
      }
    }, /*#__PURE__*/React.createElement(Input, {
      icon: "search",
      placeholder: "Buchungen durchsuchen \u2026",
      value: q,
      onChange: setQ,
      kbd: "\u21B5"
    })), /*#__PURE__*/React.createElement(IconButton, {
      icon: dark ? 'sun' : 'moon',
      "aria-label": dark ? 'Helles Design' : 'Dunkles Design',
      onClick: onToggleTheme
    }), /*#__PURE__*/React.createElement(IconButton, {
      icon: "bell",
      dot: true,
      "aria-label": "Hinweise",
      onClick: () => onNavigate('/')
    }), /*#__PURE__*/React.createElement(Button, {
      icon: "upload",
      size: "sm",
      onClick: () => onNavigate('/import')
    }, "Import")), /*#__PURE__*/React.createElement("main", {
      style: {
        flex: 1,
        overflow: 'auto'
      },
      id: "cp-main"
    }, children)));
  }
  Object.assign(window, {
    Shell
  });
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Shell.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/data.js
try { (() => {
// Synthetic household data shaped like Finanzguru exports (Oct 2024 – 1 Oct 2026).
(function () {
  let seed = 4821;
  const r = () => (seed = (seed * 1664525 + 1013904223) % 4294967296) / 4294967296;
  const ri = (a, b) => Math.floor(a + r() * (b - a + 1));
  const pick = a => a[Math.floor(r() * a.length)];
  const TODAY = new Date(2026, 9, 2, 18, 40);
  const LAST = new Date(2026, 9, 1, 23, 59).getTime();
  const CATS = {
    wohnen: {
      id: 'wohnen',
      name: 'Wohnen',
      icon: 'house',
      color: 'var(--cp-prism-violet)'
    },
    lebensmittel: {
      id: 'lebensmittel',
      name: 'Lebensmittel',
      icon: 'shopping-basket',
      color: 'var(--cp-prism-green)'
    },
    mobilitaet: {
      id: 'mobilitaet',
      name: 'Mobilität',
      icon: 'car',
      color: 'var(--cp-prism-blue)'
    },
    freizeit: {
      id: 'freizeit',
      name: 'Freizeit & Essen',
      icon: 'popcorn',
      color: 'var(--cp-prism-rose)'
    },
    shopping: {
      id: 'shopping',
      name: 'Shopping',
      icon: 'shopping-bag',
      color: 'var(--cp-prism-orange)'
    },
    vertraege: {
      id: 'vertraege',
      name: 'Verträge & Abos',
      icon: 'repeat',
      color: 'var(--cp-prism-cyan)'
    },
    versicherungen: {
      id: 'versicherungen',
      name: 'Versicherungen',
      icon: 'shield',
      color: 'var(--cp-prism-teal)'
    },
    gesundheit: {
      id: 'gesundheit',
      name: 'Gesundheit',
      icon: 'heart-pulse',
      color: 'var(--cp-prism-amber)'
    },
    sonstiges: {
      id: 'sonstiges',
      name: 'Sonstiges',
      icon: 'circle-ellipsis',
      color: 'var(--cp-prism-slate)'
    },
    einkommen: {
      id: 'einkommen',
      name: 'Einkommen',
      icon: 'banknote',
      color: 'var(--cp-income)'
    },
    umbuchung: {
      id: 'umbuchung',
      name: 'Umbuchung',
      icon: 'arrow-left-right',
      color: 'var(--cp-prism-slate)'
    }
  };
  const SPEND = ['wohnen', 'lebensmittel', 'freizeit', 'shopping', 'mobilitaet', 'vertraege', 'versicherungen', 'gesundheit', 'sonstiges'];
  const ACCOUNTS = [{
    id: 'giro',
    name: 'Girokonto',
    bank: 'Sparkasse Leipzig',
    iban: 'DE12 •••• •••• •••• 4821',
    icon: 'landmark',
    color: 'var(--cp-prism-blue)',
    start: 214000
  }, {
    id: 'gemeinsam',
    name: 'Gemeinschaftskonto',
    bank: 'Sparkasse Leipzig',
    iban: 'DE47 •••• •••• •••• 1093',
    icon: 'house',
    color: 'var(--cp-prism-violet)',
    start: 180000
  }, {
    id: 'tagesgeld',
    name: 'Tagesgeld',
    bank: 'Trade Republic',
    iban: 'DE88 •••• •••• •••• 6630',
    icon: 'piggy-bank',
    color: 'var(--cp-prism-teal)',
    start: 850000
  }, {
    id: 'kredit',
    name: 'Kreditkarte',
    bank: 'DKB Visa',
    iban: '•••• •••• •••• 7712',
    icon: 'credit-card',
    color: 'var(--cp-prism-orange)',
    start: 0
  }];
  const B = [];
  let id = 0;
  const add = (date, account, cat, who, ref, cents, kind, extra) => {
    if (date.getTime() > LAST) return;
    B.push({
      id: 'b' + id++,
      date: date.getTime(),
      account,
      cat,
      who,
      ref,
      cents,
      kind,
      ...extra
    });
  };
  const transfer = (date, from, to, cents, ref) => {
    add(date, from, 'umbuchung', ACCOUNTS.find(a => a.id === to).name, ref, -cents, 'Überweisung', {
      transfer: true
    });
    add(date, to, 'umbuchung', ACCOUNTS.find(a => a.id === from).name, ref, cents, 'Gutschrift', {
      transfer: true
    });
  };
  const cardByMonth = {};
  for (let m = 0; m <= 24; m++) {
    const y = 2024 + Math.floor((9 + m) / 12),
      mo = (9 + m) % 12;
    const D = d => new Date(y, mo, d, ri(8, 20), ri(0, 59));
    const ym = y * 100 + mo + 1;
    const after = (yy, mm) => ym >= yy * 100 + mm;
    // fixed
    transfer(D(1), 'giro', 'gemeinsam', 140000, 'Haushalt ' + String(mo + 1).padStart(2, '0') + '/' + y);
    transfer(D(2), 'giro', 'tagesgeld', 30000, 'Sparplan Dauerauftrag');
    add(D(1), 'giro', 'versicherungen', 'HUK-COBURG', 'Kfz-Versicherung VS-Nr. 220-118734', -6140, 'Lastschrift', {
      contract: true
    });
    add(D(1), 'giro', 'mobilitaet', 'Deutsche Bahn', 'Deutschlandticket Abo', after(2025, 1) ? -5800 : -4900, 'Lastschrift', {
      contract: true
    });
    add(D(2), 'giro', 'gesundheit', 'FitX Studios', 'Mitgliedsbeitrag', -2499, 'Lastschrift', {
      contract: true
    });
    add(D(3), 'gemeinsam', 'wohnen', 'Hausverwaltung Berger', 'Miete Whg. 3.OG links inkl. NK', -115000, 'Dauerauftrag', {
      contract: true
    });
    add(D(5), 'giro', 'vertraege', 'congstar', 'Mobilfunk Rechnung ' + ri(10000000, 99999999), -2000, 'Lastschrift', {
      contract: true
    });
    add(D(7), 'kredit', 'vertraege', 'Spotify', 'Spotify Premium Duo', after(2026, 4) ? -1499 : -1299, 'Kartenzahlung', {
      contract: true
    });
    add(D(12), 'kredit', 'vertraege', 'Netflix', 'Netflix Standard', -1399, 'Kartenzahlung', {
      contract: true
    });
    add(D(15), 'gemeinsam', 'wohnen', 'Stadtwerke Leipzig', 'Abschlag Strom Vertragskonto 2004418833', after(2026, 1) ? -10200 : -9400, 'Lastschrift', {
      contract: true
    });
    add(D(20), 'gemeinsam', 'vertraege', 'Telekom', 'Festnetz + Internet MagentaZuhause M', -4495, 'Lastschrift', {
      contract: true
    });
    if (mo % 3 === 0) add(D(15), 'gemeinsam', 'wohnen', 'ARD ZDF Deutschlandradio', 'Rundfunkbeitrag Quartal', -5508, 'Lastschrift', {
      contract: true
    });
    if (mo === 1) add(D(10), 'giro', 'versicherungen', 'Allianz', 'Privathaftpflicht Jahresbeitrag', -6890, 'Lastschrift', {
      contract: true
    });
    if (mo % 3 === 2) add(D(28), 'tagesgeld', 'einkommen', 'Trade Republic', 'Zinsen Quartal', ri(5800, 8400), 'Gutschrift');
    add(new Date(y, mo, 28, 6, 10), 'giro', 'einkommen', 'Lindner Logistik GmbH', 'Lohn/Gehalt ' + String(mo + 1).padStart(2, '0') + '/' + y, after(2026, 3) ? 402000 : 385000, 'Gutschrift');
    if (mo === 10) add(D(28), 'giro', 'einkommen', 'Lindner Logistik GmbH', 'Weihnachtsgeld', 160000, 'Gutschrift');
    if (ym === 202507) add(D(18), 'giro', 'einkommen', 'Finanzamt Leipzig', 'Erstattung Einkommensteuer 2024', 74320, 'Gutschrift');
    // variable
    const groceryBoost = ym >= 202609 ? 1.22 : 1;
    for (let k = 0, n = ri(6, 9); k < n; k++) add(D(ri(1, 28)), 'gemeinsam', 'lebensmittel', 'REWE', 'REWE SAGT DANKE ' + ri(1000, 9999), -Math.round(ri(1800, 8600) * groceryBoost), 'Kartenzahlung');
    for (let k = 0, n = ri(2, 4); k < n; k++) add(D(ri(1, 28)), 'gemeinsam', 'lebensmittel', 'Lidl', 'LIDL DIENSTL. ' + ri(100, 999), -Math.round(ri(1200, 4800) * groceryBoost), 'Kartenzahlung');
    for (let k = 0, n = ri(3, 6); k < n; k++) add(D(ri(1, 28)), 'giro', 'lebensmittel', 'Bäckerei Wendl', 'Kartenzahlung', -ri(260, 980), 'Kartenzahlung');
    for (let k = 0, n = ri(1, 3); k < n; k++) add(D(ri(1, 28)), 'gemeinsam', 'shopping', 'dm-drogerie markt', 'dm Fil. ' + ri(1000, 2999), -ri(690, 3800), 'Kartenzahlung');
    for (let k = 0, n = ri(1, 3); k < n; k++) add(D(ri(1, 28)), 'kredit', 'shopping', 'Amazon', 'AMZN Mktp DE ' + ri(100, 999) + '-' + ri(1000000, 9999999), -ri(999, mo === 11 ? 18900 : 7900), 'Kartenzahlung');
    if (r() < 0.35) add(D(ri(1, 28)), 'kredit', 'shopping', 'Zalando', 'Zalando Bestellung ' + ri(10000000, 99999999), -ri(3995, 14900), 'Kartenzahlung');
    if (r() < 0.15 || mo === 2) add(D(ri(1, 28)), 'gemeinsam', 'shopping', 'IKEA', 'IKEA Leipzig', -ri(4900, 32900), 'Kartenzahlung');
    for (let k = 0, n = ri(2, mo === 7 ? 7 : 4); k < n; k++) {
      const p = pick([['Trattoria Da Enzo', 3800, 8900], ['Lieferando', 2200, 4600], ['Café Kowalski', 780, 2400], ['CineStar Kino', 2400, 3800]]);
      add(D(ri(1, 28)), pick(['giro', 'kredit']), 'freizeit', p[0], p[0] === 'Lieferando' ? 'Lieferando.de Bestellung' : 'Kartenzahlung', -ri(p[1], p[2]), 'Kartenzahlung');
    }
    if (mo === 7) add(D(4), 'gemeinsam', 'freizeit', 'Ferienhaus Ostseeblick', 'Anzahlung Ferienhaus Zingst', -89000, 'Überweisung');
    for (let k = 0, n = ri(1, 3); k < n; k++) add(D(ri(1, 28)), 'giro', 'mobilitaet', 'Aral', 'ARAL Tankstelle ' + ri(100, 999), -ri(4200, 7800), 'Kartenzahlung');
    if (r() < 0.4) add(D(ri(1, 28)), 'kredit', 'mobilitaet', 'Deutsche Bahn', 'DB Fernverkehr Ticket ' + ri(100000, 999999), -ri(2990, 11990), 'Kartenzahlung');
    if (r() < 0.6) add(D(ri(1, 28)), 'giro', 'gesundheit', 'Apotheke am Markt', 'Kartenzahlung', -ri(450, 3600), 'Kartenzahlung');
    if (r() < 0.7) add(D(ri(1, 28)), 'giro', 'sonstiges', 'Geldautomat', 'Bargeldauszahlung', -pick([5000, 10000, 10000, 15000]), 'Auszahlung');
  }
  // credit card settled on the 22nd of the next month from Girokonto
  B.filter(b => b.account === 'kredit' && !b.transfer).forEach(b => {
    const d = new Date(b.date);
    const k = d.getFullYear() * 12 + d.getMonth();
    cardByMonth[k] = (cardByMonth[k] || 0) - b.cents;
  });
  Object.entries(cardByMonth).forEach(([k, v]) => {
    const y = Math.floor((+k + 1) / 12),
      mo = (+k + 1) % 12;
    transfer(new Date(y, mo, 22, 9, 0), 'giro', 'kredit', v, 'Kreditkartenabrechnung');
  });
  B.sort((a, b) => b.date - a.date);
  // balances
  ACCOUNTS.forEach(a => {
    a.balance = a.start + B.filter(b => b.account === a.id).reduce((s, b) => s + b.cents, 0);
  });

  // ---------- helpers ----------
  const MONTHS = ['Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'];
  const MONTHS_LONG = ['Januar', 'Februar', 'März', 'April', 'Mai', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'];
  const DAYS = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];
  const mk = t => {
    const d = new Date(t);
    return d.getFullYear() * 12 + d.getMonth();
  };
  const CUR = 2026 * 12 + 8; // September 2026 = last full month
  const monthLabel = k => MONTHS[k % 12] + ' ' + String(Math.floor(k / 12)).slice(2);
  const monthLong = k => MONTHS_LONG[k % 12] + ' ' + Math.floor(k / 12);
  const eur = (c, o = {}) => {
    const d = o.decimals ?? 2;
    const s = o.sign ? c > 0 ? '+' : c < 0 ? '−' : '' : c < 0 ? '−' : '';
    return s + (Math.abs(c) / 100).toLocaleString('de-DE', {
      minimumFractionDigits: d,
      maximumFractionDigits: d
    }) + '\u00a0€';
  };
  const eur0 = c => eur(c, {
    decimals: 0
  });
  const axis = c => {
    const e = c / 100;
    return Math.abs(e) >= 10000 ? (e / 1000).toLocaleString('de-DE', {
      maximumFractionDigits: 0
    }) + ' Tsd.' : Math.round(e).toLocaleString('de-DE') + ' €';
  };
  const pct = v => (v > 0 ? '+' : v < 0 ? '−' : '') + Math.abs(Math.round(v * 100)).toLocaleString('de-DE') + ' %';
  const dayLabel = t => {
    const d = new Date(t);
    const t0 = new Date(TODAY.getFullYear(), TODAY.getMonth(), TODAY.getDate()).getTime();
    const dd = new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();
    const diff = Math.round((t0 - dd) / 864e5);
    if (diff === 0) return 'Heute';
    if (diff === 1) return 'Gestern';
    return DAYS[d.getDay()] + ', ' + d.getDate() + '. ' + MONTHS_LONG[d.getMonth()] + ' ' + d.getFullYear();
  };
  const date = t => new Date(t).toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });
  const spend = B.filter(b => !b.transfer && b.cents < 0);
  const income = B.filter(b => !b.transfer && b.cents > 0);
  const range = (from, to) => {
    const a = [];
    for (let k = from; k <= to; k++) a.push(k);
    return a;
  };
  const sumBy = (list, f) => list.reduce((s, b) => s + (f ? f(b) : b.cents), 0);
  const monthTotals = k => ({
    income: sumBy(income.filter(b => mk(b.date) === k)),
    spend: -sumBy(spend.filter(b => mk(b.date) === k))
  });
  const catTotals = (k0, k1) => SPEND.map(c => ({
    ...CATS[c],
    total: -sumBy(spend.filter(b => b.cat === c && mk(b.date) >= k0 && mk(b.date) <= k1))
  })).sort((a, b) => b.total - a.total);
  const catMonth = (c, k) => -sumBy(spend.filter(b => b.cat === c && mk(b.date) === k));
  const balanceAt = (accId, k) => {
    const acc = ACCOUNTS.filter(a => !accId || a.id === accId);
    return acc.reduce((s, a) => s + a.start + sumBy(B.filter(b => b.account === a.id && mk(b.date) <= k)), 0);
  };
  const toTx = b => ({
    id: b.id,
    title: b.who,
    meta: CATS[b.cat].name + ' · ' + ACCOUNTS.find(a => a.id === b.account).name,
    icon: CATS[b.cat].icon,
    color: CATS[b.cat].color,
    cents: b.cents,
    day: dayLabel(b.date),
    raw: b
  });
  const contracts = () => {
    const map = {};
    B.filter(b => b.contract).forEach(b => {
      const key = b.who + '|' + b.ref.replace(/\d{5,}/g, '');
      (map[key] = map[key] || []).push(b);
    });
    return Object.values(map).map(list => {
      list.sort((a, b) => b.date - a.date);
      const last = list[0],
        prev = list.find(b => b.cents !== last.cents);
      const gaps = list.length > 1 ? (list[0].date - list[list.length - 1].date) / (list.length - 1) / 864e5 : 30;
      const every = gaps > 300 ? 12 : gaps > 80 ? 3 : 1;
      const nd = new Date(last.date);
      nd.setMonth(nd.getMonth() + every);
      return {
        id: last.id,
        who: last.who,
        ref: last.ref,
        cat: CATS[last.cat],
        account: ACCOUNTS.find(a => a.id === last.account).name,
        cents: last.cents,
        every,
        monthly: Math.round(last.cents / every),
        yearly: Math.round(last.cents * 12 / every),
        next: nd.getTime(),
        change: prev && list.indexOf(prev) < 8 ? last.cents - prev.cents : 0,
        since: prev && list.indexOf(prev) < 8 ? list[list.indexOf(prev) - 1].date : null,
        count: list.length
      };
    }).sort((a, b) => a.monthly - b.monthly);
  };
  const IMPORTS = [{
    id: 'i4',
    at: new Date(2026, 9, 1, 19, 42).getTime(),
    file: 'Finanzguru_Alle_Buchungen_20261001.xlsx',
    exported: new Date(2026, 9, 1).getTime(),
    rows: B.length,
    inserted: 46,
    updated: 3
  }, {
    id: 'i3',
    at: new Date(2026, 8, 2, 8, 15).getTime(),
    file: 'Finanzguru_Alle_Buchungen_20260901.xlsx',
    exported: new Date(2026, 8, 1).getTime(),
    rows: B.length - 46,
    inserted: 51,
    updated: 0
  }, {
    id: 'i2',
    at: new Date(2026, 7, 3, 21, 3).getTime(),
    file: 'Export August (Kopie).xlsx',
    exported: new Date(2026, 7, 2).getTime(),
    rows: B.length - 97,
    inserted: 49,
    updated: 7
  }, {
    id: 'i1',
    at: new Date(2026, 6, 4, 20, 11).getTime(),
    file: 'alle_buchungen.xlsx',
    exported: null,
    rows: B.length - 146,
    inserted: B.length - 146,
    updated: 0
  }];
  window.CP = {
    TODAY,
    CATS,
    SPEND,
    ACCOUNTS,
    BOOKINGS: B,
    IMPORTS,
    CUR,
    mk,
    monthLabel,
    monthLong,
    MONTHS_LONG,
    eur,
    eur0,
    axis,
    pct,
    dayLabel,
    date,
    range,
    sumBy,
    spend,
    income,
    monthTotals,
    catTotals,
    catMonth,
    balanceAt,
    toTx,
    contracts
  };
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/data.js", error: String((e && e.message) || e) }); }

__ds_ns.AreaChart = __ds_scope.AreaChart;

__ds_ns.BarChart = __ds_scope.BarChart;

__ds_ns.DonutChart = __ds_scope.DonutChart;

__ds_ns.Sparkline = __ds_scope.Sparkline;

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.CategoryIcon = __ds_scope.CategoryIcon;

__ds_ns.Icon = __ds_scope.Icon;

__ds_ns.IconButton = __ds_scope.IconButton;

__ds_ns.Amount = __ds_scope.Amount;

__ds_ns.DataTable = __ds_scope.DataTable;

__ds_ns.Legend = __ds_scope.Legend;

__ds_ns.Pager = __ds_scope.Pager;

__ds_ns.ProgressBar = __ds_scope.ProgressBar;

__ds_ns.StatCard = __ds_scope.StatCard;

__ds_ns.TransactionList = __ds_scope.TransactionList;

__ds_ns.Alert = __ds_scope.Alert;

__ds_ns.EmptyState = __ds_scope.EmptyState;

__ds_ns.Insight = __ds_scope.Insight;

__ds_ns.Sheet = __ds_scope.Sheet;

__ds_ns.Spinner = __ds_scope.Spinner;

__ds_ns.Dropzone = __ds_scope.Dropzone;

__ds_ns.FilterChip = __ds_scope.FilterChip;

__ds_ns.Input = __ds_scope.Input;

__ds_ns.Segmented = __ds_scope.Segmented;

__ds_ns.Select = __ds_scope.Select;

__ds_ns.Switch = __ds_scope.Switch;

__ds_ns.Brand = __ds_scope.Brand;

__ds_ns.PageHeader = __ds_scope.PageHeader;

__ds_ns.Sidebar = __ds_scope.Sidebar;

__ds_ns.Tabs = __ds_scope.Tabs;

__ds_ns.Topbar = __ds_scope.Topbar;

})();
