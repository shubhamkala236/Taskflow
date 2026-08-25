import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

import { Tasks } from './tasks/tasks';

export const routes: Routes = [{ path: '', component: Tasks, canActivate: [MsalGuard] }];
