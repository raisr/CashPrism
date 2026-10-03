export interface TabsProps {
tabs: (string | { value: string; label: string; count?: number | string })[];
  value: string;
  onChange?: (value: string) => void;
  style?: React.CSSProperties;
}
export declare function Tabs(props: TabsProps): JSX.Element;
