import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

import { NotificationsComponent } from '../../shared/components/notifications/notifications';

@Component({
    selector: 'app-guest-layout',
    standalone: true,
    imports: [CommonModule, RouterModule, NotificationsComponent],
    templateUrl: './guest-layout.html',
    styleUrl: './guest-layout.css'
})
export class GuestLayoutComponent {
    constructor(public authService: AuthService) { }

    logout() {
        this.authService.logout();
    }
}
