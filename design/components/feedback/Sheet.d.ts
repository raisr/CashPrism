export interface SheetProps {
open: boolean;
  onClose?: () => void;
  title: React.ReactNode;
  subtitle?: React.ReactNode;
  /** Icon/tile left of the title. */
  leading?: React.ReactNode;
  footer?: React.ReactNode;
  children?: React.ReactNode;
}
export declare function Sheet(props: SheetProps): JSX.Element | null;
