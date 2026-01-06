import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { SharedModule } from '../../shared/shared.module';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SharedModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  userData = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    phoneNumber: ''
  };
  isLoading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) { }

  onSubmit() {
    this.isLoading = true;

    this.authService.register(this.userData).subscribe({
      next: () => {
        this.isLoading = false;
        this.notificationService.show('Registration successful! Logging you in...', 'success');
        this.router.navigate(['/']); // Auto login/redirect after register
      },
      error: (err) => {
        this.isLoading = false;
        const msg = err.error?.message || 'Registration failed. Please try again.';
        this.notificationService.show(msg, 'error');
        console.error(err);
      }
    });
  }
}
