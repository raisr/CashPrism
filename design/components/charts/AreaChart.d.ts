export interface AreaChartProps {
labels: string[];
  series: { name: string; color: string; values: number[]; area?: boolean; dashed?: boolean }[];
  height?: number;
  /** Tooltip value formatter. */
  format?: (v: number) => string;
  /** Y-axis formatter (shorter), defaults to format. */
  formatAxis?: (v: number) => string;
  tooltipTitle?: (index: number) => string;
  /** Gradient fill under lines. Default true. */
  area?: boolean;
  style?: React.CSSProperties;
}
export declare function AreaChart(props: AreaChartProps): JSX.Element;
