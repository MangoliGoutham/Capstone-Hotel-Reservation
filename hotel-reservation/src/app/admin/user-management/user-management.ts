import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, ConfirmDialogComponent],
  template: `
    <div class="container mt-3">
      <div class="page-header">
        <h2>User Management</h2>
      </div>

      <div *ngIf="showEditForm" class="card fade-in">
        <h3 class="mb-3">Edit User Role</h3>
        <form [formGroup]="userForm" (ngSubmit)="onSubmit()">
          <div class="d-flex flex-column gap-2 mb-3">
            <div class="form-group">
                <label>Full Name</label>
                <input type="text" formControlName="name" class="form-control" readonly>
            </div>
            
            <div class="form-group">
                <label>Email</label>
                <input type="email" formControlName="email" class="form-control" readonly>
            </div>

            <div class="form-group">
                <label>Role</label>
                <select formControlName="role" class="form-control">
                    <option value="Guest">Guest</option>
                    <option value="Receptionist">Receptionist</option>
                    <option value="HotelManager">Hotel Manager</option>
                    <option value="Admin">Admin</option>
                </select>
            </div>
          </div>

          <div class="d-flex gap-2">
              <button type="submit" [disabled]="!userForm.valid" class="btn btn-success flex-grow-1">Save Changes</button>
              <button type="button" class="btn btn-secondary" (click)="cancelEdit()">Cancel</button>
          </div>
        </form>
      </div>

      <div class="card p-0 overflow-hidden">
        <div class="p-3 border-bottom">
            <h3 class="m-0">All Users</h3>
        </div>
        <table class="elegant-table">
          <thead>
            <tr>
              <th (click)="sort('name')" style="cursor: pointer">
                Name <span *ngIf="sortColumn === 'name'">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
              </th>
              <th (click)="sort('email')" style="cursor: pointer">
                Email <span *ngIf="sortColumn === 'email'">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
              </th>
              <th (click)="sort('role')" style="cursor: pointer">
                Role <span *ngIf="sortColumn === 'role'">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
              </th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let user of userList">
              <td class="fw-bold">{{ user.firstName }} {{ user.lastName }}</td>
              <td>{{ user.email }}</td>
              <td>
                <span class="status-badge" 
                      [class.danger]="user.role === 'Admin'"
                      [class.success]="user.role === 'HotelManager'" 
                      [class.info]="user.role === 'Receptionist'">
                  {{ user.role }}
                </span>
              </td>
              <td>
                <div class="d-flex gap-1">
                    <button class="action-btn" (click)="startEdit(user)" title="Edit Role">✏️</button>
                    <button class="action-btn delete" (click)="deleteUser(user.id)" title="Delete User">🗑️</button>
                </div>
              </td>
            </tr>
            <tr *ngIf="userList.length === 0">
              <td colspan="4" class="text-center p-4 text-muted">No users found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: []
})
export class UserManagementComponent implements OnInit {
  userList: any[] = [];
  showEditForm = false;
  userForm: FormGroup;
  editingUserId: number | null = null;
  editingUser: any = null;

  // Sorting
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {
    this.userForm = this.fb.group({
      name: [''],
      email: [''],
      role: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    // Uses the new GET endpoint
    this.http.get<any[]>('/api/users').subscribe({
      next: (data) => {
        this.userList = data;
        this.applySort();
      },
      error: (err) => console.error('Failed to load users', err)
    });
  }

  sort(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.applySort();
  }

  applySort() {
    if (!this.sortColumn) return;

    this.userList.sort((a, b) => {
      let valA: any = '';
      let valB: any = '';

      if (this.sortColumn === 'name') {
        valA = (a.firstName + ' ' + a.lastName).toLowerCase();
        valB = (b.firstName + ' ' + b.lastName).toLowerCase();
      } else {
        valA = a[this.sortColumn]?.toLowerCase() || '';
        valB = b[this.sortColumn]?.toLowerCase() || '';
      }

      if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
      if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
      return 0;
    });

  }

  getBadgeClass(role: string): string {
    switch (role) {
      case 'Admin': return 'badge-admin';
      case 'HotelManager': return 'badge-manager';
      case 'Receptionist': return 'badge-receptionist';
      default: return 'badge-guest';
    }
  }

  startEdit(user: any) {
    this.editingUser = user;
    this.editingUserId = user.id;
    this.userForm.patchValue({
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      role: user.role
    });
    this.showEditForm = true;
  }

  cancelEdit() {
    this.showEditForm = false;
    this.editingUserId = null;
    this.editingUser = null;
  }

  onSubmit() {
    if (this.userForm.valid && this.editingUserId) {
      // Prepare payload, keeping existing name/email intact, mostly creating a proper DTO to satisfy backend
      // Ideally backend wants full DTO, so we must send all fields even if we only change Role
      // OR we can make a lightweight Patch endpoint. 
      // For now, re-sending existing data with new role.

      const nameParts = this.userForm.get('name')?.value.split(' ');
      const firstName = nameParts[0];
      const lastName = nameParts.length > 1 ? nameParts.slice(1).join(' ') : ''; // Rough reconstruction if name wasn't editable

      // Better: Use the ORIGINAL user object for detailed fields
      const payload = {
        firstName: this.editingUser.firstName,
        lastName: this.editingUser.lastName,
        email: this.editingUser.email,
        role: this.userForm.value.role
      };

      this.http.put(`/api/users/${this.editingUserId}`, payload).subscribe({
        next: () => {
          this.loadUsers();
          this.cancelEdit();
          this.notificationService.show('User role updated successfully.', 'success');
        },
        error: (err) => this.notificationService.show('Failed to update role: ' + (err.error?.message || err.message), 'error')
      });
    }
  }

  deleteUser(id: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete User',
        message: 'Are you sure you want to delete this user? This action cannot be undone.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.http.delete(`/api/users/${id}`).subscribe({
          next: () => {
            this.loadUsers();
            this.notificationService.show('User deleted successfully.', 'success');
          },
          error: (err) => {
            console.error('Failed to delete user', err);
            this.notificationService.show('Failed to delete user.', 'error');
          }
        });
      }
    });
  }
}
