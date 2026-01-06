import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    private apiUrl = '/api/Reports';

    constructor(private http: HttpClient) { }

    getOccupancy(date?: string): Observable<any> {
        let params = new HttpParams();
        if (date) params = params.set('date', date);
        return this.http.get(`${this.apiUrl}/occupancy`, { params });
    }

    getRevenue(startDate?: string, endDate?: string): Observable<any> {
        let params = new HttpParams();
        if (startDate) params = params.set('startDate', startDate);
        if (endDate) params = params.set('endDate', endDate);
        return this.http.get(`${this.apiUrl}/revenue`, { params });
    }

    getReservationSummary(startDate?: string, endDate?: string): Observable<any> {
        let params = new HttpParams();
        if (startDate) params = params.set('startDate', startDate);
        if (endDate) params = params.set('endDate', endDate);
        return this.http.get(`${this.apiUrl}/reservation-summary`, { params });
    }
}
