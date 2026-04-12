import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ProgressVariant = 'default' | 'xp' | 'success' | 'warning' | 'danger';
export type ProgressSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="progress-container" [class]="containerClasses">
      <div class="progress-label" *ngIf="showLabel">
        <span class="label-text">{{ label }}</span>
        <span class="label-value" *ngIf="showValue">
          {{ current }} / {{ max }}
          <span class="label-unit" *ngIf="unit"> {{ unit }}</span>
        </span>
      </div>
      
      <div class="progress-bar" [class]="barClasses">
        <div
          class="progress-fill"
          [style.width.%]="percentage"
          [class.animated]="animated"
        >
          <span class="progress-glow" *ngIf="showGlow"></span>
        </div>
      </div>
      
      <div class="progress-percentage" *ngIf="showPercentage">
        {{ percentage.toFixed(0) }}%
      </div>
    </div>
  `,
  styleUrls: ['./progress-bar.scss']
})
export class ProgressBarComponent {
  @Input() current = 0;
  @Input() max = 100;
  @Input() label = '';
  @Input() unit = '';
  @Input() variant: ProgressVariant = 'default';
  @Input() size: ProgressSize = 'md';
  @Input() showLabel = true;
  @Input() showValue = true;
  @Input() showPercentage = false;
  @Input() showGlow = true;
  @Input() animated = true;

  get percentage(): number {
    if (this.max === 0) return 0;
    return Math.min(100, (this.current / this.max) * 100);
  }

  get containerClasses(): string {
    return ['progress-container', `progress-${this.size}`].join(' ');
  }

  get barClasses(): string {
    return ['progress-bar', `progress-bar-${this.variant}`].join(' ');
  }
}
