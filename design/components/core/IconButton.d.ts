export interface IconButtonProps {
icon: string;
  variant?: 'ghost' | 'outlined';
  size?: 'sm' | 'md';
  /** Small red notification dot. */
  dot?: boolean;
  disabled?: boolean;
  onClick?: (e: React.MouseEvent) => void;
  /** German accessible name — required. */
  'aria-label': string;
  className?: string;
  style?: React.CSSProperties;
}
export declare function IconButton(props: IconButtonProps): JSX.Element;
