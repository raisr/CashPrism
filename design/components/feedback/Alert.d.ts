export interface AlertProps {
tone?: 'info' | 'success' | 'warning' | 'danger';
  title?: React.ReactNode;
  /** Bullet list under the text. */
  details?: React.ReactNode[];
  icon?: string;
  /** Right-aligned button. */
  action?: React.ReactNode;
  children?: React.ReactNode;
  style?: React.CSSProperties;
}
export declare function Alert(props: AlertProps): JSX.Element;
