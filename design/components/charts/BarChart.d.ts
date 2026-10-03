export interface BarChartProps {
labels: string[];
  series: { name: string; color: string; values: number[] }[];
  /** Stack series into one bar per label. */
  stacked?: boolean;
  height?: number;
  format?: (v: number) => string;
  formatAxis?: (v: number) => string;
  /** Index to emphasise (others dim). */
  highlight?: number;
  onSelect?: (index: number) => void;
  style?: React.CSSProperties;
}
export declare function BarChart(props: BarChartProps): JSX.Element;
