import { Routes } from '@angular/router';
import { authGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'admin',
        loadChildren: () => import('./admin/admin.routes').then(m => m.adminRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'staff',
        loadChildren: () => import('./staff/staff.routes').then(m => m.staffRoutes),
        canActivate: [authGuard]
    },
    { path: 'auth/login', loadComponent: () => import('./auth/login/login').then(m => m.LoginComponent) },
    { path: 'auth/register', loadComponent: () => import('./auth/register/register').then(m => m.RegisterComponent) },
    {
        path: '',
        loadChildren: () => import('./guest/guest.routes').then(m => m.guestRoutes)
    }
];
