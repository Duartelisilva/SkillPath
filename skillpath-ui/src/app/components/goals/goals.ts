import { Component, OnInit, OnDestroy, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ErrorHandlerService } from '../../services/error-handler.service';
import { ApiService } from '../../services/api.service';
import { Goal } from '../../models/goal.model';
import { GenerationSettingsModal, GenerationSettings } from '../../shared/components/generation-settings-modal/generation-settings-modal';

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
    ProgressBarComponent,
    GenerationSettingsModal
  ],
  templateUrl: './goals.html',
  styleUrls: ['./goals.scss']
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
  showSettingsModal = signal(false);
  pendingGoalId = signal<string | null>(null);

  private api = inject(ApiService);
  private router = inject(Router);
  private progressInterval: ReturnType<typeof setInterval> | null = null;
  private currentGoalId: string | null = null;
  private destroyRef = inject(DestroyRef);
  private errorHandler = inject(ErrorHandlerService);

  ngOnInit() {
    this.loadGoals();
  }

  ngOnDestroy() {
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
    }
  }

  loadGoals() {
    this.api.getGoals()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
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
    // Clear generation state when opening a goal
    this.generationStates.delete(goal.id);
    this.router.navigate(['/goals', goal.id, 'skill-tree']);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
    this.newTitle = '';
    this.newDescription = '';
  }

  createGoal() {
    if (!this.newTitle.trim() || !this.newDescription.trim()) return;

    this.api.createGoal(this.newTitle, this.newDescription)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: goal => {
          this.goals.update(goals => [...goals, goal]);
          this.closeCreateModal();
        }
    });
  }

  handleGenerateClick(event: Event, goal: Goal) {
    event.stopPropagation();
    this.pendingGoalId.set(goal.id);
    this.showSettingsModal.set(true); // Open settings modal instead
  }

  generateTree(goal: Goal) {
    if (this.generating()) return;

    this.currentGoalId = goal.id;
    this.generating.set(goal.id);
    
    // Set generation state
    this.generationStates.set(goal.id, {
      status: 'generating',
      progress: 0
    });
    
    this.progressTitle.set('Generating Skill Tree');
    this.startProgressSimulation(goal.id);
  
    this.api.generateSkillTree(goal.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.stopProgressSimulation();
          this.generating.set(null);
          
          // Update state to success
          this.generationStates.set(goal.id, {
            status: 'success',
            progress: 100
          });
          
          this.loadGoals(); // Refresh to show new skill count
          this.currentGoalId = null;
        },
        error: (err) => {
          this.stopProgressSimulation();
          this.generating.set(null);
          
          // Update state to failed
          this.generationStates.set(goal.id, {
            status: 'failed',
            progress: 0
          });
          
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

  cancelGeneration() {
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
      this.progressInterval = null;
    }

    this.generating.set(null);
  }


  private handleGenerationError(error: any) {
    const appError =
      this.errorHandler.handleGenerationError(error, 'Skill Tree');

    this.errorMessage.set(appError.message);
    this.showErrorModal.set(true);

    
    let message = 'The AI failed to generate a valid skill tree. This can happen when:';
    message += '\n\n• The AI response was malformed';
    message += '\n• The connection to Ollama was interrupted';
    message += '\n• The model generated invalid JSON';
    message += '\n\nWould you like to try again?';

    this.errorMessage.set(message);
    this.showErrorModal.set(true);
  }

  private startProgressSimulation(goalId: string) {
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
      
      // Update the generation state
      const state = this.generationStates.get(goalId);
      if (state) {
        state.progress = currentProgress;
      }
  
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

  private generationStates = new Map<string, {
    status: 'generating' | 'success' | 'failed' | null;
    progress: number;
  }>();

  getGenerationStatus(goalId: string): 'generating' | 'success' | 'failed' | null {
  return this.generationStates.get(goalId)?.status || null;
  }
  
  getGenerationProgress(goalId: string): number {
    return this.generationStates.get(goalId)?.progress || 0;
  }
  
  shouldShowGenerationUI(goal: Goal): boolean {
    const state = this.generationStates.get(goal.id);
    if (!state) return false;
    
    // Hide after user navigates away or reloads
    if (state.status === 'success' || state.status === 'failed') {
      // Check if user has interacted since completion
      return false; // Implement interaction tracking if needed
    }
    
    return true;
  }

  generateTreeWithSettings(settings: GenerationSettings) {
    const goalId = this.pendingGoalId();
    if (!goalId) return;

    const goal = this.goals().find(g => g.id === goalId);
    if (!goal) return;

    this.showSettingsModal.set(false);
    this.generating.set(goalId);
    
    this.generationStates.set(goalId, {
      status: 'generating',
      progress: 0
    });
    
    this.progressTitle.set('Generating Skill Tree');
    this.startProgressSimulation(goalId);

    this.api.generateSkillTree(goalId, settings) // Pass settings
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.stopProgressSimulation();
          this.generating.set(null);
          this.generationStates.set(goalId, { status: 'success', progress: 100 });
          this.loadGoals();
          this.pendingGoalId.set(null);
        },
        error: (err) => {
          this.stopProgressSimulation();
          this.generating.set(null);
          this.generationStates.set(goalId, { status: 'failed', progress: 0 });
          this.handleGenerationError(err);
          this.pendingGoalId.set(null);
        }
      });
  }
}