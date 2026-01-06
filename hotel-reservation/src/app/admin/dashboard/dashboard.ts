import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { AdminService } from '../services/admin.service';
import { Reservation } from '../../shared/models/reservation.models';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, SharedModule],
    templateUrl: './dashboard.html',
    styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
    stats = {
        totalHotels: 0,
        totalRooms: 0,
        activeBookings: 0,
        totalRevenue: 0
    };
    recentReservations: Reservation[] = [];
    isLoading = true;

    constructor(private adminService: AdminService) { }

    ngOnInit() {
        this.loadDashboardData();
    }

    loadDashboardData() {
        this.isLoading = true;
        forkJoin({
            hotels: this.adminService.getHotels(),
            rooms: this.adminService.getRooms(),
            reservations: this.adminService.getReservations()
        }).subscribe({
            next: (data) => {
                this.stats.totalHotels = data.hotels.length;
                this.stats.totalRooms = data.rooms.length;

                
                this.processReservations(data.reservations);

                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading dashboard data', err);
                this.isLoading = false;
            }
        });
    }

    private processReservations(reservations: Reservation[]) {
        // Active bookings: status is Confirmed or CheckedIn
        const activeDefaults = ['Confirmed', 'CheckedIn'];
        this.stats.activeBookings = reservations.filter(r => activeDefaults.includes(r.status)).length;

        // Total Revenue: sum of totalAmount
        this.stats.totalRevenue = reservations.reduce((sum, r) => sum + r.totalAmount, 0);

        
      
        this.recentReservations = [...reservations]
            .sort((a, b) => b.id - a.id)
            .slice(0, 5);
    }
}

