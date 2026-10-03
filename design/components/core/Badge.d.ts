export interface BadgeProps {
tone?: 'neutral' | 'success' | 'danger' | 'warning' | 'info';
  /** Leading status dot. */
  dot?: boolean;
  icon?: string;
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
}
export declare function Badge(props: BadgeProps): JSX.Element;
