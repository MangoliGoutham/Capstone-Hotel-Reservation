import { Routes } from '@angular/router';
import { GuestLayoutComponent } from './layout/guest-layout';
import { HomeComponent } from './home/home';
import { authGuard } from '../auth/guards/auth.guard';

export const guestRoutes: Routes = [
    {
        path: '',
        component: GuestLayoutComponent,
        children: [
            { path: '', component: HomeComponent },
            { path: 'hotels/:id', loadComponent: () => import('./hotel-details/hotel-details').then(m => m.HotelDetailsComponent) },
            {
                path: 'my-bookings',
                loadComponent: () => import('./my-bookings/my-bookings').then(m => m.MyBookingsComponent),
                canActivate: [authGuard]
            }
        ]
    }
];
