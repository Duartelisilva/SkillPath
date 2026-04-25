import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from './toast.service';

export interface AppError {
  message: string;
  code?: string;
  details?: string;
}

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  private toast = inject(ToastService);

  /**
   * Handle HTTP errors and show user-friendly messages
   */
  handleHttpError(error: HttpErrorResponse, context?: string): AppError {
    let message = 'An unexpected error occurred';
    let details = '';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      message = 'Network error. Please check your connection.';
      details = error.error.message;
    } else {
      // Server-side error
      switch (error.status) {
        case 0:
          message = 'Cannot connect to server. Please check your connection.';
          break;
        case 400:
          message = error.error?.title || 'Invalid request. Please check your input.';
          details = error.error?.detail || '';
          break;
        case 401:
          message = 'You are not authorized. Please log in.';
          break;
        case 403:
          message = 'Access denied.';
          break;
        case 404:
          message = context ? `${context} not found.` : 'Resource not found.';
          break;
        case 409:
          message = 'This operation conflicts with existing data.';
          details = error.error?.detail || '';
          break;
        case 422:
          message = 'Validation failed. Please check your input.';
          details = error.error?.detail || '';
          break;
        case 500:
        case 502:
        case 503:
          message = 'Server error. Please try again later.';
          break;
        case 504:
          message = 'Request timeout. The server took too long to respond.';
          break;
        default:
          message = `Error: ${error.statusText || 'Unknown error'}`;
      }
    }

    const appError: AppError = {
      message,
      code: error.status?.toString(),
      details
    };

    // Show toast notification
    this.toast.error(message);

    // Log to console in development
    if (!this.isProduction()) {
      console.error('HTTP Error:', {
        status: error.status,
        message: error.message,
        url: error.url,
        error: error.error
      });
    }

    return appError;
  }

  /**
   * Handle domain/business logic errors
   */
  handleDomainError(error: Error, context?: string): AppError {
    const message = context 
      ? `${context}: ${error.message}`
      : error.message;

    this.toast.error(message);

    if (!this.isProduction()) {
      console.error('Domain Error:', error);
    }

    return {
      message,
      details: error.stack
    };
  }

  /**
   * Handle AI/generation specific errors
   */
  handleGenerationError(error: any, goalTitle: string): AppError {
    let message = `Failed to generate skill tree for "${goalTitle}".`;
    
    if (error.status === 0) {
      message += ' Cannot connect to AI service.';
    } else if (error.status === 504) {
      message += ' AI generation timed out. Please try again.';
    } else if (error.error?.detail) {
      message += ` ${error.error.detail}`;
    }

    this.toast.error(message, 8000); // Longer duration for generation errors

    return {
      message,
      code: error.status?.toString(),
      details: error.error?.detail
    };
  }

  /**
   * Show success message
   */
  success(message: string) {
    this.toast.success(message);
  }

  /**
   * Show warning message
   */
  warning(message: string) {
    this.toast.warning(message);
  }

  /**
   * Show info message
   */
  info(message: string) {
    this.toast.info(message);
  }

  private isProduction(): boolean {
    // You'll set this in environment config
    return false; // Change to environment.production
  }
}