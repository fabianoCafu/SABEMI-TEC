
export interface TableColumn<T = any> {
    field: keyof T | string;
    header?: string;
    width?: string;
    align?: 'left' | 'center' | 'right';
    hidden?: boolean;
    cellClass?: (row: T) => string;
}