import { Component, OnInit, OnDestroy, inject, signal, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { Goal } from '../../models/goal.model';
import { Skill } from '../../models/skill.model';
import { LearningTask } from '../../models/task.model';
import cytoscape from 'cytoscape';

// Import components
import { ButtonComponent } from '../../shared/components/button/button';
import { BadgeComponent } from '../../shared/components/badge/badge';
import { BadgeVariant } from '../../shared/components/badge/badge';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar';
import { SpinnerComponent } from '../../shared/components/spinner/spinner';
import { ModalComponent } from '../../shared/components/modal/modal';

@Component({
  selector: 'app-skill-tree',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent,
    BadgeComponent,
    ProgressBarComponent,
    SpinnerComponent,
    ModalComponent
  ],
  template: `
    <div class="skill-tree-page">
      <!-- Breadcrumb Navigation (NEW!) -->
      <nav class="breadcrumb" *ngIf="!loading()">
        <a routerLink="/goals" class="breadcrumb-link">
          <span class="breadcrumb-icon">🏠</span>
          Goals
        </a>
        <span class="breadcrumb-separator">›</span>
        <span class="breadcrumb-current">{{ currentGoal()?.title || 'Skill Tree' }}</span>
      </nav>

      <!-- Goal Header (NEW!) -->
      <header class="goal-header" *ngIf="!loading() && currentGoal()">
        <div class="goal-header-content">
          <div class="goal-header-main">
            <h1 class="goal-header-title">{{ currentGoal()!.title }}</h1>
             <app-badge [variant]="getStatusBadgeVariant(currentGoal()!.status)">
              {{ currentGoal()!.status }}
            </app-badge>
          </div>
          <p class="goal-header-description">{{ currentGoal()!.description }}</p>
        </div>
        <div class="goal-header-actions">
          <app-button variant="secondary" size="sm" (clicked)="resetZoom()">
            🔍 Reset View
          </app-button>
          <app-button variant="secondary" size="sm" (clicked)="goBack()">
            ← Back to Goals
          </app-button>
        </div>
      </header>

      <div class="layout" *ngIf="!loading()">
        <div #cyContainer class="cy-container"></div>

        <!-- Enhanced Task Panel -->
        <div class="task-panel" [class.open]="selectedSkill() !== null">
          <div class="panel-header" *ngIf="selectedSkill()">
            <div class="skill-info">
              <app-badge
                [variant]="getSkillStatusBadgeVariant(selectedSkill()!.status)"
                [showDot]="true"
                size="sm"
              >
                {{ selectedSkill()!.status }}
              </app-badge>
              <h2>{{ selectedSkill()!.name }}</h2>
            </div>
            <app-button
              variant="ghost"
              size="sm"
              (clicked)="closePanel()"
              class="close-btn-wrapper"
            >
              ✕
            </app-button>
          </div>

          <p class="skill-desc" *ngIf="selectedSkill()">
            {{ selectedSkill()!.description }}
          </p>

          <!-- XP Progress Bar (NEW!) -->
          <div class="xp-progress-section" *ngIf="selectedSkill() && tasks().length > 0">
            <app-progress-bar
              label="Experience Points"
              [current]="getEarnedXP()"
              [max]="selectedSkill()!.requiredExperiencePoints || 100"
              unit="XP"
              variant="xp"
              size="md"
              [showPercentage]="true"
            />
          </div>

          <div class="tasks-section" *ngIf="selectedSkill()">
            <div class="tasks-header">
              <h3>Tasks</h3>
              <app-button
                variant="ghost"
                size="sm"
                (clicked)="showRegenerateModal.set(true)"
                [disabled]="regeneratingTasks() || selectedSkill()?.status === 'Completed'"
              >
                {{ regeneratingTasks() ? '⚙ Regenerating...' : '🔄 Regenerate' }}
              </app-button>
            </div>

            <!-- Tasks Loading -->
            <div *ngIf="tasksLoading()" class="tasks-loading">
              <app-spinner size="sm" variant="primary" />
            </div>

            <!-- Task List -->
            <div class="task-list" *ngIf="!tasksLoading()">
              <div class="task-item {{ task.status }}" *ngFor="let task of tasks()">
                <div
                  class="task-check"
                  (click)="toggleTaskStatus(task)"
                  [class.disabled]="!canToggleTask(task)"
                >
                  <span *ngIf="task.status === 'Completed'" class="check-icon">✓</span>
                  <span *ngIf="task.status === 'InProgress'" class="progress-icon">◎</span>
                  <span *ngIf="task.status === 'NotStarted'" class="empty-icon">○</span>
                </div>
                <div class="task-content">
                  <div class="task-header-row">
                    <p class="task-title">{{ task.title }}</p>
                    <app-badge
                      [variant]="task.experiencePoints === 0 ? 'archived' : 'info'"
                      size="sm"
                    >
                      {{ task.experiencePoints }} XP
                    </app-badge>
                  </div>
                  <p class="task-desc">{{ task.description }}</p>
                </div>
              </div>
              <div *ngIf="tasks().length === 0" class="no-tasks">
                <span class="no-tasks-icon">📝</span>
                <p>No tasks generated for this skill.</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-container" *ngIf="loading()">
        <app-spinner size="xl" variant="primary" text="Loading skill tree..." />
      </div>

      <!-- Regenerate Confirmation Modal -->
      <app-modal
        [isOpen]="showRegenerateModal()"
        size="sm"
        [hasFooter]="true"
        (closed)="showRegenerateModal.set(false)"
      >
        <h2 modal-header>Regenerate Tasks?</h2>
        <p>This will delete all existing tasks for <strong>{{ selectedSkill()?.name }}</strong> and generate new ones using AI.</p>
        <p style="color: var(--text-muted); font-size: 0.875rem; margin-top: 0.5rem;">
          ⚠️ This action cannot be undone.
        </p>

        <div modal-footer class="modal-actions">
          <app-button
            variant="secondary"
            (clicked)="showRegenerateModal.set(false)"
          >
            Cancel
          </app-button>
          <app-button
            variant="primary"
            (clicked)="confirmRegenerateTasks()"
          >
            Regenerate Tasks
          </app-button>
        </div>
      </app-modal>
    </div>
  `,
  styles: [`
    .skill-tree-page {
      min-height: 100vh;
      background: var(--bg-primary);
      display: flex;
      flex-direction: column;
    }

    /* Breadcrumb Navigation */
    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem 2rem;
      background: var(--bg-secondary);
      border-bottom: 1px solid var(--border-accent);
      font-size: 0.875rem;
    }

    .breadcrumb-link {
      display: flex;
      align-items: center;
      gap: 0.375rem;
      color: var(--text-secondary);
      text-decoration: none;
      transition: color 0.2s;

      &:hover {
        color: var(--accent-purple);
      }
    }

    .breadcrumb-icon {
      font-size: 1rem;
    }

    .breadcrumb-separator {
      color: var(--text-muted);
    }

    .breadcrumb-current {
      color: var(--text-primary);
      font-weight: 500;
      max-width: 300px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    /* Goal Header */
    .goal-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 2rem;
      padding: 1.5rem 2rem;
      background: var(--bg-card);
      border-bottom: 1px solid var(--border-accent);
    }

    .goal-header-content {
      flex: 1;
      min-width: 0;
    }

    .goal-header-main {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 0.5rem;
    }

    .goal-header-title {
      font-size: 1.75rem;
      background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      margin: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .goal-header-description {
      color: var(--text-secondary);
      font-size: 0.95rem;
      line-height: 1.6;
      margin: 0;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .goal-header-actions {
      display: flex;
      gap: 0.75rem;
      flex-shrink: 0;
    }

    /* Layout */
    .layout {
      display: flex;
      position: relative;
      flex: 1;
      overflow: hidden;
      height: calc(100vh - 200px);
    }

    .cy-container {
      flex: 1;
      background: var(--bg-secondary);
    }

    /* Task Panel */
    .task-panel {
      position: absolute;
      top: 0;
      right: 0;
      height: 100%;
      width: 0;
      overflow: hidden;
      background: var(--bg-card);
      border-left: 1px solid var(--border-accent);
      transition: width 0.3s ease;
      display: flex;
      flex-direction: column;
      overflow-y: auto;
      z-index: 10;
      box-shadow: -4px 0 20px rgba(0, 0, 0, 0.3);
    }

    .task-panel.open {
      width: 420px;
      padding: 1.5rem;
    }

    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1rem;
      gap: 1rem;
    }

    .skill-info {
      flex: 1;
      min-width: 0;
    }

    .skill-info h2 {
      font-size: 1.4rem;
      color: var(--text-primary);
      margin: 0.5rem 0 0 0;
      line-height: 1.3;
    }

    .close-btn-wrapper {
      flex-shrink: 0;
      padding: 0.25rem 0.5rem !important;
      font-size: 1.25rem;
    }

    .skill-desc {
      color: var(--text-secondary);
      font-size: 0.9rem;
      line-height: 1.6;
      margin-bottom: 1.5rem;
    }

    /* XP Progress Section */
    .xp-progress-section {
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: rgba(124, 58, 237, 0.05);
      border: 1px solid rgba(124, 58, 237, 0.2);
      border-radius: 8px;
    }

    /* Tasks Section */
    .tasks-section {
      flex: 1;
    }

    .tasks-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .tasks-header h3 {
      font-size: 0.85rem;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.1em;
      margin: 0;
    }

    .task-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .task-item {
      display: flex;
      gap: 0.75rem;
      padding: 0.875rem;
      border-radius: 8px;
      border: 1px solid var(--border-accent);
      background: var(--bg-secondary);
      transition: all 0.2s ease;

      &:hover {
        border-color: var(--accent-purple);
        background: var(--bg-card-hover);
      }

      &.Completed {
        border-color: rgba(16, 185, 129, 0.3);
        background: rgba(16, 185, 129, 0.05);

        .task-check {
          color: var(--completed);
        }
      }

      &.InProgress {
        border-color: rgba(245, 158, 11, 0.3);
        background: rgba(245, 158, 11, 0.05);

        .task-check {
          color: var(--in-progress);
        }
      }

      &.NotStarted {
        .task-check {
          color: var(--text-muted);
        }
      }
    }

    .task-check {
      font-size: 1.2rem;
      padding-top: 0.1rem;
      flex-shrink: 0;
      cursor: pointer;
      transition: transform 0.2s ease;
      user-select: none;
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;

      &:hover:not(.disabled) {
        transform: scale(1.3);
      }

      &:active:not(.disabled) {
        transform: scale(1.1);
      }

      &.disabled {
        cursor: not-allowed;
        opacity: 0.5;
      }
    }

    .task-content {
      flex: 1;
      min-width: 0;
    }

    .task-header-row {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 0.5rem;
      margin-bottom: 0.375rem;
    }

    .task-title {
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.9rem;
      margin: 0;
      flex: 1;
      line-height: 1.4;
    }

    .task-desc {
      color: var(--text-secondary);
      font-size: 0.8rem;
      line-height: 1.5;
      margin: 0;
    }

    .no-tasks,
    .tasks-loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem 1rem;
      text-align: center;
      color: var(--text-muted);
      gap: 0.75rem;
    }

    .no-tasks-icon {
      font-size: 2.5rem;
      opacity: 0.5;
    }

    .no-tasks p {
      margin: 0;
      font-size: 0.9rem;
    }

    /* Loading */
    .loading-container {
      display: flex;
      align-items: center;
      justify-content: center;
      flex: 1;
    }

    /* Modal Actions */
    .modal-actions {
      display: flex;
      gap: 1rem;
      justify-content: flex-end;
    }

    /* Responsive */
    @media (max-width: 1024px) {
      .goal-header {
        flex-direction: column;
      }

      .goal-header-actions {
        width: 100%;
        justify-content: flex-end;
      }

      .task-panel.open {
        width: 100%;
        max-width: 400px;
      }
    }

    @media (max-width: 640px) {
      .breadcrumb {
        padding: 0.75rem 1rem;
      }

      .goal-header {
        padding: 1rem;
      }

      .task-panel.open {
        max-width: 100%;
      }

      .layout {
        height: calc(100vh - 180px);
      }
    }
  `]
})
export class SkillTreeComponent implements OnInit, OnDestroy {
  @ViewChild('cyContainer', { static: false }) cyContainer!: ElementRef;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(ApiService);

  loading = signal(true);
  currentGoal = signal<Goal | null>(null);
  selectedSkill = signal<Skill | null>(null);
  tasks = signal<LearningTask[]>([]);
  tasksLoading = signal(false);
  regeneratingTasks = signal(false);
  showRegenerateModal = signal(false);

  private cy: any = null;
  private goalId = '';
  private allSkills: Skill[] = [];

  ngOnInit() {
    this.goalId = this.route.snapshot.paramMap.get('goalId') ?? '';
    this.loadGoalAndSkillTree();
  }

  ngOnDestroy() {
    this.cy?.destroy();
  }

  loadGoalAndSkillTree() {
    // Load goal first
    this.api.getGoals().subscribe({
      next: (goals) => {
        const goal = goals.find(g => g.id === this.goalId);
        this.currentGoal.set(goal || null);
        
        // Then load skills
        this.loadSkillTree();
      },
      error: () => {
        this.loadSkillTree(); // Continue even if goal fetch fails
      }
    });
  }

  loadSkillTree() {
    this.api.getSkills(this.goalId).subscribe({
      next: skills => {
        this.allSkills = skills;
        this.loading.set(false);
        setTimeout(() => this.initCytoscape(skills), 50);
      },
      error: () => this.loading.set(false)
    });
  }

  goBack() {
    this.router.navigate(['/goals']);
  }

  closePanel() {
    this.selectedSkill.set(null);
    if (this.cy) {
      this.cy.elements().unselect();
    }
  }

  resetZoom() {
    if (this.cy) {
      this.cy.fit(undefined, 50);
    }
  }

  getStatusBadgeVariant(status: string): BadgeVariant {
    const map: Record<string, BadgeVariant> = {
      'Draft': 'draft',
      'Active': 'active',
      'Completed': 'completed',
      'Archived': 'archived'
    };
    return map[status] || 'info';
  }

  getSkillStatusBadgeVariant(status: string): BadgeVariant {
    const map: Record<string, BadgeVariant> = {
      'Locked': 'locked',
      'Available': 'available',
      'InProgress': 'in-progress',
      'Completed': 'completed'
    };
    return map[status] || 'info';
  }

  getEarnedXP(): number {
    const earned = this.tasks()
      .filter(t => t.status === 'Completed')
      .reduce((sum, t) => sum + t.experiencePoints, 0);
    
    const required = this.selectedSkill()?.requiredExperiencePoints || 100;
    return Math.min(earned, required);
  }

  canToggleTask(task: LearningTask): boolean {
    const skill = this.selectedSkill();
    if (!skill) return false;

    if (skill.status === 'Locked' || task.status === 'Completed') {
      return false;
    }

    return true;
  }

  initCytoscape(skills: Skill[]) {
    if (this.cy) {
      this.cy.destroy();
      this.cy = null;
    }

    if (!this.cyContainer) return;

    const statusColors: Record<string, string> = {
      Locked: '#374151',
      Available: '#1d4ed8',
      InProgress: '#d97706',
      Completed: '#059669',
    };

    const nodes = skills.map(s => ({
      data: {
        id: s.id,
        label: s.name,
        status: s.status,
        skill: s,
      }
    }));

    const edges: any[] = [];
    skills.forEach(s => {
      (s.dependsOn ?? []).forEach((depId: string) => {
        edges.push({ data: { source: depId, target: s.id } });
      });
    });

    this.cy = cytoscape({
      container: this.cyContainer.nativeElement,
      elements: { nodes, edges },
      style: [
        {
          selector: 'node',
          style: {
            'background-color': (ele: any) => statusColors[ele.data('status')] ?? '#374151',
            'label': 'data(label)',
            'color': '#f1f5f9',
            'font-size': '15px',
            'font-weight': '600',
            'font-family': 'Inter, sans-serif',
            'text-valign': 'center',
            'text-halign': 'center',
            'text-wrap': 'wrap',
            'text-max-width': '160px',
            'width': 180,
            'height': 80,
            'shape': 'roundrectangle',
            'border-width': 3,
            'border-color': (ele: any) => statusColors[ele.data('status')] ?? '#374151',
            'border-opacity': 0.9,
            'padding': '12px',
          } as any
        },
        {
          selector: 'node:selected',
          style: {
            'border-color': '#7c3aed',
            'border-width': 4,
            'box-shadow': '0 0 20px rgba(124,58,237,0.8)',
          } as any
        },
        {
          selector: 'edge',
          style: {
            'width': 3,
            'line-color': '#4b5563',
            'target-arrow-color': '#4b5563',
            'target-arrow-shape': 'triangle',
            'curve-style': 'bezier',
            'arrow-scale': 1.5,
          } as any
        }
      ],
      layout: {
        name: 'breadthfirst',
        directed: true,
        padding: 80,
        spacingFactor: 1,
        avoidOverlap: true,
      },
      minZoom: 0.3,
      maxZoom: 2.0,
      wheelSensitivity: 3,
      userZoomingEnabled: true,
      userPanningEnabled: true,
      autoungrabify: true,
      boxSelectionEnabled: false,
    });

    this.cy.fit(undefined, 50);

    this.cy.on('tap', 'node', (evt: any) => {
      const skill: Skill = evt.target.data('skill');
      this.selectedSkill.set(skill);
      this.loadTasks(skill);
    });

    this.cy.on('tap', (evt: any) => {
      if (evt.target === this.cy) {
        this.closePanel();
      }
    });
  }

  loadTasks(skill: Skill) {
    this.tasksLoading.set(true);
    this.tasks.set([]);
    this.api.getTasks(this.goalId, skill.id).subscribe({
      next: tasks => {
        this.tasks.set(tasks);
        this.tasksLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading tasks:', err);
        this.tasksLoading.set(false);
      }
    });
  }

  toggleTaskStatus(task: LearningTask) {
    const currentSkill = this.selectedSkill();
    if (!currentSkill || !this.canToggleTask(task)) return;

    let nextStatus: string | null = null;

    if (task.status === 'NotStarted') {
      nextStatus = 'InProgress';
    } else if (task.status === 'InProgress') {
      nextStatus = 'Completed';
    }

    if (!nextStatus) return;

    this.api.updateTaskStatus(this.goalId, currentSkill.id, task.id, nextStatus).subscribe({
      next: updatedTask => {
        this.tasks.update(tasks =>
          tasks.map(t => t.id === updatedTask.id ? updatedTask : t)
        );

        setTimeout(() => {
          this.loadSkillTree();
        }, 300);
      },
      error: (err) => {
        console.error('Error updating task status:', err);
      }
    });
  }

  confirmRegenerateTasks() {
    const skill = this.selectedSkill();
    if (!skill) return;

    this.showRegenerateModal.set(false);
    this.regeneratingTasks.set(true);

    this.api.regenerateTasksForSkill(this.goalId, skill.id).subscribe({
      next: (newTasks) => {
        this.tasks.set(newTasks);
        this.regeneratingTasks.set(false);
      },
      error: (err) => {
        console.error('Error regenerating tasks:', err);
        this.regeneratingTasks.set(false);
      }
    });
  }
}