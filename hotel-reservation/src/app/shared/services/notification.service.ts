import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, timer } from 'rxjs';
import { switchMap, tap, share, retry } from 'rxjs/operators';
import { AuthService } from '../../auth/services/auth.service';

export interface Notification {
    id: number;
    message: string;
    type: string;
    createdAt: string;
    isRead: boolean;
}

export interface Toast {
    message: string;
    type: 'success' | 'error' | 'info';
    id: number;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private apiUrl = '/api/Notifications';
    private notificationsSubject = new BehaviorSubject<Notification[]>([]);
    public notifications$ = this.notificationsSubject.asObservable();

    private toastSubject = new BehaviorSubject<Toast[]>([]);
    public toasts$ = this.toastSubject.asObservable();

    // Polling interval
    private pollingInterval = 10000; // 10 seconds

    constructor(private http: HttpClient, private authService: AuthService) {
        this.startPolling();

        // Also trigger when user logs in
        this.authService.currentUser$.subscribe(user => {
            if (user) {
                this.getMyNotifications().subscribe();
            } else {
                this.notificationsSubject.next([]);
            }
        });
    }

    private startPolling() {
        timer(0, this.pollingInterval).pipe(
            switchMap(() => {
                // Return observable directly which fits switchMap
                if (this.authService.isAuthenticated()) {
                    return this.getMyNotifications();
                }
                return [];
            }),
            retry(),
            share()
        ).subscribe();
    }

    getMyNotifications(): Observable<Notification[]> {
        return this.http.get<Notification[]>(`${this.apiUrl}/my-notifications`).pipe(
            tap(notifications => this.notificationsSubject.next(notifications))
        );
    }

    markAsRead(id: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/read`, {}).pipe(
            tap(() => {
                const current = this.notificationsSubject.value;
                const updated = current.map(n => n.id === id ? { ...n, isRead: true } : n);
                this.notificationsSubject.next(updated.filter(n => !n.isRead)); // Optional: remove read ones or keep them
                // If API returns only unread specific endpoint, then local update might need adjustment. 
                // Current backend returns 'UnreadByUserIdAsync'. So locally removing them makes sense or refreshing.
                this.getMyNotifications().subscribe(); // Refresh list to match backend state
            })
        );
    }

    get unreadCount$(): Observable<number> {
        return new Observable(observer => {
            this.notifications$.subscribe(notifications => {
                observer.next(notifications.filter(n => !n.isRead).length);
            });
        });
    }

    show(message: string, type: 'success' | 'error' | 'info' = 'info') {
        const id = Date.now();
        const toast: Toast = { message, type, id };
        this.toastSubject.next([...this.toastSubject.value, toast]);

        // Auto remove after 3 seconds
        setTimeout(() => this.remove(id), 3000);
    }

    remove(id: number) {
        this.toastSubject.next(this.toastSubject.value.filter(t => t.id !== id));
    }
}
