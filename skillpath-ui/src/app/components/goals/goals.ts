import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Goal } from '../../models/goal.model';

// Import new components
import { ButtonComponent } from '../../shared/components/button/button';
import { CardComponent } from '../../shared/components/card/card';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge';
import { ModalComponent } from '../../shared/components/modal/modal';
import { SpinnerComponent } from '../../shared/components/spinner/spinner';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar';

@Component({
  selector: 'app-goals',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonComponent,
    CardComponent,
    BadgeComponent,
    ModalComponent,
    SpinnerComponent,
    ProgressBarComponent
  ],
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
        <app-button
          variant="primary"
          size="lg"
          (clicked)="showCreateModal.set(true)"
        >
          + New Goal
        </app-button>
      </header>

      <!-- Stats Section (NEW!) -->
      <section class="stats-section" *ngIf="!loading()">
        <app-card variant="elevated" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon">🎯</div>
            <div class="stat-details">
              <p class="stat-label">Total Goals</p>
              <p class="stat-value">{{ goals().length }}</p>
            </div>
          </div>
        </app-card>

        <app-card variant="elevated" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon">📚</div>
            <div class="stat-details">
              <p class="stat-label">Active Skills</p>
              <p class="stat-value">{{ getTotalSkills() }}</p>
            </div>
          </div>
        </app-card>

        <app-card variant="elevated" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon">✓</div>
            <div class="stat-details">
              <p class="stat-label">Active Goals</p>
              <p class="stat-value">{{ getActiveGoalsCount() }}</p>
            </div>
          </div>
        </app-card>
      </section>

      <!-- Goals Grid -->
      <main class="goals-grid" *ngIf="!loading()">
        <app-card
          *ngFor="let goal of goals()"
          [clickable]="true"
          [hasHeader]="true"
          [hasFooter]="true"
          (cardClicked)="openGoal(goal)"
          class="goal-card-wrapper"
        >
          <!-- Header -->
          <div card-header class="goal-card-header">
            <app-badge
              [variant]="getStatusBadgeVariant(goal.status)"
              size="sm"
            >
              {{ goal.status }}
            </app-badge>
            <span class="skill-count">{{ getSkillCount(goal) }} skills</span>
          </div>

          <!-- Body -->
          <div class="goal-card-body">
            <h2 class="goal-title">{{ goal.title }}</h2>
            <p class="goal-description">{{ goal.description }}</p>
            
            <!-- Progress indicator (NEW!) -->
            <div class="goal-progress" *ngIf="getSkillCount(goal) > 0">
              <app-progress-bar
                [current]="getSkillCount(goal)"
                [max]="getSkillCount(goal)"
                label="Skills Generated"
                [showValue]="false"
                [showPercentage]="false"
                variant="xp"
                size="sm"
              />
            </div>
          </div>

          <!-- Footer -->
          <div card-footer class="goal-card-footer">
            <app-button
              variant="primary"
              size="sm"
              [fullWidth]="true"
              [loading]="generating() === goal.id"
              [disabled]="generating() === goal.id"
              (clicked)="handleGenerateClick($event, goal)"
            >
              {{ getSkillCount(goal) > 0 ? '🔄 Regenerate Skills' : '🤖 Generate Skill Tree' }}
            </app-button>
          </div>
        </app-card>

        <!-- Empty State -->
        <app-card *ngIf="goals().length === 0" class="empty-card">
          <div class="empty-content">
            <span class="empty-icon">🎯</span>
            <h3>No goals yet</h3>
            <p>Create your first learning goal to get started!</p>
            <app-button
              variant="primary"
              (clicked)="showCreateModal.set(true)"
            >
              Create Your First Goal
            </app-button>
          </div>
        </app-card>
      </main>

      <!-- Loading State -->
      <div class="loading-container" *ngIf="loading()">
        <app-spinner size="xl" variant="primary" text="Loading your goals..." />
      </div>

      <!-- Create Goal Modal -->
      <app-modal
        [isOpen]="showCreateModal()"
        size="md"
        [hasFooter]="true"
        (closed)="closeCreateModal()"
        ariaLabelledBy="create-goal-title"
      >
        <h2 modal-header id="create-goal-title">Create New Goal</h2>
        
        <div class="modal-form">
          <div class="form-group">
            <label for="goal-title">Title</label>
            <input
              id="goal-title"
              [(ngModel)]="newTitle"
              placeholder="e.g. Learn C#"
              maxlength="200"
              (keyup.enter)="createGoal()"
            />
          </div>
          <div class="form-group">
            <label for="goal-description">Description</label>
            <textarea
              id="goal-description"
              [(ngModel)]="newDescription"
              placeholder="What do you want to achieve?"
              rows="4"
              maxlength="2000"
            ></textarea>
          </div>
        </div>

        <div modal-footer class="modal-actions">
          <app-button
            variant="secondary"
            (clicked)="closeCreateModal()"
          >
            Cancel
          </app-button>
          <app-button
            variant="primary"
            [disabled]="!newTitle.trim() || !newDescription.trim()"
            (clicked)="createGoal()"
          >
            Create Goal
          </app-button>
        </div>
      </app-modal>

      <!-- Generation Progress Modal -->
      <app-modal
        [isOpen]="!!generating()"
        size="md"
        [showCloseButton]="false"
        [closeOnOverlayClick]="false"
        [closeOnEscape]="false"
      >
        <div class="progress-modal-content">
          <div class="progress-spinner-wrapper">
            <app-spinner size="xl" variant="primary" />
          </div>
          <h2>{{ progressTitle() }}</h2>
          <p class="progress-message">{{ progressMessage() }}</p>
          <app-progress-bar
            [current]="progressPercent()"
            [max]="100"
            [showLabel]="false"
            [showValue]="false"
            variant="xp"
            size="lg"
          />
          <p class="progress-hint">This may take 1-2 minutes. Please wait...</p>
        </div>
      </app-modal>

      <!-- Error Modal -->
      <app-modal
        [isOpen]="showErrorModal()"
        size="md"
        [hasFooter]="true"
        (closed)="showErrorModal.set(false)"
      >
        <div class="error-modal-content">
          <div class="error-icon">⚠️</div>
          <h2 modal-header>Generation Failed</h2>
          <p class="error-message">{{ errorMessage() }}</p>
        </div>

        <div modal-footer class="modal-actions">
          <app-button
            variant="secondary"
            (clicked)="showErrorModal.set(false)"
          >
            Cancel
          </app-button>
          <app-button
            variant="primary"
            (clicked)="retryGeneration()"
          >
            🔄 Try Again
          </app-button>
        </div>
      </app-modal>
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
      margin-bottom: 2rem;
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
      margin: 0;
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.9rem;
      margin: 0.25rem 0 0 0;
    }

    /* Stats Section */
    .stats-section {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }

    .stat-card {
      background: linear-gradient(135deg, rgba(124, 58, 237, 0.1), rgba(6, 182, 212, 0.1));
    }

    .stat-content {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .stat-icon {
      font-size: 2.5rem;
      flex-shrink: 0;
    }

    .stat-details {
      flex: 1;
    }

    .stat-label {
      color: var(--text-secondary);
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin: 0 0 0.25rem 0;
    }

    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      font-family: 'Rajdhani', sans-serif;
      background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      margin: 0;
    }

    /* Goals Grid */
    .goals-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.5rem;
    }

    .goal-card-wrapper {
      transition: all 0.3s ease;
    }

    .goal-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
    }

    .skill-count {
      color: var(--text-muted);
      font-size: 0.8rem;
    }

    .goal-card-body {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .goal-title {
      font-size: 1.5rem;
      color: var(--text-primary);
      margin: 0;
      line-height: 1.3;
    }

    .goal-description {
      color: var(--text-secondary);
      font-size: 0.9rem;
      line-height: 1.6;
      margin: 0;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .goal-progress {
      margin-top: 0.5rem;
    }

    .goal-card-footer {
      width: 100%;
    }

    /* Empty State */
    .empty-card {
      grid-column: 1 / -1;
      border-style: dashed;
    }

    .empty-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 3rem 2rem;
      gap: 1rem;
    }

    .empty-icon {
      font-size: 4rem;
      opacity: 0.5;
    }

    .empty-content h3 {
      font-size: 1.5rem;
      color: var(--text-primary);
      margin: 0;
    }

    .empty-content p {
      color: var(--text-muted);
      margin: 0 0 1rem 0;
    }

    /* Loading */
    .loading-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 50vh;
    }

    /* Modal Forms */
    .modal-form {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
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

    /* Progress Modal */
    .progress-modal-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.5rem;
      text-align: center;
      padding: 1rem 0;
    }

    .progress-spinner-wrapper {
      margin-bottom: 0.5rem;
    }

    .progress-modal-content h2 {
      font-size: 1.75rem;
      background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      margin: 0;
    }

    .progress-message {
      color: var(--text-primary);
      font-size: 1.1rem;
      margin: 0;
    }

    .progress-hint {
      color: var(--text-muted);
      font-size: 0.85rem;
      margin: 0;
    }

    /* Error Modal */
    .error-modal-content {
      text-align: center;
    }

    .error-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .error-modal-content h2 {
      font-size: 1.75rem;
      color: var(--accent-red);
      margin: 0 0 1rem 0;
    }

    .error-message {
      color: var(--text-secondary);
      font-size: 1rem;
      line-height: 1.6;
      white-space: pre-line;
      margin: 0;
    }

    /* Responsive */
    @media (max-width: 640px) {
      .goals-page {
        padding: 1rem;
      }

      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }

      .stats-section {
        grid-template-columns: 1fr;
      }

      .goals-grid {
        grid-template-columns: 1fr;
      }
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
  showCreateModal = signal(false);
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

  getTotalSkills(): number {
    return this.goals().reduce((sum, goal) => sum + this.getSkillCount(goal), 0);
  }

  getActiveGoalsCount(): number {
    return this.goals().filter(g => g.status === 'Active').length;
  }

  getStatusBadgeVariant(status: string): BadgeVariant {
    const statusMap: Record<string, BadgeVariant> = {
      'Draft': 'draft',
      'Active': 'active',
      'Completed': 'completed',
      'Archived': 'archived'
    };
    return statusMap[status] || 'info';
  }

  openGoal(goal: Goal) {
    this.router.navigate(['/goals', goal.id, 'skill-tree']);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
    this.newTitle = '';
    this.newDescription = '';
  }

  createGoal() {
    if (!this.newTitle.trim() || !this.newDescription.trim()) return;

    this.api.createGoal(this.newTitle, this.newDescription).subscribe({
      next: goal => {
        this.goals.update(goals => [...goals, goal]);
        this.closeCreateModal();
      }
    });
  }

  handleGenerateClick(event: Event, goal: Goal) {
    event.stopPropagation();
    this.generateTree(goal);
  }

  generateTree(goal: Goal) {
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
        this.generateTree(goal);
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