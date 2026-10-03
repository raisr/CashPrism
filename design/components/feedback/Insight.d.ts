export interface InsightProps {
icon?: string;
  color?: string;
  /** One plain-language observation. */
  title: React.ReactNode;
  sub?: React.ReactNode;
  onClick?: () => void;
}
export declare function Insight(props: InsightProps): JSX.Element;
