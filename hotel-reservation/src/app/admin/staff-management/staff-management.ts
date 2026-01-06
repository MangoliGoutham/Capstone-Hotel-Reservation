import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-staff-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, ConfirmDialogComponent],
  template: `
    <div class="container mt-3">
      <div class="page-header">
        <h2>Staff Management</h2>
        <button class="btn btn-primary" (click)="toggleForm()">
          <span class="icon" *ngIf="!showForm">+</span> {{ showForm ? 'Cancel' : 'Add New Staff' }}
        </button>
      </div>

      <div *ngIf="showForm" class="card fade-in">
        <h3 class="mb-3">Add New Staff Member</h3>
        <form [formGroup]="staffForm" (ngSubmit)="onSubmit()">
          <div class="d-flex flex-column gap-2 mb-3">
            <div class="form-group">
                <label>Full Name</label>
                <input type="text" formControlName="name" class="form-control" placeholder="John Doe">
            </div>
            
            <div class="d-flex gap-2">
                <div class="form-group w-50">
                    <label>Email</label>
                    <input type="email" formControlName="email" class="form-control" placeholder="john@example.com">
                </div>
                <div class="form-group w-50">
                    <label>Role</label>
                    <select formControlName="role" class="form-control">
                        <option value="Receptionist">Receptionist</option>
                        <option value="HotelManager">Hotel Manager</option>
                    </select>
                </div>
            </div>

            <div class="form-group">
                <label>Password</label>
                <input type="password" formControlName="password" class="form-control" placeholder="******">
            </div>
          </div>

          <button type="submit" [disabled]="!staffForm.valid" class="btn btn-success w-100">Create Staff</button>
        </form>
      </div>

      <div class="card p-0 overflow-hidden">
        <div class="p-3 border-bottom">
            <h3 class="m-0">Current Staff</h3>
        </div>
        <table class="elegant-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let user of staffList">
              <td>
                  <div class="fw-bold">{{ user.firstName }} {{ user.lastName }}</div>
              </td>
              <td>{{ user.email }}</td>
              <td>
                <span class="status-badge" [class.success]="user.role === 'HotelManager'" 
                      [class.info]="user.role === 'Receptionist'">
                  {{ user.role }}
                </span>
              </td>
              <td>
                <div class="d-flex gap-1">
                    <button class="action-btn" (click)="startEdit(user)" title="Edit">✏️</button>
                    <button class="action-btn delete" (click)="deleteStaff(user.id)" title="Delete">🗑️</button>
                </div>
              </td>
            </tr>
            <tr *ngIf="staffList.length === 0">
              <td colspan="4" class="text-center p-4 text-muted">No staff found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: []
})
export class StaffManagementComponent implements OnInit {
  staffList: any[] = [];
  showForm = false;
  staffForm: FormGroup;
  editingUserId: number | null = null;

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {
    this.staffForm = this.fb.group({
      name: ['', Validators.required], // Will need to split into first/last in backend or updated dto
      email: ['', [Validators.required, Validators.email]],
      password: [''], // Removed required validator from default, will handle manually
      role: ['Receptionist', Validators.required]
    });
  }

  ngOnInit() {
    this.loadStaff();
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (this.showForm) {
      if (this.editingUserId) {
        // Form is already populated if we clicked Edit
      } else {
        this.staffForm.reset({ role: 'Receptionist' });
        this.editingUserId = null;
      }
    }
  }

  loadStaff() {
    // Requires UsersController.GetStaff
    this.http.get<any[]>('/api/users/staff').subscribe({
      next: (data) => this.staffList = data,
      error: (err) => {
        console.error('Failed to load staff', err);
        this.notificationService.show('Failed to load staff list.', 'error');
      }
    });
  }

  startEdit(user: any) {
    this.editingUserId = user.id;
    this.staffForm.patchValue({
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      password: '', // Password optional on edit, but typically left blank in UI. Here we might need a separate Logic if password is not to be changed.
      // For simplicity, we are keeping the required validator if we want to force reset, OR we remove it for edit. 
      // To keep it simple: We will allow re-setting password.
      role: user.role
    });
    this.showForm = true;
  }

  onSubmit() {
    if (this.staffForm.valid) {
      const formVal = this.staffForm.value;
      const nameParts = formVal.name.split(' ');
      const firstName = nameParts[0];
      const lastName = nameParts.length > 1 ? nameParts.slice(1).join(' ') : '';

      const payload = {
        firstName,
        lastName,
        email: formVal.email,
        role: formVal.role,
        ...(formVal.password ? { password: formVal.password } : {}) // Only include password if provided
      };

      if (this.editingUserId) {
        // Update existing user
        this.http.put(`/api/users/${this.editingUserId}`, payload).subscribe({
          next: () => {
            this.loadStaff();
            this.showForm = false;
            this.editingUserId = null;
            this.staffForm.reset({ role: 'Receptionist' });
            this.notificationService.show('Staff updated successfully.', 'success');
          },
          error: (err) => this.notificationService.show('Failed to update user: ' + (err.error?.message || err.message), 'error')
        });
      } else {
        // Create new user (requires password)
        if (!formVal.password) {
          this.notificationService.show('Password is required for new users', 'error');
          return;
        }
        this.http.post('/api/auth/register', { ...payload, password: formVal.password }).subscribe({
          next: () => {
            this.loadStaff();
            this.showForm = false;
            this.staffForm.reset({ role: 'Receptionist' });
            this.notificationService.show('Staff member created successfully.', 'success');
          },
          error: (err) => this.notificationService.show('Failed to create user: ' + (err.error?.message || err.message), 'error')
        });
      }
    }
  }

  deleteStaff(id: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Staff Member',
        message: 'Are you sure you want to delete this staff member? This action cannot be undone.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.http.delete(`/api/users/${id}`).subscribe({
          next: () => {
            this.loadStaff();
            this.notificationService.show('Staff member deleted successfully.', 'success');
          },
          error: (err) => {
            console.error('Failed to delete staff', err);
            this.notificationService.show('Failed to delete staff member.', 'error');
          }
        });
      }
    });
  }
}
