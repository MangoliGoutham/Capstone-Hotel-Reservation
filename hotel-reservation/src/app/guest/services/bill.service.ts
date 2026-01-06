import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Bill {
    id: number;
    billNumber: string;
    reservationId: number;
    roomCharges: number;
    additionalCharges: number;
    taxAmount: number;
    totalAmount: number;
    paymentStatus: string;
    createdAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class BillService {
    private apiUrl = '/api/Bills';

    constructor(private http: HttpClient) { }

    getBillByReservation(reservationId: number): Observable<Bill> {
        return this.http.get<Bill>(`${this.apiUrl}/reservation/${reservationId}`);
    }

    payBill(billId: number): Observable<Bill> {
        return this.http.post<Bill>(`${this.apiUrl}/${billId}/pay`, {});
    }
}
