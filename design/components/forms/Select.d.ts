export interface SelectProps {
label?: string;
  icon?: string;
  options: (string | { value: string; label: string })[];
  value?: string;
  onChange?: (value: string) => void;
  size?: 'sm' | 'md';
  style?: React.CSSProperties;
}
export declare function Select(props: SelectProps): JSX.Element;
