import { Routes } from '@angular/router';
import { StaffLayoutComponent } from './layout/staff-layout';
import { StaffDashboardComponent } from './dashboard/dashboard';
import { StaffReservationsComponent } from './reservations/staff-reservations';
import { authGuard } from '../auth/guards/auth.guard';

export const staffRoutes: Routes = [
    {
        path: '',
        component: StaffLayoutComponent,
        canActivate: [authGuard],
        children: [
            { path: 'dashboard', component: StaffDashboardComponent },
            {
                path: 'reservations',
                component: StaffReservationsComponent
            },
            {
                path: 'reports',
                loadComponent: () => import('../admin/reports/reports').then(m => m.AdminReportsComponent)
            },

            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    }
];
