export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber?: string;
}

export interface AuthResponseDto {
  token: string;
  role: string;
  firstName: string;
  lastName: string;
  hotelId?: number;
}

export interface User {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  hotelId?: number;
}
