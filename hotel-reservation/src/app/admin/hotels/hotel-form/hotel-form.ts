import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminService } from '../../services/admin.service';
import { CreateHotelDto } from '../../../shared/models/hotel.models';

@Component({
  selector: 'app-hotel-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './hotel-form.html',
  styleUrl: './hotel-form.css'
})
export class HotelFormComponent implements OnInit {
  hotel: CreateHotelDto = {
    name: '',
    address: '',
    city: '',
    country: '',
    starRating: 3,
    phoneNumber: '',
    email: '',
    description: ''
  };
  isEditMode = false;
  isLoading = false;
  hotelId?: number;

  constructor(
    private adminService: AdminService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.hotelId = +id;
      this.loadHotel(this.hotelId);
    }
  }

  loadHotel(id: number) {
    this.adminService.getHotel(id).subscribe(data => {
      this.hotel = data;
    });
  }

  onSubmit() {
    this.isLoading = true;
    if (this.isEditMode && this.hotelId) {
      this.adminService.updateHotel(this.hotelId, this.hotel).subscribe({
        next: () => this.router.navigate(['/admin/hotels']),
        error: (err) => {
          console.error(err);
          this.isLoading = false;
        }
      });
    } else {
      this.adminService.createHotel(this.hotel).subscribe({
        next: () => this.router.navigate(['/admin/hotels']),
        error: (err) => {
          console.error(err);
          this.isLoading = false;
        }
      });
    }
  }
}
