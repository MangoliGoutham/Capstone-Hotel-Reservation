import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { GuestService } from '../services/guest.service';
import { Reservation } from '../../shared/models/reservation.models';
import { BillService, Bill } from '../services/bill.service';
import { PaymentComponent } from '../payment/payment';
import { NotificationService } from '../../shared/services/notification.service';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
    selector: 'app-my-bookings',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        PaymentComponent,
        MatDialogModule,
        ConfirmDialogComponent,
        ReactiveFormsModule,
        MatPaginatorModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatIconModule,
        MatButtonModule
    ],
    templateUrl: './my-bookings.html',
    styleUrl: './my-bookings.css'
})
export class MyBookingsComponent implements OnInit {
    reservations: Reservation[] = [];
    filteredReservations: Reservation[] = [];
    pagedReservations: Reservation[] = [];
    bills: { [reservationId: number]: Bill } = {};
    isLoading = true;
    isCancelling: number | null = null;

    // Search Form
    searchForm: FormGroup;


    // Pagination
    pageSize = 5;
    pageSizeOptions = [5, 10, 25];
    pageIndex = 0;

    // Payment Modal State
    showPaymentModal = false;
    selectedBill: Bill | null = null;

    billErrorMessages: { [reservationId: number]: string } = {};
    billErrors: { [reservationId: number]: boolean } = {};

    constructor(
        private guestService: GuestService,
        private billService: BillService,
        private notificationService: NotificationService,
        private dialog: MatDialog,
        private fb: FormBuilder
    ) {
        this.searchForm = this.fb.group({
            searchTerm: ['']
        });
    }

    ngOnInit() {
        this.loadReservations();

        // Listen to form changes
        this.searchForm.valueChanges.subscribe(() => {
            this.pageIndex = 0; // Reset to first page on filter change
            this.filterReservations();
        });
    }

    loadReservations() {
        this.isLoading = true;
        this.guestService.getMyBookings().subscribe({
            next: (data) => {
                this.reservations = data;
                this.isLoading = false;
                this.filterReservations(); // Initial filter & pagination
                this.loadBills();
            },
            error: (err) => {
                console.error(err);
                this.isLoading = false;
            }
        });
    }

    filterReservations() {
        const { searchTerm } = this.searchForm.value;
        const term = (searchTerm || '').toLowerCase();

        this.filteredReservations = this.reservations.filter(res => {
            const matchesSearch = res.hotelName.toLowerCase().includes(term) ||
                res.reservationNumber.toString().includes(term);
            return matchesSearch;
        });

        // Always sort by CheckIn date descending by default
        this.filteredReservations.sort((a, b) => new Date(b.checkInDate).getTime() - new Date(a.checkInDate).getTime());

        this.updatePagedReservations();
    }

    updatePagedReservations() {
        const startIndex = this.pageIndex * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        this.pagedReservations = this.filteredReservations.slice(startIndex, endIndex);
    }

    onPageChange(event: PageEvent) {
        this.pageIndex = event.pageIndex;
        this.pageSize = event.pageSize;
        this.updatePagedReservations();
    }

    loadBills() {
        // Fetch bills for any checked-out reservations
        this.reservations.forEach(res => {
            if (res.status === 'Checked-out' || res.status === 'CheckedOut') {
                this.billService.getBillByReservation(res.id).subscribe({
                    next: (bill) => {
                        this.bills[res.id] = bill;
                        this.billErrors[res.id] = false;
                        this.billErrorMessages[res.id] = '';
                    },
                    error: (err) => {
                        console.error('Error fetching bill', err);
                        this.billErrors[res.id] = true;
                        const detail = err.error?.message || err.message || JSON.stringify(err);
                        this.billErrorMessages[res.id] = `Error: ${detail}`;
                    }
                });
            }
        });
    }

    canCancel(res: Reservation): boolean {
        return res.status === 'Pending' || res.status === 'Confirmed';
    }

    cancelReservation(id: number) {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            data: {
                title: 'Cancel Reservation',
                message: 'Are you sure you want to cancel this reservation? This cannot be undone.'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.isCancelling = id;
                this.guestService.cancelReservation(id).subscribe({
                    next: () => {
                        this.notificationService.show('Reservation cancelled successfully.', 'success');
                        this.isCancelling = null;
                        this.loadReservations(); // Refresh list to see Cancelled status
                    },
                    error: (err) => {
                        console.error(err);
                        this.isCancelling = null;
                        this.notificationService.show('Failed to cancel reservation.', 'error');
                    }
                });
            }
        });
    }

    downloadInvoice(res: Reservation) {
        this.notificationService.show('Generating invoice...', 'info');

        this.billService.getBillByReservation(res.id).subscribe({
            next: (bill) => {
                const doc = new jsPDF();

                // Branding
                doc.setFontSize(22);
                doc.setTextColor(79, 70, 229); // Primary Color
                doc.text('Hotel Reservation System', 14, 20);

                doc.setFontSize(12);
                doc.setTextColor(100);
                doc.text('123 Luxury Avenue, Metropolis', 14, 28);
                doc.text('support@hotelreservation.com', 14, 34);

                // Invoice Title & Info
                doc.setFontSize(18);
                doc.setTextColor(0);
                doc.text('INVOICE', 140, 20);

                doc.setFontSize(10);
                doc.text(`Invoice #: ${bill.billNumber || 'INV-' + bill.id}`, 140, 28);
                doc.text(`Date: ${new Date().toLocaleDateString()}`, 140, 34);
                doc.text(`Status: ${bill.paymentStatus}`, 140, 40);

                // Guest Details
                doc.line(14, 45, 196, 45); // Horizontal Line
                doc.setFontSize(12);
                doc.text('Bill To:', 14, 55);
                doc.setFontSize(10);
                doc.text(`Guest: ${res.userName}`, 14, 62);
                doc.text(`Reservation #: ${res.reservationNumber}`, 14, 68);
                doc.text(`Hotel: ${res.hotelName}`, 14, 74);
                doc.text(`Stay: ${new Date(res.checkInDate).toLocaleDateString()} - ${new Date(res.checkOutDate).toLocaleDateString()}`, 14, 80);

                // Table Items
                const tableBody = [
                    ['Room Charges', `Accommodation (${this.getNightCount(res)} Nights)`, `₹${bill.roomCharges.toFixed(2)}`],
                    ['Taxes', '10% GST', `₹${bill.taxAmount.toFixed(2)}`],
                    ['Additional Charges', '-', `₹${bill.additionalCharges.toFixed(2)}`]
                ];

                autoTable(doc, {
                    startY: 90,
                    head: [['Description', 'Details', 'Amount']],
                    body: tableBody,
                    theme: 'grid',
                    headStyles: { fillColor: [79, 70, 229] },
                    foot: [['', 'Total', `₹${bill.totalAmount.toFixed(2)}`]],
                    footStyles: { fillColor: [241, 245, 249], textColor: [0, 0, 0], fontStyle: 'bold' }
                });

                // Footer
                const finalY = (doc as any).lastAutoTable.finalY + 20;
                doc.setFontSize(10);
                doc.setTextColor(150);
                doc.text('Thank you for choosing Hotel Reservation System!', 105, finalY, { align: 'center' });

                // Download
                doc.save(`Invoice_${res.reservationNumber}.pdf`);
                this.notificationService.show('Invoice downloaded successfully', 'success');
            },
            error: (err) => {
                console.error(err);
                this.notificationService.show('Could not fetch invoice details.', 'error');
            }
        });
    }

    private getNightCount(res: Reservation): number {
        const start = new Date(res.checkInDate);
        const end = new Date(res.checkOutDate);
        const diff = Math.abs(end.getTime() - start.getTime());
        return Math.ceil(diff / (1000 * 60 * 60 * 24)) || 1;
    }

    // Payment Logic integrated from previous file
    initiatePayment(bill: Bill) {
        this.selectedBill = bill;
        this.showPaymentModal = true;
    }

    processPayment() {
        if (!this.selectedBill) return;

        this.billService.payBill(this.selectedBill.id).subscribe({
            next: (updatedBill) => {
                this.bills[this.selectedBill!.reservationId] = updatedBill;
                this.showPaymentModal = false;
                this.selectedBill = null;
                this.notificationService.show('Bill paid successfully!', 'success');
                this.loadReservations();
            },
            error: (err) => {
                console.error(err);
                this.notificationService.show('Payment failed. Please try again.', 'error');
                this.showPaymentModal = false;
            }
        });
    }
}
