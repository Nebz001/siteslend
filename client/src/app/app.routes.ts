import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'SitesLend',
    loadComponent: () => import('./home/home').then((m) => m.Home),
  },
  {
    path: 'assets',
    title: 'Assets | SitesLend',
    loadComponent: () => import('./assets/asset-list').then((m) => m.AssetList),
  },
];
