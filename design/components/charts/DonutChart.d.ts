export interface DonutChartProps {
data: { label: string; value: number; color: string }[];
  /** px; default 200. */
  size?: number;
  thickness?: number;
  /** Shown in the middle when nothing is hovered. */
  centerLabel?: React.ReactNode;
  centerValue?: React.ReactNode;
  format?: (v: number) => string;
  /** Controlled hover index (sync with a Legend). */
  active?: number | null;
  onActiveChange?: (index: number | null) => void;
  style?: React.CSSProperties;
}
export declare function DonutChart(props: DonutChartProps): JSX.Element;
