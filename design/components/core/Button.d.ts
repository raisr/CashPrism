export interface ButtonProps {
/** primary = the one main action per view; secondary = outlined; soft = tinted; ghost = toolbar; danger = destructive. */
  variant?: 'primary' | 'secondary' | 'soft' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  /** Lucide icon name before the label. */
  icon?: string;
  iconEnd?: string;
  block?: boolean;
  href?: string;
  disabled?: boolean;
  type?: 'button' | 'submit';
  onClick?: (e: React.MouseEvent) => void;
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
}
export declare function Button(props: ButtonProps): JSX.Element;
