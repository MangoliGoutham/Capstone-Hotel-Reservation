import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../auth/services/auth.service';

@Component({
    selector: 'app-staff-dashboard',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './dashboard.html',
    styleUrl: './dashboard.css'
})
export class StaffDashboardComponent {
    constructor(public authService: AuthService) { }
}
