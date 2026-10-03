export interface DropzoneProps {
accept?: string;
  title?: string;
  hint?: string;
  icon?: string;
  onFile?: (file: File) => void;
  disabled?: boolean;
}
export declare function Dropzone(props: DropzoneProps): JSX.Element;
