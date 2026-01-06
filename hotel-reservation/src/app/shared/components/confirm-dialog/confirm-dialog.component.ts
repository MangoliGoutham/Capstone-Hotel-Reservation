import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <div class="dialog-header">
        <h2 mat-dialog-title class="m-0">{{ data.title }}</h2>
    </div>
    <mat-dialog-content class="dialog-body">
      <p class="dialog-message">{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end" class="dialog-footer">
      <button mat-button class="btn-cancel" (click)="onNoClick()">Cancel</button>
      <button mat-raised-button color="primary" class="btn-confirm" [mat-dialog-close]="true">Confirm</button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
        display: block;
        background: var(--color-bg-surface);
    }
    .dialog-header {
        padding: 1.5rem 1.5rem 0.5rem;
    }
    h2 {
        font-size: 1.5rem !important;
        font-weight: 700;
        color: var(--color-text-main);
        letter-spacing: -0.01em;
    }
    .dialog-body {
        margin: 0;
        padding: 0 1.5rem 1rem !important;
    }
    .dialog-message {
        color: var(--color-text-muted);
        font-size: 1rem;
        line-height: 1.6;
        margin: 0;
    }
    .dialog-footer {
        padding: 1rem 1.5rem 1.5rem !important;
        margin: 0;
    }
    .btn-cancel {
        color: var(--color-text-muted) !important;
    }
    .btn-confirm {
        box-shadow: var(--shadow-md);
        font-weight: 600;
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { title: string; message: string }
  ) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
