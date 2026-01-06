import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GuestService } from '../services/guest.service';
import { SharedModule } from '../../shared/shared.module';
import { AuthService } from '../../auth/services/auth.service';
import { NotificationService } from '../../shared/services/notification.service';
import { Hotel } from '../../shared/models/hotel.models';
import { Room } from '../../shared/models/room.models';

@Component({
    selector: 'app-hotel-details',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, SharedModule],
    templateUrl: './hotel-details.html',
    styleUrl: './hotel-details.css'
})
export class HotelDetailsComponent implements OnInit {
    hotel?: Hotel;
    availableRooms: Room[] = [];
    isLoading = true;
    errorMessage = '';

    // Booking Logic
    isLoggedIn = false;
    showBookingModal = false;
    selectedRoom?: Room;

    bookingData = {
        checkInDate: '',
        checkOutDate: '',
        guests: 2
    };

    minDate: string = new Date().toISOString().split('T')[0];
    isBookingLoading = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private guestService: GuestService,
        private authService: AuthService,
        private notificationService: NotificationService
    ) {
        // Subscribe to auth state to update UI
        this.authService.currentUser$.subscribe(user => {
            this.isLoggedIn = !!user;
        });
    }

    ngOnInit() {
        // Set default dates (tomorrow and day after)
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        const dayAfter = new Date(tomorrow);
        dayAfter.setDate(dayAfter.getDate() + 1);

        this.bookingData.checkInDate = tomorrow.toISOString().split('T')[0];
        this.bookingData.checkOutDate = dayAfter.toISOString().split('T')[0];

        const id = this.route.snapshot.params['id'];
        if (id) {
            this.loadData(id);
        } else {
            this.notificationService.show('Invalid Hotel ID', 'error');
            this.isLoading = false;
        }
    }

    loadData(id: number) {
        this.isLoading = true;
        this.guestService.getHotel(id).subscribe({
            next: (hotel) => {
                this.hotel = hotel;
                this.loadRooms(id);
            },
            error: (err) => {
                console.error(err);
                this.notificationService.show('Failed to load hotel details.', 'error');
                this.isLoading = false;
            }
        });
    }

    loadRooms(hotelId: number) {
        this.guestService.getRoomsByHotel(hotelId).subscribe({
            next: (rooms) => {
                this.availableRooms = rooms; // In real app, filter by availability based on dates
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading rooms', err);
                this.notificationService.show('Failed to load rooms.', 'error');
                this.isLoading = false;
            }
        });
    }

    openBookingModal(room: Room) {
        if (!this.isLoggedIn) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
            return;
        }
        this.selectedRoom = room;
        this.showBookingModal = true;
    }

    closeBookingModal() {
        this.showBookingModal = false;
        this.selectedRoom = undefined;
    }

    get numberOfNights(): number {
        if (!this.bookingData.checkInDate || !this.bookingData.checkOutDate) return 0;
        const start = new Date(this.bookingData.checkInDate);
        const end = new Date(this.bookingData.checkOutDate);
        const diff = end.getTime() - start.getTime();
        const nights = Math.ceil(diff / (1000 * 3600 * 24));
        return nights > 0 ? nights : 0;
    }

    get bookingTotal(): number {
        if (!this.selectedRoom) return 0;
        return this.selectedRoom.basePrice * this.numberOfNights;
    }

    calculateTotal() {
        // Triggered by date change, getters handle calculation
    }

    confirmBooking() {
        if (!this.selectedRoom) return;

        this.isBookingLoading = true;
        const booking = {
            roomId: this.selectedRoom.id,
            checkInDate: new Date(this.bookingData.checkInDate),
            checkOutDate: new Date(this.bookingData.checkOutDate),
            numberOfGuests: this.bookingData.guests,
            specialRequests: 'Online Booking'
        };

        this.guestService.createReservation(booking).subscribe({
            next: () => {
                this.isBookingLoading = false;
                this.closeBookingModal();
                this.notificationService.show('Booking Confirmed! Thank you for choosing us.', 'success');
                this.router.navigate(['/my-bookings']);
            },
            error: (err) => {
                console.error(err);
                this.isBookingLoading = false;
                const msg = err.error?.message || 'Booking failed. Please try again.';
                this.notificationService.show(msg, 'error');
            }
        });
    }
}
