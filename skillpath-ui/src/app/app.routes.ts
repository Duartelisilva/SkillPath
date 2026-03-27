import { Routes } from '@angular/router';
import { GoalsComponent } from './components/goals/goals';
import { SkillTreeComponent } from './components/skill-tree/skill-tree';

export const routes: Routes = [
  { path: '', redirectTo: 'goals', pathMatch: 'full' },
  { path: 'goals', component: GoalsComponent },
  { path: 'goals/:goalId/skill-tree', component: SkillTreeComponent },
];