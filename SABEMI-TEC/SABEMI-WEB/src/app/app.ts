import { Component, signal } from '@angular/core';
import { HeaderComponent } from './shared/components/header/header';
import { HomeComponent } from './pages/home/home';
import { FooterComponent } from './shared/components/footer/footer';

@Component({
    selector: 'app-root',
    imports: [HeaderComponent, FooterComponent, HomeComponent],
    templateUrl: './app.html',
    styleUrls: ['./app.css']
})

export class App {
    protected readonly title = signal('SABEMI-TEC');
}
