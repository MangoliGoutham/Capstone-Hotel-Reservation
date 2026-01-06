import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService } from '../../services/admin.service';
import { Hotel } from '../../../shared/models/hotel.models';
import { SharedModule } from '../../../shared/shared.module';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../../../shared/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-hotel-list',
  standalone: true,
  imports: [CommonModule, RouterModule, SharedModule, ConfirmDialogComponent],
  templateUrl: './hotel-list.html',
  styleUrl: './hotel-list.css'
})
export class HotelListComponent implements OnInit, AfterViewInit {
  dataSource = new MatTableDataSource<Hotel>([]);
  displayedColumns: string[] = ['name', 'city', 'rating', 'actions'];
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private adminService: AdminService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.loadHotels();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadHotels() {
    this.isLoading = true;
    this.adminService.getHotels().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading hotels', error);
        this.notificationService.show('Failed to load hotels.', 'error');
        this.isLoading = false;
      }
    });
  }

  deleteHotel(id: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Hotel',
        message: 'Are you sure you want to delete this hotel? This action cannot be undone.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminService.deleteHotel(id).subscribe({
          next: () => {
            this.dataSource.data = this.dataSource.data.filter(h => h.id !== id);
            this.notificationService.show('Hotel deleted successfully.', 'success');
          },
          error: (error) => {
            console.error('Error deleting hotel', error);
            this.notificationService.show('Failed to delete hotel.', 'error');
          }
        });
      }
    });
  }
}
