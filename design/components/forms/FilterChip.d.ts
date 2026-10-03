export interface FilterChipProps {
active?: boolean;
  icon?: string;
  onClick?: () => void;
  /** Shows an × instead of the chevron. */
  onRemove?: () => void;
  children?: React.ReactNode;
}
export declare function FilterChip(props: FilterChipProps): JSX.Element;
