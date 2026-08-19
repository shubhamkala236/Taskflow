import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { WeatherService } from './weather.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('taskflow-web');

  private readonly weatherService = inject(WeatherService);
  protected readonly apiMessage = signal('Calling API...');

  ngOnInit(): void {
    this.weatherService.hello().subscribe({
      next: (message) => this.apiMessage.set(message),
      error: () => this.apiMessage.set('API call failed — check console for CORS/network errors')
    });
  }
}
