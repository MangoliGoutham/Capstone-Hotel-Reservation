import { Component, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '../../services/notification.service';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-notifications',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './notifications.component.html',
    styleUrl: './notifications.component.css'
})
export class NotificationsComponent {
    isOpen = false;
    notifications$: Observable<Notification[]>;
    unreadCount$: Observable<number>;

    constructor(
        private notificationService: NotificationService,
        private elementRef: ElementRef
    ) {
        this.notifications$ = this.notificationService.notifications$;
        this.unreadCount$ = this.notificationService.unreadCount$;
    }

    toggleDropdown() {
        this.isOpen = !this.isOpen;
        if (this.isOpen) {
            // Refresh when opening
            this.notificationService.getMyNotifications().subscribe();
        }
    }

    markAsRead(notification: Notification) {
        if (!notification.isRead) {
            this.notificationService.markAsRead(notification.id).subscribe();
        }
    }

    @HostListener('document:click', ['$event'])
    clickout(event: any) {
        if (!this.elementRef.nativeElement.contains(event.target)) {
            this.isOpen = false;
        }
    }
}
