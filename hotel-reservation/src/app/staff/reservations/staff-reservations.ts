import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../admin/services/admin.service';
import { AuthService } from '../../auth/services/auth.service';
import { Reservation } from '../../shared/models/reservation.models';
import { SharedModule } from '../../shared/shared.module';
import { NotificationService } from '../../shared/services/notification.service';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-staff-reservations',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, ConfirmDialogComponent],
  templateUrl: './staff-reservations.html',
  styleUrl: './staff-reservations.css'
})
export class StaffReservationsComponent implements OnInit {
  reservations: Reservation[] = [];
  filteredReservations: Reservation[] = [];
  isLoading = true;
  searchTerm = '';
  userRole: string = '';

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    const user = this.authService.currentUserValue;
    this.userRole = user ? user.role : '';
    this.loadReservations();
  }

  loadReservations() {
    this.isLoading = true;
    this.adminService.getReservations().subscribe({
      next: (data) => {
        this.reservations = data;
        this.filteredReservations = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  filterReservations() {
    if (!this.searchTerm) {
      this.filteredReservations = this.reservations;
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredReservations = this.reservations.filter(r =>
        r.reservationNumber.toLowerCase().includes(term) ||
        (r.hotelName && r.hotelName.toLowerCase().includes(term)) // Check if hotelName exists on DTO
      );
    }
    this.applySort();
  }

  // Sorting
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

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

    this.filteredReservations.sort((a: any, b: any) => {
      let valA = a[this.sortColumn];
      let valB = b[this.sortColumn];

      // Handle specifics if needed, e.g. dates or numbers
      // For now, simple comparison works for strings/numbers
      if (typeof valA === 'string') valA = valA.toLowerCase();
      if (typeof valB === 'string') valB = valB.toLowerCase();

      if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
      if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }

  updateStatus(reservation: Reservation, newStatus: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Update Status',
        message: `Are you sure you want to change status to ${newStatus}?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminService.updateReservationStatus(reservation.id, newStatus).subscribe({
          next: () => {
            reservation.status = newStatus;
            this.notificationService.show(`Reservation status updated to ${newStatus}`, 'success');
            // Optionally reload to get fresh data
          },
          error: (err) => {
            console.error(err);
            this.notificationService.show('Failed to update reservation status.', 'error');
          }
        });
      }
    });
  }
}
