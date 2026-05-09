import { Component, OnInit, OnDestroy, inject, signal, ElementRef, ViewChild, DestroyRef, effect } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { ErrorHandlerService } from '../../services/error-handler.service';
import { Goal } from '../../models/goal.model';
import { Skill } from '../../models/skill.model';
import { LearningTask } from '../../models/task.model';
import cytoscape from 'cytoscape';
// @ts-ignore
import dagre from 'cytoscape-dagre';
// @ts-ignore
import coseBilkent from 'cytoscape-cose-bilkent';

// Import components
import { ButtonComponent } from '../../shared/components/button/button';
import { BadgeComponent } from '../../shared/components/badge/badge';
import { BadgeVariant } from '../../shared/components/badge/badge';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar';
import { SpinnerComponent } from '../../shared/components/spinner/spinner';
import { ModalComponent } from '../../shared/components/modal/modal';

// Register layout extensions
cytoscape.use(dagre);
cytoscape.use(coseBilkent);

type LayoutType = 'breadthfirst' | 'dagre' | 'cose-bilkent' | 'grid' | 'circle';

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
  
  // Layout controls
  currentLayout = signal<LayoutType>('breadthfirst');
  showLayoutMenu = signal(false);
  showMinimap = signal(false);
  
  // Hover tooltip
  tooltipVisible = signal(false);
  tooltipContent = signal<{
    skillName: string;
    status: string;
    earnedXP: number;
    requiredXP: number;
    taskCount: number;
    completedTasks: number;
    dependencies: string[];
  } | null>(null);
  tooltipX = signal(0);
  tooltipY = signal(0);

  private cy: cytoscape.Core | null = null;
  private goalId = '';
  private allSkills: Skill[] = [];
  private layoutAnimating = false;

  ngOnInit() {
    this.goalId = this.route.snapshot.paramMap.get('goalId') ?? '';
    this.loadGoalAndSkillTree();
  }

  ngOnDestroy() {
    this.cy?.destroy();
  }

  loadGoalAndSkillTree() {
    this.api.getGoals()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (goals) => {
          const goal = goals.find(g => g.id === this.goalId);
          this.currentGoal.set(goal || null);
          this.loadSkillTree();
        },
        error: () => {
          this.loadSkillTree();
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

  changeLayout(layout: LayoutType) {
    if (this.layoutAnimating || !this.cy) return;
    
    this.layoutAnimating = true;
    this.currentLayout.set(layout);
    this.showLayoutMenu.set(false);

    const layoutConfig = this.getLayoutConfig(layout);
    const cyLayout = this.cy.layout(layoutConfig);
    
    cyLayout.run();
    
    setTimeout(() => {
      this.layoutAnimating = false;
    }, 1000);
  }

  toggleMinimap() {
    this.showMinimap.update(v => !v);
  }

  exportAsPNG() {
    if (!this.cy) return;
    
    const png = this.cy.png({
      full: true,
      scale: 2,
      bg: '#0a0e1a'
    });

    const link = document.createElement('a');
    link.download = `${this.currentGoal()?.title || 'skill-tree'}.png`;
    link.href = png;
    link.click();
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

  private getLayoutConfig(layout: LayoutType): any {
    const configs: Record<LayoutType, any> = {
      breadthfirst: {
        name: 'breadthfirst',
        directed: true,
        padding: 80,
        spacingFactor: 1.2,
        avoidOverlap: true,
        animate: true,
        animationDuration: 500,
        animationEasing: 'ease-out'
      },
      dagre: {
        name: 'dagre',
        rankDir: 'TB',
        nodeSep: 80,
        rankSep: 120,
        animate: true,
        animationDuration: 500,
        animationEasing: 'ease-out'
      },
      'cose-bilkent': {
        name: 'cose-bilkent',
        idealEdgeLength: 150,
        nodeRepulsion: 8000,
        animate: true,
        animationDuration: 1000,
        animationEasing: 'ease-out',
        randomize: false
      },
      grid: {
        name: 'grid',
        rows: 3,
        padding: 80,
        animate: true,
        animationDuration: 500,
        animationEasing: 'ease-out'
      },
      circle: {
        name: 'circle',
        padding: 80,
        animate: true,
        animationDuration: 500,
        animationEasing: 'ease-out'
      }
    };

    return configs[layout];
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
        edges.push({ 
          data: { 
            source: depId, 
            target: s.id,
            id: `${depId}-${s.id}`
          } 
        });
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
            'transition-property': 'background-color, border-color, border-width',
            'transition-duration': '0.3s'
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
          selector: 'node.highlighted',
          style: {
            'border-color': '#06b6d4',
            'border-width': 4,
            'box-shadow': '0 0 15px rgba(6,182,212,0.6)',
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
            'transition-property': 'line-color, width',
            'transition-duration': '0.3s'
          } as any
        },
        {
          selector: 'edge.highlighted',
          style: {
            'line-color': '#06b6d4',
            'target-arrow-color': '#06b6d4',
            'width': 4,
          } as any
        }
      ],
      layout: this.getLayoutConfig(this.currentLayout()),
      minZoom: 0.3,
      maxZoom: 2.5,
      wheelSensitivity: 0.2,
      userZoomingEnabled: true,
      userPanningEnabled: true,
      autoungrabify: true,
      boxSelectionEnabled: false,
    });

    this.setupInteractions();
  }

  private setupInteractions() {
    if (!this.cy) return;

    // Click to select skill
    this.cy.on('tap', 'node', (evt: any) => {
      const skill: Skill = evt.target.data('skill');
      this.selectedSkill.set(skill);
      this.loadTasks(skill);
      this.highlightDependencies(evt.target);
    });

    // Click background to deselect
    this.cy.on('tap', (evt: any) => {
      if (evt.target === this.cy) {
        this.closePanel();
        this.clearHighlights();
      }
    });

    // Hover tooltip
    this.cy.on('mouseover', 'node', (evt: any) => {
      const skill: Skill = evt.target.data('skill');
      this.showTooltip(evt, skill);
    });

    this.cy.on('mouseout', 'node', () => {
      this.hideTooltip();
    });
  }

  private highlightDependencies(node: any) {
    if (!this.cy) return;

    this.clearHighlights();

    // Highlight all dependencies (predecessors)
    const predecessors = node.predecessors();
    predecessors.addClass('highlighted');

    // Highlight all dependents (successors)
    const successors = node.successors();
    successors.addClass('highlighted');
  }

  private clearHighlights() {
    if (!this.cy) return;
    this.cy.elements().removeClass('highlighted');
  }

private showTooltip(evt: any, skill: Skill) {
  const renderedPosition = evt.target.renderedPosition();
  
  // Get actual task info from loaded tasks if this is the selected skill
  let taskInfo = { earnedXP: 0, totalTasks: 0, completedTasks: 0 };
  
  if (this.selectedSkill()?.id === skill.id) {
    // Use loaded tasks for selected skill
    const currentTasks = this.tasks();
    taskInfo = {
      earnedXP: currentTasks
        .filter(t => t.status === 'Completed')
        .reduce((sum, t) => sum + t.experiencePoints, 0),
      totalTasks: currentTasks.length,
      completedTasks: currentTasks.filter(t => t.status === 'Completed').length
    };
  }
  
  // Get dependency names
  const dependencyNames = skill.dependsOn
    .map(depId => this.allSkills.find(s => s.id === depId)?.name)
    .filter(Boolean) as string[];
 
  // Only show tooltip if we have dependencies OR task data
  if (dependencyNames.length === 0 && taskInfo.totalTasks === 0) {
    return; // Don't show tooltip if no useful info
  }
 
  this.tooltipContent.set({
    skillName: skill.name,
    status: skill.status,
    earnedXP: taskInfo.earnedXP,
    requiredXP: skill.requiredExperiencePoints,
    taskCount: taskInfo.totalTasks,
    completedTasks: taskInfo.completedTasks,
    dependencies: dependencyNames
  });
 
  this.tooltipX.set(renderedPosition.x + 100);
  this.tooltipY.set(renderedPosition.y - 50);
  this.tooltipVisible.set(true);
}

  private hideTooltip() {
    this.tooltipVisible.set(false);
  }

  private getTaskInfo(skillId: string): { earnedXP: number; totalTasks: number; completedTasks: number } {
    const skill = this.allSkills.find(s => s.id === skillId);
    if (!skill) return { earnedXP: 0, totalTasks: 0, completedTasks: 0 };

    // This is a simplified version - in real implementation you'd fetch actual task data
    return {
      earnedXP: 0,
      totalTasks: 0,
      completedTasks: 0
    };
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
        // Update task list immediately
        this.tasks.update(tasks =>
          tasks.map(t => t.id === updatedTask.id ? updatedTask : t)
        );

        // Fetch updated skill data without recreating the graph
        this.api.getSkillById(this.goalId, currentSkill.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (updatedSkill) => {
              // Update skill in our local array
              const skillIndex = this.allSkills.findIndex(s => s.id === updatedSkill.id);
              if (skillIndex !== -1) {
                this.allSkills[skillIndex] = updatedSkill;
              }

              // Update the visual node without recreating graph
              if (this.cy) {
                const node = this.cy.$(`#${updatedSkill.id}`);
                if (node.length > 0) {
                  // Update node data
                  node.data('skill', updatedSkill);
                  node.data('status', updatedSkill.status);
                  
                  // Update visual style
                  const statusColors: Record<string, string> = {
                    Locked: '#374151',
                    Available: '#1d4ed8',
                    InProgress: '#d97706',
                    Completed: '#059669',
                  };
                  
                  node.style({
                    'background-color': statusColors[updatedSkill.status] ?? '#374151',
                    'border-color': statusColors[updatedSkill.status] ?? '#374151'
                  });

                  // If skill just completed, check for unlocked dependents
                  if (updatedSkill.status === 'Completed') {
                    this.checkAndUnlockDependents(updatedSkill.id);
                  }
                }
              }

              // Update selected skill reference
              this.selectedSkill.set(updatedSkill);
            },
            error: (err) => {
              this.errorHandler.handleHttpError(err, 'Refresh Skill');
            }
          });
      },
      error: (err) => {
        this.errorHandler.handleHttpError(err, 'Update Task');
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

  private checkAndUnlockDependents(completedSkillId: string) {
    if (!this.cy) return;

    this.allSkills.forEach(skill => {
      if (skill.status === 'Locked' && skill.dependsOn.includes(completedSkillId)) {
        // Check if ALL dependencies are now completed
        const allDepsCompleted = skill.dependsOn.every(depId => {
          const depSkill = this.allSkills.find(s => s.id === depId);
          return depSkill?.status === 'Completed';
        });

        if (allDepsCompleted) {
          // Fetch updated skill from server (it should be unlocked)
          this.api.getSkillById(this.goalId, skill.id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (updatedSkill) => {
                // Update local array
                const index = this.allSkills.findIndex(s => s.id === updatedSkill.id);
                if (index !== -1) {
                  this.allSkills[index] = updatedSkill;
                }

                // Update visual node
                const node = this.cy!.$(`#${updatedSkill.id}`);
                if (node.length > 0) {
                  node.data('skill', updatedSkill);
                  node.data('status', updatedSkill.status);
                  
                  const statusColors: Record<string, string> = {
                    Locked: '#374151',
                    Available: '#1d4ed8',
                    InProgress: '#d97706',
                    Completed: '#059669',
                  };
                  
                  // Animate the unlock
                  node.animate({
                    style: {
                      'background-color': statusColors[updatedSkill.status],
                      'border-color': statusColors[updatedSkill.status]
                    }
                  }, {
                    duration: 500,
                    easing: 'ease-out'
                  });
                }
              }
            });
        }
      }
    });
  }
}