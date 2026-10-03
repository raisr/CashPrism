export interface CategoryIconProps {
/** Lucide icon name. */
  icon: string;
  /** A --cp-prism-* colour (or any CSS colour). */
  color?: string;
  /** px; default 36. */
  size?: number;
  shape?: 'square' | 'round';
  /** Filled colour with white glyph instead of a soft tint. */
  solid?: boolean;
  style?: React.CSSProperties;
}
export declare function CategoryIcon(props: CategoryIconProps): JSX.Element;
