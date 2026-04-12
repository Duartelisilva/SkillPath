import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type BadgeVariant = 'draft' | 'active' | 'completed' | 'archived' | 
  'locked' | 'available' | 'in-progress' | 'info' | 'warning' | 'success' | 'error';
export type BadgeSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="badge"
      [class]="badgeClasses"
    >
      <span class="badge-dot" *ngIf="showDot"></span>
      <ng-content />
    </span>
  `,
  styleUrls: ['./badge.scss']
})
export class BadgeComponent {
  @Input() variant: BadgeVariant = 'info';
  @Input() size: BadgeSize = 'md';
  @Input() showDot = false;

  get badgeClasses(): string {
    return ['badge', `badge-${this.variant}`, `badge-${this.size}`].join(' ');
  }
}
