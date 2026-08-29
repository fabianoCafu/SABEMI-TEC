import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Pagamento } from '../../models/pagamento';
import { Observable, map } from 'rxjs';

@Injectable({
    providedIn: 'root'
})

export class PagamentoServices {

    private http = inject(HttpClient);
    private readonly pathApi = `${environment.apiUrl}/pagamentos-processados`;

    GetPagamentos(): Observable<Pagamento[]> {
        return this.http.get<any>(this.pathApi).pipe(
            map(response => {
                const pagamentos: Pagamento[] =
                response.object.map(
                    (pagamento: Pagamento) => ({
                         idTransacao: pagamento.idTransacao,
                         idContrato: pagamento.idContrato,
                         dataProcessamento: new Date( pagamento.dataProcessamento).toLocaleDateString('pt-BR'),
                         status: pagamento.status,
                         falha: pagamento.falha ?? ''
                    })
                );
                
                return pagamentos;
            })
        );
    }
}

