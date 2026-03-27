import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Goal } from '../models/goal.model';
import { Skill } from '../models/skill.model';
import { LearningTask } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = 'https://localhost:7015/api';

  // Goals
  getGoals(): Observable<Goal[]> {
    return this.http.get<Goal[]>(`${this.base}/goals`);
  }

  createGoal(title: string, description: string): Observable<Goal> {
    return this.http.post<Goal>(`${this.base}/goals`, { title, description });
  }

  generateSkillTree(goalId: string, additionalContext?: string): Observable<Skill[]> {
    return this.http.post<Skill[]>(`${this.base}/goals/${goalId}/generate-skill-tree`, { additionalContext });
  }

  // Skills
  getSkills(goalId: string): Observable<Skill[]> {
    return this.http.get<Skill[]>(`${this.base}/goals/${goalId}/skills`);
  }

  // Tasks
  getTasks(goalId: string, skillId: string): Observable<LearningTask[]> {
    return this.http.get<LearningTask[]>(`${this.base}/goals/${goalId}/skills/${skillId}/tasks`);
  }

  updateTask(goalId: string, skillId: string, taskId: string, title: string, description: string): Observable<LearningTask> {
    return this.http.put<LearningTask>(`${this.base}/goals/${goalId}/skills/${skillId}/tasks/${taskId}`, { title, description });
  }
}