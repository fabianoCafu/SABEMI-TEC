import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment.development';

@Injectable({
    providedIn: 'root'
})

export class PagamentoSignalrService {

    private hubConnection!: signalR.HubConnection;
    private readonly pathApi = `${environment.apiUrl}/pagamentos-hub`;

    iniciarConexao(callback: () => void): void {

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(this.pathApi)
            .withAutomaticReconnect()
            .build();

        this.hubConnection.on('PagamentoAtualizado', () => {
            callback();
        });

        this.hubConnection.start().then(() => {
            console.log('SignalR conectado');
        })
        .catch(error => {
            console.error('Erro SignalR:', error);
        });
    }
}
