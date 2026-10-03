export interface TopbarProps {
/** Small uppercase section label at the left. */
  section?: React.ReactNode;
  /** Search, spacer, actions — lay out freely; use <span className="cp-topbar__spacer"/> to push right. */
  children?: React.ReactNode;
  style?: React.CSSProperties;
}
export declare function Topbar(props: TopbarProps): JSX.Element;
