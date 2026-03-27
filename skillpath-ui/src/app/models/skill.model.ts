export interface Skill {
  id: string;
  goalId: string;
  name: string;
  description: string;
  order: number;
  status: 'Locked' | 'Available' | 'InProgress' | 'Completed';
  createdAtUtc: string;
  updatedAtUtc: string | null;
  dependsOn: string[];
}