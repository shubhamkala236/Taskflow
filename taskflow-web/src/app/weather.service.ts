import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '../environments/environment';

@Injectable({ providedIn: 'root' })
export class WeatherService {
  private readonly http = inject(HttpClient);

  hello() {
    return this.http.get(`${environment.apiUrl}/WeatherForecast/Hello`, { responseType: 'text' });
  }
}
