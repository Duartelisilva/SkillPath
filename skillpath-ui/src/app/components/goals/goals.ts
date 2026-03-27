import { Component, OnInit, signal } from '@angular/core';
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
      }
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
export class GoalsComponent implements OnInit {
  goals = signal<Goal[]>([]);
  loading = signal(true);
  generating = signal<string | null>(null);
  showModal = false;
  newTitle = '';
  newDescription = '';

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.api.getGoals().subscribe({
      next: goals => {
        this.goals.set(goals);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getSkillCount(goal: Goal): number {
    return 0; // will come from API later
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
    this.generating.set(goal.id);

    this.api.generateSkillTree(goal.id).subscribe({
      next: () => {
        this.generating.set(null);
        this.router.navigate(['/goals', goal.id, 'skill-tree']);
      },
      error: () => this.generating.set(null)
    });
  }
}