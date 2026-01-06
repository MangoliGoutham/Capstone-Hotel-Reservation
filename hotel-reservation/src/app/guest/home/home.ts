import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GuestService } from '../services/guest.service';
import { Hotel } from '../../shared/models/hotel.models';
import { SharedModule } from '../../shared/shared.module';

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, SharedModule],
    templateUrl: './home.html',
    styleUrl: './home.css'
})
export class HomeComponent implements OnInit {
    hotels: Hotel[] = [];
    allHotels: Hotel[] = [];
    isLoading = true;
    searchCity = '';

    constructor(private guestService: GuestService) { }

    ngOnInit() {
        this.loadHotels();
    }

    loadHotels() {
        this.isLoading = true;
        this.guestService.getHotels().subscribe({
            next: (data) => {
                this.allHotels = data;
                this.hotels = data;
                this.isLoading = false;
            },
            error: (err) => {
                console.error(err);
                this.isLoading = false;
            }
        });
    }

    search() {
        if (!this.searchCity.trim()) {
            this.hotels = this.allHotels;
        } else {
            const term = this.searchCity.toLowerCase();
            this.hotels = this.allHotels.filter(h =>
                h.city.toLowerCase().includes(term) ||
                h.name.toLowerCase().includes(term)
            );
        }
    }
}
