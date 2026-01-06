import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService } from '../../services/admin.service';
import { Room } from '../../../shared/models/room.models';
import { SharedModule } from '../../../shared/shared.module';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../../../shared/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-room-list',
  standalone: true,
  imports: [CommonModule, RouterModule, SharedModule, ConfirmDialogComponent],
  templateUrl: './room-list.html',
  styleUrl: './room-list.css'
})
export class RoomListComponent implements OnInit, AfterViewInit {
  dataSource = new MatTableDataSource<Room>([]);
  displayedColumns: string[] = ['roomNumber', 'type', 'hotel', 'price', 'status', 'actions'];
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private adminService: AdminService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.loadRooms();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadRooms() {
    this.isLoading = true;
    this.adminService.getRooms().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading rooms', error);
        this.notificationService.show('Failed to load rooms.', 'error');
        this.isLoading = false;
      }
    });
  }

  deleteRoom(id: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Room',
        message: 'Are you sure you want to delete this room? This action cannot be undone.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminService.deleteRoom(id).subscribe({
          next: () => {
            this.dataSource.data = this.dataSource.data.filter(r => r.id !== id);
            this.notificationService.show('Room deleted successfully.', 'success');
          },
          error: (error) => {
            console.error('Error deleting room', error);
            this.notificationService.show('Failed to delete room.', 'error');
          }
        });
      }
    });
  }
}
