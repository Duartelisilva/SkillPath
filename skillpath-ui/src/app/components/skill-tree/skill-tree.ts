import { Component, OnInit, OnDestroy, inject, signal, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { Skill } from '../../models/skill.model';
import { LearningTask } from '../../models/task.model';
import cytoscape from 'cytoscape';

@Component({
  selector: 'app-skill-tree',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skill-tree-page">
      <header class="page-header">
        <button class="btn btn-secondary back-btn" (click)="goBack()">← Back</button>
        <h1 class="page-title">Skill Tree</h1>
      </header>

      <div class="layout" *ngIf="!loading()">
        <div #cyContainer class="cy-container"></div>

        <!-- Task Panel -->
        <div class="task-panel" [class.open]="selectedSkill() !== null">
          <div class="panel-header" *ngIf="selectedSkill()">
            <div class="skill-info">
              <span class="skill-status-dot {{ selectedSkill()!.status }}"></span>
              <h2>{{ selectedSkill()!.name }}</h2>
            </div>
            <button class="close-btn" (click)="selectedSkill.set(null)">✕</button>
          </div>

          <p class="skill-desc" *ngIf="selectedSkill()">{{ selectedSkill()!.description }}</p>

          <div class="tasks-section" *ngIf="selectedSkill()">
            <h3>Tasks</h3>
            <div *ngIf="tasksLoading()" class="tasks-loading">
              <div class="mini-spinner"></div>
            </div>
            <div class="task-list" *ngIf="!tasksLoading()">
              <div class="task-item {{ task.status }}" *ngFor="let task of tasks()">
                <div class="task-check">
                  <span *ngIf="task.status === 'Completed'">✓</span>
                  <span *ngIf="task.status === 'InProgress'">◎</span>
                  <span *ngIf="task.status === 'NotStarted'">○</span>
                </div>
                <div class="task-content">
                  <p class="task-title">{{ task.title }}</p>
                  <p class="task-desc">{{ task.description }}</p>
                </div>
              </div>
              <div *ngIf="tasks().length === 0" class="no-tasks">No tasks yet.</div>
            </div>
          </div>
        </div>
      </div>

      <div class="loading" *ngIf="loading()">
        <div class="spinner"></div>
        <p>Loading skill tree...</p>
      </div>
    </div>
  `,
  styles: [`
    .skill-tree-page {
      min-height: 100vh;
      background: var(--bg-primary);
      display: flex;
      flex-direction: column;
    }

    .page-header {
      display: flex;
      align-items: center;
      gap: 1.5rem;
      padding: 1.5rem 2rem;
      border-bottom: 1px solid var(--border-accent);
    }

    .page-title {
      font-size: 1.75rem;
      background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .layout {
      display: flex;
      flex: 1;
      overflow: hidden;
      height: calc(100vh - 80px);
    }

    .cy-container {
      flex: 1;
      background: var(--bg-secondary);
    }

    .task-panel {
      width: 0;
      overflow: hidden;
      background: var(--bg-card);
      border-left: 1px solid var(--border-accent);
      transition: width 0.3s ease;
      display: flex;
      flex-direction: column;
      overflow-y: auto;

      &.open {
        width: 380px;
        padding: 1.5rem;
      }
    }

    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1rem;
    }

    .skill-info {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .skill-status-dot {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      flex-shrink: 0;

      &.Locked { background: var(--locked); }
      &.Available { background: var(--available); }
      &.InProgress { background: var(--in-progress); }
      &.Completed { background: var(--completed); }
    }

    .skill-info h2 {
      font-size: 1.4rem;
      color: var(--text-primary);
    }

    .close-btn {
      background: none;
      border: none;
      color: var(--text-muted);
      font-size: 1.25rem;
      cursor: pointer;
      padding: 0.25rem;
      line-height: 1;

      &:hover { color: var(--text-primary); }
    }

    .skill-desc {
      color: var(--text-secondary);
      font-size: 0.9rem;
      line-height: 1.6;
      margin-bottom: 1.5rem;
    }

    .tasks-section h3 {
      font-size: 1.1rem;
      color: var(--text-secondary);
      margin-bottom: 1rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      font-size: 0.85rem;
    }

    .task-item {
      display: flex;
      gap: 0.75rem;
      padding: 0.75rem;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      border: 1px solid var(--border-accent);
      background: var(--bg-secondary);

      &.Completed {
        border-color: rgba(16, 185, 129, 0.3);
        .task-check { color: var(--completed); }
      }
      &.InProgress {
        border-color: rgba(245, 158, 11, 0.3);
        .task-check { color: var(--in-progress); }
      }
      &.NotStarted {
        .task-check { color: var(--text-muted); }
      }
    }

    .task-check {
      font-size: 1.1rem;
      padding-top: 0.1rem;
      flex-shrink: 0;
    }

    .task-title {
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.9rem;
      margin-bottom: 0.25rem;
    }

    .task-desc {
      color: var(--text-secondary);
      font-size: 0.8rem;
      line-height: 1.5;
    }

    .no-tasks, .tasks-loading {
      color: var(--text-muted);
      text-align: center;
      padding: 2rem 0;
    }

    .mini-spinner {
      width: 24px;
      height: 24px;
      border: 2px solid var(--border-accent);
      border-top-color: var(--accent-purple);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto;
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      flex: 1;
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

    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class SkillTreeComponent implements OnInit, OnDestroy {
  @ViewChild('cyContainer', { static: false }) cyContainer!: ElementRef;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(ApiService);

  loading = signal(true);
  selectedSkill = signal<Skill | null>(null);
  tasks = signal<LearningTask[]>([]);
  tasksLoading = signal(false);

  private cy: any = null;
  private goalId = '';

  ngOnInit() {
    this.goalId = this.route.snapshot.paramMap.get('goalId') ?? '';
    this.loadSkillTree();
  }

  ngOnDestroy() {
    this.cy?.destroy();
  }

  goBack() {
    this.router.navigate(['/goals']);
  }

  loadSkillTree() {
    this.api.getSkills(this.goalId).subscribe({
      next: skills => {
        this.loading.set(false);
        setTimeout(() => this.initCytoscape(skills), 50);
      },
      error: () => this.loading.set(false)
    });
  }

  initCytoscape(skills: Skill[]) {
    if (!this.cyContainer) return;

    const statusColors: Record<string, string> = {
      Locked:     '#374151',
      Available:  '#1d4ed8',
      InProgress: '#d97706',
      Completed:  '#059669',
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
            'font-size': '13px',
            'font-family': 'Inter, sans-serif',
            'text-valign': 'center',
            'text-halign': 'center',
            'text-wrap': 'wrap',
            'text-max-width': '120px',
            'width': 140,
            'height': 60,
            'shape': 'roundrectangle',
            'border-width': 2,
            'border-color': (ele: any) => statusColors[ele.data('status')] ?? '#374151',
            'border-opacity': 0.8,
          } as any
        },
        {
          selector: 'node:selected',
          style: {
            'border-color': '#7c3aed',
            'border-width': 3,
            'box-shadow': '0 0 15px rgba(124,58,237,0.6)',
          } as any
        },
        {
          selector: 'edge',
          style: {
            'width': 2,
            'line-color': '#2d3f5e',
            'target-arrow-color': '#2d3f5e',
            'target-arrow-shape': 'triangle',
            'curve-style': 'bezier',
          } as any
        }
      ],
      layout: {
        name: 'breadthfirst',
        directed: true,
        padding: 60,
        spacingFactor: 1.75,
      },
      userZoomingEnabled: true,
      userPanningEnabled: true,
      autoungrabify: true,   // <-- disables node dragging
      boxSelectionEnabled: false,
    });

    this.cy.on('tap', 'node', (evt: any) => {
      const skill: Skill = evt.target.data('skill');
      this.selectedSkill.set(skill);
      this.loadTasks(skill);
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
      error: () => this.tasksLoading.set(false)
    });
  }
}