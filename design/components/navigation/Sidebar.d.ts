export interface SidebarProps {
/** Destinations; an entry with only "section" renders a small group label. */
  items: { href?: string; label?: string; icon?: string; count?: number | string; section?: string }[];
  active?: string;
  onNavigate?: (href: string) => void;
  tagline?: string;
  /** Pinned to the bottom (privacy note, import status …). */
  footer?: React.ReactNode;
  /** Slim 72px icon rail: labels, counts and section names hide; labels become tooltips. */
  collapsed?: boolean;
  /** Shows the quiet panel toggle (top right of the sidebar, or top of the rail when collapsed). */
  onToggleCollapse?: () => void;
  /** Footer content for the collapsed rail (e.g. a single lock icon). */
  collapsedFooter?: React.ReactNode;
  style?: React.CSSProperties;
}
export declare function Sidebar(props: SidebarProps): JSX.Element;
