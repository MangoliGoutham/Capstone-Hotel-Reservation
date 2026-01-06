import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout';
import { DashboardComponent } from './dashboard/dashboard';

export const adminRoutes: Routes = [
    {
        path: '',
        component: AdminLayoutComponent,
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard', component: DashboardComponent },
            { path: 'hotels', loadComponent: () => import('./hotels/hotel-list/hotel-list').then(m => m.HotelListComponent) },
            { path: 'hotels/new', loadComponent: () => import('./hotels/hotel-form/hotel-form').then(m => m.HotelFormComponent) },
            { path: 'hotels/edit/:id', loadComponent: () => import('./hotels/hotel-form/hotel-form').then(m => m.HotelFormComponent) },
            { path: 'rooms', loadComponent: () => import('./rooms/room-list/room-list').then(m => m.RoomListComponent) },
            { path: 'rooms/new', loadComponent: () => import('./rooms/room-form/room-form').then(m => m.RoomFormComponent) },
            { path: 'rooms/edit/:id', loadComponent: () => import('./rooms/room-form/room-form').then(m => m.RoomFormComponent) },
            { path: 'staff', loadComponent: () => import('./staff-management/staff-management').then(m => m.StaffManagementComponent) },
            { path: 'users', loadComponent: () => import('./user-management/user-management').then(m => m.UserManagementComponent) },


        ]
    }
];
