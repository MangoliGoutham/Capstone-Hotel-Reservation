# Hotel Reservation System - Capstone Project

## Overview
A modern full-stack web application designed for a hotel chain to manage room inventory, reservations, billing, and guests. Built with **ASP.NET Core (Backend)** and **Angular (Frontend)**, featuring clean architecture, JWT authentication, and role-based access control.

## Technology Stack
- **Backend**: ASP.NET Core Web API (.NET 10), Entity Framework Core, SQL Server
- **Frontend**: Angular 19, TypeScript, Bootstrap
- **Authentication**: JWT (JSON Web Tokens)
- **Tools**: Visual Studio / VS Code, SQL Server Management Studio

## Project Structure
- `HotelReservationSystem/` - Backend Web API solution
- `hotel-reservation/` - Angular Frontend application

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- Node.js (v18+) & npm
- SQL Server (LocalDB or full instance)
- Angular CLI (`npm install -g @angular/cli`)

### 1. Database Setup
The application uses Entity Framework Core Code-First approach.
1. Navigate to the backend folder:
   ```bash
   cd HotelReservationSystem
   ```
2. Update `appsettings.json` connection string if necessary (defaults to LocalDB).
3. Value seeding is automatic on application startup (`DataSeeder.cs`).

### 2. Backend Setup
1. Restore dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```
2. Run the application:
   ```bash
   dotnet run --project HotelReservationSystem
   ```
   Server will start on `https://localhost:7003`.
   Swagger UI is available at `https://localhost:7003/swagger`.

### 3. Frontend Setup
1. Navigate to the frontend folder:
   ```bash
   cd hotel-reservation
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm start
   ```
   Application will be available at `http://localhost:4200`.

## Usage & Login Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@hotel.com` | `admin123` |
| **Hotel Manager** | `manager@hotel.com` | `manager123` |
| **Receptionist** | `receptionist@hotel.com` | `receptionist123` |
| **Guest** | `virat@gmail.com` | `virat@123` |


## Features Implemented
- **User Management**: Registration, Login, Role-based Routing.
- **Reservation Lifecycle**: Search -> Book -> Confirm -> Check-In -> Check-Out -> Bill Generation -> Payment.
- **Billing**: Auto-calculation of stay duration + tax. "Self-healing" bill generation if missing.
- **Reporting**: Occupancy and Revenue reports.
- **Architecture**: Repository Pattern, Service Layer, Global Exception Handling.

## Deliverables
- Source Code (Backend + Frontend)
- Database Implementation (EF Core)
- Setup Instructions (This README)
