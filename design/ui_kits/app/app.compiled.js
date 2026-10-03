// Precompiled from the .jsx files in this folder so the prototype opens from file:// without a server.

// ---- Shell.jsx ----
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

// ---- Dashboard.jsx ----
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

// ---- Bookings.jsx ----
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

// ---- Accounts.jsx ----
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

// ---- Analysis.jsx ----
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

// ---- Reports.jsx ----
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

// ---- Import.jsx ----
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

// ---- App.jsx ----
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
