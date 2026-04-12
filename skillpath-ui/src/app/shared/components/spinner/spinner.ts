import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SpinnerSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
export type SpinnerVariant = 'default' | 'primary' | 'success' | 'warning' | 'danger';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="spinner-container" [class.spinner-inline]="inline">
      <div
        class="spinner"
        [class]="spinnerClasses"
        role="status"
        [attr.aria-label]="label"
      >
        <span class="spinner-sr-only">{{ label }}</span>
      </div>
      <p class="spinner-text" *ngIf="text">{{ text }}</p>
    </div>
  `,
  styleUrls: ['./spinner.scss']
})
export class SpinnerComponent {
  @Input() size: SpinnerSize = 'md';
  @Input() variant: SpinnerVariant = 'default';
  @Input() text = '';
  @Input() label = 'Loading...';
  @Input() inline = false;

  get spinnerClasses(): string {
    return ['spinner', `spinner-${this.size}`, `spinner-${this.variant}`].join(' ');
  }
}
