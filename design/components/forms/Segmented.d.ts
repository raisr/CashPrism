export interface SegmentedProps {
options: (string | { value: string; label: string })[];
  value: string;
  onChange?: (value: string) => void;
  style?: React.CSSProperties;
}
export declare function Segmented(props: SegmentedProps): JSX.Element;
