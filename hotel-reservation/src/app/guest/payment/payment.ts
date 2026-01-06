import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="payment-container animate-fade-in">
      <h3 class="mb-4">Complete Payment</h3>
      
      <div class="card bg-light mb-4">
        <div class="card-body">
            <h5 class="card-title">Booking Summary</h5>
            <div class="d-flex justify-content-between">
                <span>Room Charges</span>
                <span>₹{{ amount | number:'1.2-2' }}</span>
            </div>
            <div class="d-flex justify-content-between">
                <span>Taxes (10%)</span>
                <span>₹{{ amount * 0.1 | number:'1.2-2' }}</span>
            </div>
            <hr>
            <div class="d-flex justify-content-between fw-bold">
                <span>Total</span>
                <span>₹{{ amount * 1.1 | number:'1.2-2' }}</span>
            </div>
        </div>
      </div>

      <div class="text-center mb-4 text-muted">
        <p>Click the button below to simulate a secure payment transaction.</p>
      </div>

      <button type="button" class="btn btn-success w-100 py-3 fw-bold" (click)="processPayment()" [disabled]="isProcessing">
        <span *ngIf="isProcessing" class="spinner-border spinner-border-sm me-2"></span>
        {{ isProcessing ? 'Processing...' : 'Pay & Confirm Booking' }}
      </button>
      <button type="button" class="btn btn-link text-muted w-100 mt-2" (click)="cancel.emit()">Cancel</button>
    </div>
  `,
  styles: [`
    .payment-container {
        max-width: 500px;
        margin: 0 auto;
        padding: 2rem;
        background: white;
        border-radius: var(--radius-lg);
        box-shadow: 0 10px 25px rgba(0,0,0,0.1);
    }
  `]
})
export class PaymentComponent {
  @Input() amount: number = 0;
  @Output() paymentComplete = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  isProcessing = false;

  processPayment() {
    this.isProcessing = true;
    // Simulate payment delay
    setTimeout(() => {
      this.isProcessing = false;
      this.paymentComplete.emit();
    }, 1500);
  }
}
