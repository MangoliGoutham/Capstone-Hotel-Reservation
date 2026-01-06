export interface Hotel {
    id: number;
    name: string;
    address: string;
    city: string;
    country: string;
    phoneNumber?: string;
    email?: string;
    description?: string;
    starRating: number;
    imageUrl?: string;
}

export interface CreateHotelDto {
    name: string;
    address: string;
    city: string;
    country: string;
    phoneNumber?: string;
    email?: string;
    description?: string;
    starRating: number;
    imageUrl?: string;
}
