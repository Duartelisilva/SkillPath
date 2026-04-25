import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Goal } from '../models/goal.model';
import { Skill } from '../models/skill.model';
import { LearningTask } from '../models/task.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // Goals
  getGoals(): Observable<Goal[]> {
    return this.http.get<Goal[]>(`${this.baseUrl}/goals`);
  }

  getGoalById(goalId: string): Observable<Goal> {
    return this.http.get<Goal>(`${this.baseUrl}/goals/${goalId}`);
  }

  createGoal(title: string, description: string): Observable<Goal> {
    return this.http.post<Goal>(`${this.baseUrl}/goals`, { title, description });
  }

  updateGoal(goalId: string, title: string, description: string): Observable<Goal> {
    return this.http.put<Goal>(`${this.baseUrl}/goals/${goalId}`, { title, description });
  }

  deleteGoal(goalId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/goals/${goalId}`);
  }

  generateSkillTree(goalId: string, additionalContext?: string): Observable<Skill[]> {
    // Custom header to handle errors in component instead of interceptor
    const headers = new HttpHeaders().set('X-Skip-Error-Interceptor', 'true');
    return this.http.post<Skill[]>(
      `${this.baseUrl}/goals/${goalId}/generate-skill-tree`, 
      { additionalContext },
      { headers }
    );
  }

  // Skills
  getSkills(goalId: string): Observable<Skill[]> {
    return this.http.get<Skill[]>(`${this.baseUrl}/goals/${goalId}/skills`);
  }

  getSkillById(goalId: string, skillId: string): Observable<Skill> {
    return this.http.get<Skill>(`${this.baseUrl}/goals/${goalId}/skills/${skillId}`);
  }

  // Tasks
  getTasks(goalId: string, skillId: string): Observable<LearningTask[]> {
    return this.http.get<LearningTask[]>(
      `${this.baseUrl}/goals/${goalId}/skills/${skillId}/tasks`
    );
  }

  updateTask(
    goalId: string, 
    skillId: string, 
    taskId: string, 
    title: string, 
    description: string
  ): Observable<LearningTask> {
    return this.http.put<LearningTask>(
      `${this.baseUrl}/goals/${goalId}/skills/${skillId}/tasks/${taskId}`, 
      { title, description }
    );
  }

  updateTaskStatus(
    goalId: string, 
    skillId: string, 
    taskId: string, 
    status: string
  ): Observable<LearningTask> {
    return this.http.patch<LearningTask>(
      `${this.baseUrl}/goals/${goalId}/skills/${skillId}/tasks/${taskId}/status`, 
      { status }
    );
  }

  regenerateTasksForSkill(goalId: string, skillId: string): Observable<LearningTask[]> {
    // Custom header to handle errors in component
    const headers = new HttpHeaders().set('X-Skip-Error-Interceptor', 'true');
    return this.http.post<LearningTask[]>(
      `${this.baseUrl}/goals/${goalId}/skills/${skillId}/regenerate-tasks`, 
      {},
      { headers }
    );
  }
}