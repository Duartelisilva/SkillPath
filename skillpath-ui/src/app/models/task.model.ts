export interface LearningTask {
  id: string;
  skillId: string;
  title: string;
  description: string;
  order: number;
  status: 'NotStarted' | 'InProgress' | 'Completed';
  createdAtUtc: string;
  updatedAtUtc: string | null;
  completedAtUtc: string | null;
  experiencePoints: number;
}