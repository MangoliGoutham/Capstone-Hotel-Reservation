import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
    selector: 'app-admin-reports',
    standalone: true,
    imports: [CommonModule, FormsModule, SharedModule],
    templateUrl: './reports.html',
    styleUrl: './reports.css'
})
export class AdminReportsComponent implements OnInit {
    occupancyDate = new Date();
    startDate = new Date(new Date().setDate(new Date().getDate() - 30));
    endDate = new Date();

    occupancyData: any;
    revenueData: any;

    constructor(private reportService: ReportService) { }

    ngOnInit() {
        this.loadOccupancy();
        this.loadRevenue();
    }

    loadOccupancy() {
        
        const dateStr = this.occupancyDate.toISOString().split('T')[0];

        this.reportService.getOccupancy(dateStr).subscribe(data => {
           
            const list = data as any[];
            if (list && list.length > 0) {
                const totalRooms = list.reduce((sum, item) => sum + item.totalRooms, 0);
                const occupiedRooms = list.reduce((sum, item) => sum + item.occupiedRooms, 0);
                const occupancyRate = totalRooms > 0 ? occupiedRooms / totalRooms : 0;

                this.occupancyData = {
                    totalRooms,
                    occupiedRooms,
                    occupancyRate,
                    details: list 
                };
            } else {
                this.occupancyData = { totalRooms: 0, occupiedRooms: 0, occupancyRate: 0 };
            }
        });
    }

    loadRevenue() {
        const startStr = this.startDate.toISOString().split('T')[0];
        const endStr = this.endDate.toISOString().split('T')[0];

        this.reportService.getRevenue(startStr, endStr).subscribe(data => {
           
            const list = data as any[];
            if (list && list.length > 0) {
                const totalRevenue = list.reduce((sum, item) => sum + item.totalRevenue, 0);
                

                this.revenueData = {
                    totalRevenue,
                    roomRevenue: totalRevenue, 
                    otherRevenue: 0,
                    details: list
                };
            } else {
                this.revenueData = { totalRevenue: 0, roomRevenue: 0, otherRevenue: 0 };
            }
        });
    }
}
