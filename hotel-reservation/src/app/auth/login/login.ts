import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { SharedModule } from '../../shared/shared.module';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, SharedModule],
    templateUrl: './login.html',
    styleUrl: './login.css'
})
export class LoginComponent {
    credentials = { email: '', password: '' };
    isLoading = false;
    errorMessage = '';

    constructor(
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute,
        private notificationService: NotificationService
    ) { }

    onSubmit() {
        if (!this.credentials.email || !this.credentials.password) {
            this.notificationService.show('Please enter both email and password.', 'error');
            return;
        }

        this.isLoading = true;

        this.authService.login(this.credentials).subscribe({
            next: (response) => {
                this.isLoading = false;
                this.notificationService.show('Login successful!', 'success');
                this.redirectUser(response.role);
            },
            error: (err) => {
                this.isLoading = false;
                let msg = 'An error occurred. Please try again.';

                if (err.status === 0) {
                    msg = 'Network error. Please check if the backend is running.';
                } else if (err.status === 401) {
                    msg = 'Invalid email or password';
                } else if (err.status >= 500) {
                    msg = 'Server error. Please try again later.';
                }

                this.notificationService.show(msg, 'error');
                console.error('Login error:', err);
            }
        });
    }

    private redirectUser(role: string) {
        // Check for returnUrl
        const returnUrl = this.route.snapshot.queryParams['returnUrl'];
        if (returnUrl) {
            this.router.navigateByUrl(returnUrl);
            return;
        }

        // Role-based redirection
        switch (role) {
            case 'Admin':
            case 'HotelManager':
                this.router.navigate(['/admin']);
                break;
            case 'Receptionist':
                this.router.navigate(['/staff']);
                break;
            case 'Guest':
                this.router.navigate(['/']);
                break;
            default:
                this.router.navigate(['/']);
                break;
        }
    }
}
