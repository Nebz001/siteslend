import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'SitesLend',
    loadComponent: () => import('./home/home').then((m) => m.Home),
  },
];
