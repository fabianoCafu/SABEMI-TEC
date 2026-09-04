import { Component, Input } from '@angular/core';
import { TableColumn } from './table-column.interface';

@Component({
    selector: 'app-table',
    standalone: true,
    templateUrl: './table.html',
    styleUrl: './table.css'
})

export class TableComponent {
    @Input() columns: TableColumn[] = [];
    @Input() data: any[] = [];
}
