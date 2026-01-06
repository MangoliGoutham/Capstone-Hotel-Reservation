import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../auth/services/auth.service';
import { AdminService } from '../../admin/services/admin.service';
import { forkJoin, switchMap, of } from 'rxjs';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { Reservation } from '../../shared/models/reservation.models';
import { Room } from '../../shared/models/room.models';

@Component({
    selector: 'app-staff-dashboard',
    standalone: true,
    imports: [CommonModule, BaseChartDirective],
    templateUrl: './dashboard.html',
    styleUrl: './dashboard.css'
})
export class StaffDashboardComponent implements OnInit {
    stats = {
        totalRooms: 0,
        activeBookings: 0,
        pendingRequests: 0,
        todayArrivals: 0,
        todayDepartures: 0
    };

    isLoading = true;

    // Pie Chart (Reservation Status)
    public pieChartOptions: ChartConfiguration['options'] = {
        responsive: true,
        plugins: {
            legend: { display: true, position: 'top' },
            title: { display: true, text: 'Reservation Status' }
        }
    };
    public pieChartData: ChartData<'pie', number[], string | string[]> = {
        labels: [['Confirmed'], ['Checked In'], ['Checked Out'], ['Cancelled']],
        datasets: [{ data: [0, 0, 0, 0] }]
    };
    public pieChartType: ChartType = 'pie';

    // Bar Chart (Rooms by Type)
    public barChartOptions: ChartConfiguration['options'] = {
        responsive: true,
        scales: { y: { min: 0, ticks: { stepSize: 1 } } },
        plugins: {
            legend: { display: true },
            title: { display: true, text: 'Room Distribution by Type' }
        }
    };
    public barChartData: ChartData<'bar'> = {
        labels: [],
        datasets: [{ data: [], label: 'Count' }]
    };
    public barChartType: ChartType = 'bar';

    constructor(
        public authService: AuthService,
        private adminService: AdminService
    ) { }

    ngOnInit() {
        this.loadDashboardData();
    }

    loadDashboardData() {
        this.isLoading = true;
        this.authService.currentUser$.pipe(
            switchMap(user => {
                console.log('Current User in Dashboard:', user);
                if (!user || !user.hotelId) {
                    console.warn('No user or hotelId found');
                    return of(null);
                }

                console.log('Fetching data for Hotel ID:', user.hotelId);
                // Fetch data specific to this hotel
                return forkJoin({
                    rooms: this.adminService.getRoomsByHotel(user.hotelId),
                    reservations: this.adminService.getReservationsByHotel(user.hotelId)
                });
            })
        ).subscribe({
            next: (data) => {
                console.log('Dashboard Data Received:', data);
                if (data) {
                    this.processData(data.rooms, data.reservations);
                }
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading dashboard data', err);
                this.isLoading = false;
            }
        });
    }

    private processData(rooms: Room[], reservations: Reservation[]) {
        // Stats
        this.stats.totalRooms = rooms.length;
        this.stats.activeBookings = reservations.filter(r => ['Confirmed', 'CheckedIn', 'Checked-in'].includes(r.status)).length;

        // Pending Requests: Include 'Pending' and 'Booked'
        this.stats.pendingRequests = reservations.filter(r => ['Pending', 'Booked'].includes(r.status)).length;

        // Date Comparison (ISO String YYYY-MM-DD)
        const today = new Date();
        const todayStr = today.toISOString().split('T')[0];

        this.stats.todayArrivals = reservations.filter(r => {
            if (!r.checkInDate) return false;
            // Handle both full ISO string and YYYY-MM-DD format
            const checkInStart = r.checkInDate.toString().split('T')[0];
            return checkInStart === todayStr && ['Confirmed', 'Booked'].includes(r.status);
        }).length;

        this.stats.todayDepartures = reservations.filter(r => {
            if (!r.checkOutDate) return false;
            const checkOutStart = r.checkOutDate.toString().split('T')[0];
            return checkOutStart === todayStr && ['CheckedIn', 'Checked-in'].includes(r.status);
        }).length;

        // Pie Chart
        const confirmed = reservations.filter(r => r.status === 'Confirmed').length;
        const booked = reservations.filter(r => r.status === 'Booked').length; // Add Booked
        const checkedIn = reservations.filter(r => ['CheckedIn', 'Checked-in'].includes(r.status)).length;
        const checkedOut = reservations.filter(r => ['CheckedOut', 'Checked-out'].includes(r.status)).length;
        const cancelled = reservations.filter(r => r.status === 'Cancelled').length;

        this.pieChartData = {
            labels: [['Confirmed'], ['Booked'], ['Checked In'], ['Checked Out'], ['Cancelled']],
            datasets: [{
                data: [confirmed, booked, checkedIn, checkedOut, cancelled],
                backgroundColor: ['#3b82f6', '#8b5cf6', '#10b981', '#64748b', '#ef4444']
            }]
        };

        this.pieChartData = {
            labels: [['Confirmed'], ['Checked In'], ['Checked Out'], ['Cancelled']],
            datasets: [{
                data: [confirmed, checkedIn, checkedOut, cancelled],
                backgroundColor: ['#3b82f6', '#10b981', '#64748b', '#ef4444']
            }]
        };

        // Bar Chart (Rooms by Type)
        const roomTypeCounts: { [type: string]: number } = {};
        rooms.forEach(r => {
            roomTypeCounts[r.roomType] = (roomTypeCounts[r.roomType] || 0) + 1;
        });

        this.barChartData = {
            labels: Object.keys(roomTypeCounts),
            datasets: [{
                data: Object.values(roomTypeCounts),
                label: 'Rooms',
                backgroundColor: '#4f46e5'
            }]
        };
    }
}
