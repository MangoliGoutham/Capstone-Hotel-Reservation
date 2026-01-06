export interface Room {
    id: number;
    roomNumber: string;
    roomType: string;
    basePrice: number;
    capacity: number;
    description?: string;
    status: string;
    hotelId: number;
    hotelName: string;
}

export interface CreateRoomDto {
    roomNumber: string;
    roomType: string;
    basePrice: number;
    capacity: number;
    description?: string;
    hotelId: number;
}
