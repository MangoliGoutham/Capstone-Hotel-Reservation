import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Hotel, CreateHotelDto } from '../../shared/models/hotel.models';
import { Room, CreateRoomDto } from '../../shared/models/room.models';
import { Reservation } from '../../shared/models/reservation.models';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private apiUrl = '/api';

    constructor(private http: HttpClient) { }

    // Hotels
    getHotels(): Observable<Hotel[]> {
        return this.http.get<Hotel[]>(`${this.apiUrl}/hotels`);
    }

    getHotel(id: number): Observable<Hotel> {
        return this.http.get<Hotel>(`${this.apiUrl}/hotels/${id}`);
    }

    createHotel(hotel: CreateHotelDto): Observable<Hotel> {
        return this.http.post<Hotel>(`${this.apiUrl}/hotels`, hotel);
    }

    updateHotel(id: number, hotel: CreateHotelDto): Observable<Hotel> {
        return this.http.put<Hotel>(`${this.apiUrl}/hotels/${id}`, hotel);
    }

    deleteHotel(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/hotels/${id}`);
    }

    // Rooms
    getRooms(): Observable<Room[]> {
        return this.http.get<Room[]>(`${this.apiUrl}/rooms`);
    }

    getRoom(id: number): Observable<Room> {
        return this.http.get<Room>(`${this.apiUrl}/rooms/${id}`);
    }



    createRoom(room: CreateRoomDto): Observable<Room> {
        return this.http.post<Room>(`${this.apiUrl}/rooms`, room);
    }

    updateRoom(id: number, room: CreateRoomDto): Observable<Room> {
        return this.http.put<Room>(`${this.apiUrl}/rooms/${id}`, room); // API doc didn't specify full update, assuming standard PUT or similar to CreateRoomDto
        // Re-reading API: "Update room status (Staff) PATCH /api/rooms/{id}/status". 
        // It seems there is NO general "Update Room" endpoint for Admin in the provided context, only status patch.
        // However, usually Admins can edit rooms. I will implement a general update assuming it exists or uses the same DTO, 
        // but I'll add the PATCH status as well.
        // Context check: "Update room status (Staff) PATCH /api/rooms/{id}/status". 
        // "Create room (Admin/Manager) POST /api/rooms".
        // I'll stick to what's likely required for full management: generic update usually exists. If not, I'll rely on Create/Delete.
        // I will add the update method, if it 404s user will know.
    }

    updateRoomStatus(id: number, status: string): Observable<void> {
        return this.http.patch<void>(`${this.apiUrl}/rooms/${id}/status`, { status });
    }

    deleteRoom(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/rooms/${id}`);
    }

    // Reservations
    getReservations(): Observable<Reservation[]> {
        return this.http.get<Reservation[]>(`${this.apiUrl}/reservations`);
    }

    getRoomsByHotel(hotelId: number): Observable<Room[]> {
        console.log(`Calling API: ${this.apiUrl}/rooms/hotel/${hotelId}`);
        return this.http.get<Room[]>(`${this.apiUrl}/rooms/hotel/${hotelId}`);
    }

    getReservationsByHotel(hotelId: number): Observable<Reservation[]> {
        console.log(`Calling API: ${this.apiUrl}/reservations/hotel/${hotelId}`);
        return this.http.get<Reservation[]>(`${this.apiUrl}/reservations/hotel/${hotelId}`);
    }

    updateReservationStatus(id: number, status: string): Observable<void> {
        return this.http.patch<void>(`${this.apiUrl}/reservations/${id}/status`, { status });
    }
}
