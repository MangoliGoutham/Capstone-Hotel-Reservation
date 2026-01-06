import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminService } from '../../services/admin.service';
import { CreateRoomDto } from '../../../shared/models/room.models';
import { Hotel } from '../../../shared/models/hotel.models';

@Component({
  selector: 'app-room-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './room-form.html',
  styleUrl: './room-form.css'
})
export class RoomFormComponent implements OnInit {
  room: CreateRoomDto = {
    roomNumber: '',
    roomType: '',
    basePrice: 0,
    capacity: 2,
    description: '',
    hotelId: 0
  };
  hotels: Hotel[] = [];
  isEditMode = false;
  isLoading = false;
  roomId?: number;

  roomTypes = ['Standard', 'Deluxe', 'Suite', 'Double', 'Family'];

  constructor(
    private adminService: AdminService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.loadHotels();
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.roomId = +id;
      this.loadRoom(this.roomId);
    }
  }

  loadHotels() {
    this.adminService.getHotels().subscribe(data => {
      this.hotels = data;
    });
  }

  loadRoom(id: number) {
    this.adminService.getRoom(id).subscribe(data => {
      
      this.room = {
        roomNumber: data.roomNumber,
        roomType: data.roomType,
        basePrice: data.basePrice,
        capacity: data.capacity,
        description: data.description,
        hotelId: data.hotelId
      };
    });
  }

  onSubmit() {
    this.isLoading = true;
    if (this.isEditMode && this.roomId) {
      this.adminService.updateRoom(this.roomId, this.room).subscribe({
        next: () => this.router.navigate(['/admin/rooms']),
        error: (err) => {
          console.error(err);
          this.isLoading = false;
        }
      });
    } else {
      this.adminService.createRoom(this.room).subscribe({
        next: () => this.router.navigate(['/admin/rooms']),
        error: (err) => {
          console.error(err);
          this.isLoading = false;
        }
      });
    }
  }
}
