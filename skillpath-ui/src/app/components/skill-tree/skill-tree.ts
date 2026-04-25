import { Component, OnInit, OnDestroy, inject, signal, ElementRef, ViewChild, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { ErrorHandlerService } from '../../services/error-handler.service';
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
  templateUrl: './skill-tree.html',
  styleUrls: ['./skill-tree.scss']
})
export class SkillTreeComponent implements OnInit, OnDestroy {
  @ViewChild('cyContainer', { static: false }) cyContainer!: ElementRef;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(ApiService);
  private destroyRef = inject(DestroyRef);
  private errorHandler = inject(ErrorHandlerService);

  loading = signal(true);
  currentGoal = signal<Goal | null>(null);
  selectedSkill = signal<Skill | null>(null);
  tasks = signal<LearningTask[]>([]);
  tasksLoading = signal(false);
  regeneratingTasks = signal(false);
  showRegenerateModal = signal(false);

  private cy: cytoscape.Core | null = null;
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
    this.api.getGoals()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({

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
    this.api.getSkills(this.goalId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: skills => {
          this.allSkills = skills;
          this.loading.set(false);
          setTimeout(() => this.initCytoscape(skills), 100);
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

    if (!this.cyContainer?.nativeElement) return;

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
    this.api.getTasks(this.goalId, skill.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: tasks => {
          this.tasks.set(tasks);
          this.tasksLoading.set(false);
        },
        error: (err) => {
          this.errorHandler.handleHttpError(err, 'Load Tasks');
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

    this.api.updateTaskStatus(
      this.goalId,
      currentSkill.id,
      task.id,
      nextStatus
    )
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: updatedTask => {
        this.tasks.update(tasks =>
          tasks.map(t => t.id === updatedTask.id ? updatedTask : t)
        );

        setTimeout(() => {
          this.loadSkillTree();
        }, 300);
      },
      error: (err) => {
        this.errorHandler.handleHttpError(err, 'Update Tasks');
      }
    });
  }

  confirmRegenerateTasks() {
    const skill = this.selectedSkill();
    if (!skill) return;

    this.showRegenerateModal.set(false);
    this.regeneratingTasks.set(true);

    this.api.regenerateTasksForSkill(
      this.goalId,
      skill.id
    )
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: (newTasks) => {
        this.tasks.set(newTasks);
        this.regeneratingTasks.set(false);
      },
      error: (err) => {
        this.errorHandler.handleHttpError(err, 'Regenerate Tasks');
        this.regeneratingTasks.set(false);
      }
    });
  }
}