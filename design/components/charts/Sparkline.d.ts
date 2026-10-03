export interface SparklineProps {
values: number[];
  color?: string;
  width?: number;
  height?: number;
  area?: boolean;
  style?: React.CSSProperties;
}
export declare function Sparkline(props: SparklineProps): JSX.Element;
