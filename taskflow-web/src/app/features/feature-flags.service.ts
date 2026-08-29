import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '../../environments/environment';

interface FeatureFlags {
  attachments: boolean;
}

@Injectable({ providedIn: 'root' })
export class FeatureFlagsService {
  private readonly http = inject(HttpClient);

  getFeatures() {
    return this.http.get<FeatureFlags>(`${environment.apiUrl}/api/features`);
  }
}
