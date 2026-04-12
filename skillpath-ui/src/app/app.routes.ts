import { Routes } from '@angular/router';
import { GoalsComponent } from './components/goals/goals';
import { SkillTreeComponent } from './components/skill-tree/skill-tree';
import { ComponentShowcaseComponent } from './pages/showcase/component-showcase';

export const routes: Routes = [
  { path: '', redirectTo: 'goals', pathMatch: 'full' },
  { path: 'goals', component: GoalsComponent },
  { path: 'goals/:goalId/skill-tree', component: SkillTreeComponent },
  { path: 'showcase', component: ComponentShowcaseComponent },
];