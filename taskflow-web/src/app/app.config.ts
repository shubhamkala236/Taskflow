import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { InteractionType, PublicClientApplication } from '@azure/msal-browser';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalInterceptor,
  MsalService
} from '@azure/msal-angular';

import { routes } from './app.routes';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: MSAL_INSTANCE,
      useFactory: () =>
        new PublicClientApplication({
          auth: {
            clientId: environment.msal.clientId,
            authority: environment.msal.authority,
            redirectUri: environment.msal.redirectUri
          },
          cache: { cacheLocation: 'localStorage' }
        })
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useValue: { interactionType: InteractionType.Redirect }
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: () => ({
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map([[`${environment.apiUrl}/api/*`, [environment.msal.apiScope]]])
      })
    },
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
    provideAppInitializer(() => {
      const msalInstance = inject(MSAL_INSTANCE);
      return msalInstance.initialize().then(() => msalInstance.handleRedirectPromise());
    }),
    MsalService,
    MsalGuard,
    MsalBroadcastService
  ]
};
