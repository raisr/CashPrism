import React from 'react';
import { Icon } from '../core/Icon.jsx';
import { Brand } from './Brand.jsx';
export function Sidebar({ items, active, onNavigate, tagline = 'Deine Finanzen, lokal', footer, collapsedFooter, collapsed, onToggleCollapse, style }) {
  const toggle = onToggleCollapse && <button type="button" className="cp-iconbtn cp-iconbtn--sm cp-sidebar__toggle" onClick={onToggleCollapse} aria-label={collapsed ? 'Navigation einblenden' : 'Navigation ausblenden'} title={collapsed ? 'Navigation einblenden' : 'Navigation ausblenden'} aria-expanded={!collapsed}><Icon name={collapsed ? 'panel-left-open' : 'panel-left-close'} /></button>;
  return <aside className={'cp-sidebar' + (collapsed ? ' cp-sidebar--collapsed' : '')} style={style}>
    <div className="cp-sidebar__brand">{collapsed ? <span className="cp-sidebar__mono" aria-label="CashPrism" title="CashPrism">C<span>P</span></span> : <Brand tagline={tagline} />}{toggle}</div>
    <nav className="cp-nav">{items.map((it, i) => it.section
      ? <div key={'s' + i} className="cp-sidebar__section cp-label">{it.section}</div>
      : <button key={it.href} type="button" className={'cp-nav__item' + (active === it.href ? ' cp-nav__item--on' : '')} aria-current={active === it.href ? 'page' : undefined} title={collapsed ? it.label : undefined} aria-label={collapsed ? it.label : undefined} onClick={() => onNavigate && onNavigate(it.href)}>
          <Icon name={it.icon} /><span>{it.label}</span>{it.count != null && <span className="cp-nav__count">{it.count}</span>}
        </button>)}</nav>
    {(collapsed ? collapsedFooter : footer) && <div className="cp-sidebar__foot">{collapsed ? collapsedFooter : footer}</div>}
  </aside>;
}
