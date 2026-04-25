import { Injectable, signal } from '@angular/core';

export interface LoadingState {
  key: string;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private loadingStates = signal<Map<string, string>>(new Map());

  /**
   * Check if a specific key is loading
   */
  isLoading(key: string): boolean {
    return this.loadingStates().has(key);
  }

  /**
   * Get loading message for a key
   */
  getMessage(key: string): string | undefined {
    return this.loadingStates().get(key);
  }

  /**
   * Start loading for a key
   */
  start(key: string, message?: string) {
    this.loadingStates.update(states => {
      const newStates = new Map(states);
      newStates.set(key, message || 'Loading...');
      return newStates;
    });
  }

  /**
   * Stop loading for a key
   */
  stop(key: string) {
    this.loadingStates.update(states => {
      const newStates = new Map(states);
      newStates.delete(key);
      return newStates;
    });
  }

  /**
   * Stop all loading states
   */
  stopAll() {
    this.loadingStates.set(new Map());
  }

  /**
   * Check if anything is loading
   */
  get isAnyLoading(): boolean {
    return this.loadingStates().size > 0;
  }
}