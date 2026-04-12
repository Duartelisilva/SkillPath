import { Component, Input, Output, EventEmitter, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardVariant = 'default' | 'outlined' | 'elevated';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="card"
      [class.card-clickable]="clickable"
      [class.card-outlined]="variant === 'outlined'"
      [class.card-elevated]="variant === 'elevated'"
      (click)="handleClick($event)"
    >
      <div class="card-header" *ngIf="hasHeader">
        <ng-content select="[card-header]" />
      </div>
      
      <div class="card-body">
        <ng-content />
      </div>
      
      <div class="card-footer" *ngIf="hasFooter">
        <ng-content select="[card-footer]" />
      </div>
    </div>
  `,
  styleUrls: ['./card.scss']
})
export class CardComponent {
  @Input() variant: CardVariant = 'default';
  @Input() clickable = false;
  @Input() hasHeader = false;
  @Input() hasFooter = false;
  
  @Output() cardClicked = new EventEmitter<Event>();

  handleClick(event: Event): void {
    if (this.clickable) {
      this.cardClicked.emit(event);
    }
  }
}
