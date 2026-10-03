export interface PageHeaderProps {
eyebrow?: React.ReactNode;
  title: React.ReactNode;
  /** One friendly sentence about what the page shows. */
  lead?: React.ReactNode;
  actions?: React.ReactNode;
  style?: React.CSSProperties;
}
export declare function PageHeader(props: PageHeaderProps): JSX.Element;
