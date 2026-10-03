export interface CardProps {
title?: React.ReactNode;
  subtitle?: React.ReactNode;
  /** Right side of the header: a Segmented, a ghost Button … */
  action?: React.ReactNode;
  /** Wrap children in 20px padding. false for edge-to-edge tables/lists. */
  padded?: boolean;
  /** No shadow (dense nested use). */
  flat?: boolean;
  className?: string;
  style?: React.CSSProperties;
  bodyStyle?: React.CSSProperties;
  children?: React.ReactNode;
}
export declare function Card(props: CardProps): JSX.Element;
