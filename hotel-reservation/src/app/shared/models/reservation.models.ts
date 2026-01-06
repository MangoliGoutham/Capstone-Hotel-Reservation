export interface Reservation {
    id: number;
    reservationNumber: string;
    userId: number;
    roomId: number;
    checkInDate: Date;
    checkOutDate: Date;
    numberOfGuests: number;
    totalAmount: number;
    status: string;
    specialRequests?: string;
    userName: string;
    roomNumber: string;
    hotelName: string;
}

export interface CreateReservationDto {
    roomId: number;
    checkInDate: Date;
    checkOutDate: Date;
    numberOfGuests: number;
    specialRequests?: string;
}
