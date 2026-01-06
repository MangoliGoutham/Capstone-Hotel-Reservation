import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Toast } from '../../services/notification.service';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-toast',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './toast.component.html',
    styleUrls: ['./toast.component.css']
})
export class ToastComponent {
    toasts$: Observable<Toast[]>;

    constructor(private notificationService: NotificationService) {
        this.toasts$ = this.notificationService.toasts$;
    }

    remove(id: number) {
        this.notificationService.remove(id);
    }
}
