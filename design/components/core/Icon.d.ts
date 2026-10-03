export interface IconProps {
/** Lucide icon name, kebab-case: "wallet", "chart-pie", "receipt-text". */
  name: string;
  /** px; default 20. */
  size?: number;
  color?: string;
  className?: string;
  style?: React.CSSProperties;
  title?: string;
}
export declare function Icon(props: IconProps): JSX.Element;
