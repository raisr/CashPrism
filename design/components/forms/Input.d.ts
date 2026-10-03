export interface InputProps {
/** Small uppercase label above. */
  label?: string;
  icon?: string;
  placeholder?: string;
  value?: string;
  onChange?: (value: string) => void;
  type?: string;
  size?: 'sm' | 'md';
  /** Keyboard hint at the right, e.g. "/". */
  kbd?: string;
  style?: React.CSSProperties;
  inputStyle?: React.CSSProperties;
}
export declare function Input(props: InputProps): JSX.Element;
