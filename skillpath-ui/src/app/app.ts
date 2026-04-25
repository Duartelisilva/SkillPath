import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastService } from './services/toast.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  template: `
    <router-outlet />
    
    <!-- Global Toast Container -->
    <div class="toast-container">
      @for (toast of toastService.toasts$(); track toast.id) {
        <div class="toast" [class]="toast.type">
          <div class="toast-icon">
            @switch (toast.type) {
              @case ('success') { <span>✓</span> }
              @case ('error') { <span>✕</span> }
              @case ('warning') { <span>⚠</span> }
              @case ('info') { <span>ℹ</span> }
            }
          </div>
          <p class="toast-message">{{ toast.message }}</p>
          <button class="toast-close" (click)="toastService.remove(toast.id)">✕</button>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
    }

    .toast-container {
      position: fixed;
      top: 1rem;
      right: 1rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-width: 400px;
    }

    .toast {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 1rem;
      background: var(--bg-card);
      border: 1px solid;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
      animation: slideIn 0.3s ease;

      &.success {
        border-color: var(--completed);
        .toast-icon { color: var(--completed); }
      }
      &.error {
        border-color: var(--accent-red);
        .toast-icon { color: var(--accent-red); }
      }
      &.warning {
        border-color: var(--accent-yellow);
        .toast-icon { color: var(--accent-yellow); }
      }
      &.info {
        border-color: var(--accent-blue);
        .toast-icon { color: var(--accent-blue); }
      }
    }

    .toast-icon {
      font-size: 1.25rem;
      font-weight: bold;
      flex-shrink: 0;
    }

    .toast-message {
      flex: 1;
      color: var(--text-primary);
      font-size: 0.9rem;
      line-height: 1.4;
      margin: 0;
    }

    .toast-close {
      background: none;
      border: none;
      color: var(--text-muted);
      font-size: 1.25rem;
      cursor: pointer;
      padding: 0;
      line-height: 1;
      transition: color 0.2s;

      &:hover { color: var(--text-primary); }
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @media (max-width: 640px) {
      .toast-container {
        left: 1rem;
        right: 1rem;
        max-width: none;
      }
    }
  `]
})
export class App {
  toastService = inject(ToastService);
}