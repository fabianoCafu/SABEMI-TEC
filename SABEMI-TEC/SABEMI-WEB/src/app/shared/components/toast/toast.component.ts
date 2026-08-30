import { Component, input, output } from '@angular/core';

@Component({
    selector: 'app-toast',
    standalone: true,
    templateUrl: './toast.component.html',
    styleUrl: './toast.component.css'
})

export class ToastComponent {

    mensagem = input<string>('');
    fechar = output<void>();

    fecharToast(): void {
        this.fechar.emit();
    }
}