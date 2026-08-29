import { Component, OnInit, inject, signal} from '@angular/core';
import { TableComponent } from '../../shared/components/table/table';
import { TableColumn } from '../../shared/components/table/table-column.interface';
import { PagamentoServices } from '../../core/services/pagamento.services';
import { Pagamento } from '../../models/pagamento';
import { PagamentoSignalrService } from '../../core/services/pagamento-signalr.service';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [TableComponent, FormsModule],
    templateUrl: './home.html',
    styleUrl: './home.css',
})

export class HomeComponent implements OnInit {

    pagamentos = signal<Pagamento[]>([]);
    pagamentosSemFiltro = signal<Pagamento[]>([]);
    contratoFiltro = '';
    statusFiltro = '';
    toastVisivel = signal(false);
    mensagemToast = signal('');
    carregando = true;

    private pagamentosConhecidos = new Map<string, string>();
    private pagamentoService = inject(PagamentoServices);
    private pagamentoSignalrService = inject(PagamentoSignalrService);
    private primeiraCarga = true;
    private filtroEstavaAtivo = false;

    statusOptions = [
        { value: '', label: '- Selecione -' },
        { value: 'SUCESSO', label: 'SUCESSO' },
        { value: 'ERRO', label: 'ERRO' }
    ];

    columns: TableColumn[] = 
    [
        { field: 'idContrato', header: 'Contrato', width: '80px', align: 'center'},
        { field: 'dataProcessamento', header: 'Data Proc.', align: 'center'},
        { field: 'status', header: 'Status', align: 'center',
            cellClass: (row) => row.status === 'ERRO' ? 'status-error' : 'status-success'
        },
        { field: 'falha', header: 'Falha', align: 'left'}
    ];

    ngOnInit(): void {
       this.CarregarPagamentos(); 
       this.pagamentoSignalrService.iniciarConexao(() => {
            if(this.possuiFiltroAtivo()) {
                return;
            } 
            this.CarregarPagamentos(); 
        }); 
    }

    private possuiFiltroAtivo(): boolean {
        return (this.contratoFiltro.trim() !== '' || this.statusFiltro.trim() !== '');
    }

    mostrarToast(mensagem: string): void {
        this.mensagemToast.set(mensagem);
        this.toastVisivel.set(true);

        setTimeout(() => {
            this.toastVisivel.set(false);
        }, 5000);  
    }
  
    CarregarPagamentos(): void {
        this.pagamentoService.GetPagamentos().subscribe({
            next: (data) => {
                if (this.primeiraCarga) {
                    data.forEach(pagamento => {
                        this.pagamentosConhecidos.set(String(pagamento.idTransacao), pagamento.status);
                    });
                    this.primeiraCarga = false;
                } else {
                    data.forEach(pagamento => { 
                        const statusAnterior = this.pagamentosConhecidos.get(String(pagamento.idTransacao));
                        const pagamentoNovo = statusAnterior === undefined;
                        const virouErro = statusAnterior !== 'ERRO' && pagamento.status === 'ERRO';

                        if (pagamentoNovo && pagamento.status === 'ERRO') {
                            this.mostrarToast(pagamento.falha);
                        } else {
                            if (virouErro) {
                                 this.mostrarToast(pagamento.falha);
                            }
                        }

                        this.pagamentosConhecidos.set(String(pagamento.idTransacao), pagamento.status);
                    });
                }

                this.pagamentos.set(data);
                this.pagamentosSemFiltro.set(data);
                this.carregando = false; 
            },

            error: (error) => {
                console.error('Erro ao carregar pagamentos:', error );
                this.carregando = false;
            }
        });
    }

    PesquisarPagamentos(): void {  
        const contrato = this.contratoFiltro.trim();
        const status = this.statusFiltro;
        const filtroAtivo = contrato !== '' || status !== '';

        if (!filtroAtivo) {
            this.pagamentos.set([...this.pagamentosSemFiltro()]);

            if (this.filtroEstavaAtivo) {
                this.CarregarPagamentos();
            }
            
            this.filtroEstavaAtivo = false;

            return;
        }

        this.filtroEstavaAtivo = true;

        const resultado = this.pagamentosSemFiltro().filter(pagamento => {
            const contratoFiltrado = !contrato || String(pagamento.idContrato).includes(contrato);
            const statusFiltrado = !status || pagamento.status === status; 

            return contratoFiltrado && statusFiltrado;
        });

        this.pagamentos.set(resultado);
    }
}





