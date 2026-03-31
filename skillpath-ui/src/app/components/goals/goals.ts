import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Goal } from '../../models/goal.model';

@Component({
  selector: 'app-goals',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="goals-page">
      <header class="page-header">
        <div class="header-content">
          <div class="logo">
            <span class="logo-icon">⚡</span>
            <h1>SkillPath</h1>
          </div>
          <p class="subtitle">Your AI-powered learning journey</p>
        </div>
        <button class="btn btn-primary" (click)="showModal = true">
          + New Goal
        </button>
      </header>

      <main class="goals-grid" *ngIf="!loading()">
        <div class="goal-card" *ngFor="let goal of goals()" (click)="openGoal(goal)">
          <div class="card-header">
            <span class="status-badge {{ goal.status }}">{{ goal.status }}</span>
            <span class="skill-count">{{ getSkillCount(goal) }} skills</span>
          </div>
          <h2 class="card-title">{{ goal.title }}</h2>
          <p class="card-desc">{{ goal.description }}</p>
          <div class="card-footer">
            <button class="btn btn-primary generate-btn"
              (click)="generateTree($event, goal)"
              [disabled]="generating() === goal.id">
              {{ generating() === goal.id ? '⚙ Generating...' : '🤖 Generate Skill Tree' }}
            </button>
          </div>
        </div>

        <div class="goal-card empty-card" *ngIf="goals().length === 0">
          <span class="empty-icon">🎯</span>
          <p>No goals yet. Create your first learning goal!</p>
        </div>
      </main>

      <div class="loading" *ngIf="loading()">
        <div class="spinner"></div>
        <p>Loading goals...</p>
      </div>

      <!-- Create Goal Modal -->
      <div class="modal-overlay" *ngIf="showModal" (click)="showModal = false">
        <div class="modal" (click)="$event.stopPropagation()">
          <h2>Create New Goal</h2>
          <div class="form-group">
            <label>Title</label>
            <input [(ngModel)]="newTitle" placeholder="e.g. Learn C#" maxlength="200" />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea [(ngModel)]="newDescription" placeholder="What do you want to achieve?" rows="4" maxlength="2000"></textarea>
          </div>
          <div class="modal-actions">
            <button class="btn btn-secondary" (click)="showModal = false">Cancel</button>
            <button class="btn btn-primary" (click)="createGoal()" [disabled]="!newTitle || !newDescription">
              Create Goal
            </button>
          </div>
        </div>
      </div>

      <!-- Generation Progress Modal -->
      <div class="modal-overlay" *ngIf="generating()" (click)="$event.stopPropagation()">
        <div class="modal progress-modal" (click)="$event.stopPropagation()">
          <div class="progress-content">
            <div class="progress-spinner">
              <div class="spinner-large"></div>
            </div>
            <h2>{{ progressTitle() }}</h2>
            <p class="progress-message">{{ progressMessage() }}</p>
            <div class="progress-bar">
              <div class="progress-fill" [style.width.%]="progressPercent()"></div>
            </div>
            <p class="progress-hint">This may take 1-2 minutes. Please wait...</p>
          </div>
        </div>
      </div>

      <!-- Error Modal -->
      <div class="modal-overlay" *ngIf="showErrorModal()" (click)="showErrorModal.set(false)">
        <div class="modal error-modal" (click)="$event.stopPropagation()">
          <div class="error-icon">⚠️</div>
          <h2>Generation Failed</h2>
          <p class="error-message">{{ errorMessage() }}</p>
          <div class="modal-actions">
            <button class="btn btn-secondary" (click)="showErrorModal.set(false)">Cancel</button>
            <button class="btn btn-primary" (click)="retryGeneration()">
              🔄 Try Again
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .goals-page {
      min-height: 100vh;
      background: var(--bg-primary);
      padding: 2rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 3rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid var(--border-accent);
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.25rem;
    }

    .logo-icon {
      font-size: 2rem;
      filter: drop-shadow(0 0 10px rgba(124, 58, 237, 0.8));
    }

    .logo h1 {
      font-size: 2.5rem;
      background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.9rem;
      margin-top: 0.25rem;
    }

    .goals-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.5rem;
    }

    .goal-card {
      background: var(--bg-card);
      border: 1px solid var(--border-accent);
      border-radius: 16px;
      padding: 1.5rem;
      cursor: pointer;
      transition: all 0.3s ease;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;

      &:hover {
        border-color: var(--accent-purple);
        transform: translateY(-4px);
        box-shadow: 0 8px 30px rgba(124, 58, 237, 0.2);
      }
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .skill-count {
      color: var(--text-muted);
      font-size: 0.8rem;
    }

    .card-title {
      font-size: 1.5rem;
      color: var(--text-primary);
    }

    .card-desc {
      color: var(--text-secondary);
      font-size: 0.9rem;
      line-height: 1.6;
      flex: 1;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .card-footer {
      margin-top: 0.5rem;
    }

    .generate-btn {
      width: 100%;
      justify-content: center;
    }

    .empty-card {
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 200px;
      border-style: dashed;
      cursor: default;

      &:hover {
        transform: none;
        box-shadow: none;
      }

      .empty-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
      }

      p {
        color: var(--text-muted);
      }
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 50vh;
      gap: 1rem;
      color: var(--text-secondary);
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid var(--border-accent);
      border-top-color: var(--accent-purple);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.7);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 100;
      backdrop-filter: blur(4px);
    }

    .modal {
      background: var(--bg-card);
      border: 1px solid var(--border-accent);
      border-radius: 16px;
      padding: 2rem;
      width: 100%;
      max-width: 480px;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;

      h2 {
        font-size: 1.75rem;
        background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        margin: 0;
      }
    }

    .progress-modal, .error-modal {
      max-width: 520px;
    }

    .progress-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.5rem;
    }

    .progress-spinner {
      margin-bottom: 0.5rem;
    }

    .spinner-large {
      width: 64px;
      height: 64px;
      border: 4px solid var(--border-accent);
      border-top-color: var(--accent-purple);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .progress-message {
      color: var(--text-primary);
      font-size: 1.1rem;
      text-align: center;
      margin: 0;
    }

    .progress-bar {
      width: 100%;
      height: 8px;
      background: var(--bg-secondary);
      border-radius: 4px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      background: linear-gradient(90deg, var(--accent-purple), var(--accent-cyan));
      transition: width 0.5s ease;
      border-radius: 4px;
    }

    .progress-hint {
      color: var(--text-muted);
      font-size: 0.85rem;
      text-align: center;
      margin: 0;
    }

    .error-modal {
      text-align: center;
    }

    .error-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .error-message {
      color: var(--text-secondary);
      font-size: 1rem;
      line-height: 1.6;
      margin: 0;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;

      label {
        color: var(--text-secondary);
        font-size: 0.9rem;
        font-weight: 500;
      }

      input, textarea {
        background: var(--bg-secondary);
        border: 1px solid var(--border-accent);
        border-radius: 8px;
        padding: 0.75rem 1rem;
        color: var(--text-primary);
        font-family: 'Inter', sans-serif;
        font-size: 0.95rem;
        outline: none;
        resize: vertical;
        transition: border-color 0.2s;

        &:focus {
          border-color: var(--accent-purple);
        }

        &::placeholder {
          color: var(--text-muted);
        }
      }
    }

    .modal-actions {
      display: flex;
      gap: 1rem;
      justify-content: flex-end;
    }
  `]
})
export class GoalsComponent implements OnInit, OnDestroy {
  goals = signal<Goal[]>([]);
  loading = signal(true);
  generating = signal<string | null>(null);
  progressTitle = signal('Generating Skill Tree');
  progressMessage = signal('Initializing AI model...');
  progressPercent = signal(0);
  showModal = false;
  showErrorModal = signal(false);
  errorMessage = signal('');
  newTitle = '';
  newDescription = '';

  private api = inject(ApiService);
  private router = inject(Router);
  private progressInterval: any;
  private currentGoalId: string | null = null;

  ngOnInit() {
    this.loadGoals();
  }

  ngOnDestroy() {
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
    }
  }

  loadGoals() {
    this.api.getGoals().subscribe({
      next: goals => {
        this.goals.set(goals);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getSkillCount(goal: Goal): number {
    return goal.skillCount ?? 0;
  }

  openGoal(goal: Goal) {
    this.router.navigate(['/goals', goal.id, 'skill-tree']);
  }

  createGoal() {
    if (!this.newTitle || !this.newDescription) return;

    this.api.createGoal(this.newTitle, this.newDescription).subscribe({
      next: goal => {
        this.goals.update(goals => [...goals, goal]);
        this.showModal = false;
        this.newTitle = '';
        this.newDescription = '';
      }
    });
  }

  generateTree(event: Event, goal: Goal) {
    event.stopPropagation();
    this.currentGoalId = goal.id;
    this.generating.set(goal.id);
    this.progressTitle.set('Generating Skill Tree');
    this.startProgressSimulation();

    this.api.generateSkillTree(goal.id).subscribe({
      next: () => {
        this.stopProgressSimulation();
        this.generating.set(null);
        this.currentGoalId = null;
        this.router.navigate(['/goals', goal.id, 'skill-tree']);
      },
      error: (err) => {
        this.stopProgressSimulation();
        this.generating.set(null);
        this.handleGenerationError(err);
      }
    });
  }

  retryGeneration() {
    this.showErrorModal.set(false);
    if (this.currentGoalId) {
      const goal = this.goals().find(g => g.id === this.currentGoalId);
      if (goal) {
        // Simulate a click event
        this.generateTree(new Event('click'), goal);
      }
    }
  }

  private handleGenerationError(error: any) {
    console.error('Generation error:', error);
    
    let message = 'The AI failed to generate a valid skill tree. This can happen when:';
    message += '\n\n• The AI response was malformed';
    message += '\n• The connection to Ollama was interrupted';
    message += '\n• The model generated invalid JSON';
    message += '\n\nWould you like to try again?';

    this.errorMessage.set(message);
    this.showErrorModal.set(true);
  }

  private startProgressSimulation() {
    const messages = [
      'Initializing AI model...',
      'Analyzing your learning goal...',
      'Generating skill progression...',
      'Creating learning tasks...',
      'Building skill dependencies...',
      'Finalizing skill tree...'
    ];

    let messageIndex = 0;
    let currentProgress = 0;

    this.progressMessage.set(messages[0]);
    this.progressPercent.set(0);

    this.progressInterval = setInterval(() => {
      const increment = currentProgress < 50 ? 3 : currentProgress < 80 ? 1.5 : 0.5;
      currentProgress = Math.min(95, currentProgress + increment);
      this.progressPercent.set(currentProgress);

      const newMessageIndex = Math.floor((currentProgress / 95) * messages.length);
      if (newMessageIndex !== messageIndex && newMessageIndex < messages.length) {
        messageIndex = newMessageIndex;
        this.progressMessage.set(messages[messageIndex]);
      }
    }, 800);
  }

  private stopProgressSimulation() {
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
    }
    this.progressPercent.set(100);
    this.progressMessage.set('Complete!');
  }
}