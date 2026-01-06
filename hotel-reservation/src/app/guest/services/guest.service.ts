import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Hotel } from '../../shared/models/hotel.models';
import { Room } from '../../shared/models/room.models';
import { CreateReservationDto, Reservation } from '../../shared/models/reservation.models';

@Injectable({
    providedIn: 'root'
})
export class GuestService {
    private apiUrl = '/api';

    constructor(private http: HttpClient) { }

    getHotels(): Observable<Hotel[]> {
        return this.http.get<Hotel[]>(`${this.apiUrl}/Hotels`);
    }

    getHotel(id: number): Observable<Hotel> {
        return this.http.get<Hotel>(`${this.apiUrl}/Hotels/${id}`);
    }

    getRoomsByHotel(hotelId: number): Observable<Room[]> {
        return this.http.get<Room[]>(`${this.apiUrl}/Rooms/hotel/${hotelId}`); // Verify this endpoint exists and is public?
        // AdminService used /rooms/hotel/{id}. Let's check RoomsController. 
        // If not public, we might need to rely on generic room fetch or ensure RoomsController is public.
    }

    createReservation(reservation: CreateReservationDto): Observable<Reservation> {
        return this.http.post<Reservation>(`${this.apiUrl}/Reservations`, reservation);
    }

    getMyBookings(): Observable<Reservation[]> {
        return this.http.get<Reservation[]>(`${this.apiUrl}/Reservations/my-reservations`);
    }

    cancelReservation(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/Reservations/${id}`);
    }
}
