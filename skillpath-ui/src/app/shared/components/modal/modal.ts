import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="modal-overlay"
      [class.modal-overlay-visible]="isOpen"
      (click)="handleOverlayClick($event)"
      *ngIf="isOpen"
    >
      <div
        class="modal"
        [class]="modalClasses"
        (click)="$event.stopPropagation()"
        role="dialog"
        [attr.aria-modal]="true"
        [attr.aria-labelledby]="ariaLabelledBy"
      >
        <div class="modal-header" *ngIf="showHeader">
          <div class="modal-title-wrapper">
            <ng-content select="[modal-header]" />
          </div>
          <button
            class="modal-close"
            (click)="close()"
            *ngIf="showCloseButton"
            aria-label="Close modal"
          >
            ✕
          </button>
        </div>

        <div class="modal-body">
          <ng-content />
        </div>

        <div class="modal-footer" *ngIf="hasFooter">
          <ng-content select="[modal-footer]" />
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./modal.scss']
})
export class ModalComponent {
  @Input() isOpen = false;
  @Input() size: ModalSize = 'md';
  @Input() showHeader = true;
  @Input() showCloseButton = true;
  @Input() hasFooter = false;
  @Input() closeOnOverlayClick = true;
  @Input() closeOnEscape = true;
  @Input() ariaLabelledBy = '';
  
  @Output() closed = new EventEmitter<void>();

  get modalClasses(): string {
    return ['modal', `modal-${this.size}`].join(' ');
  }

  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: Event): void {
    if (this.isOpen && this.closeOnEscape) {
      event.preventDefault();
      this.close();
    }
  }

  handleOverlayClick(event: MouseEvent): void {
    if (this.closeOnOverlayClick) {
      this.close();
    }
  }

  close(): void {
    this.closed.emit();
  }
}
