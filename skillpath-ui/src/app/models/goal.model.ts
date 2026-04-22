export interface Goal {
  id: string;
  title: string;
  description: string;
  status: string;
  skillCount: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  _generationStatus?: 'generating' | 'success' | 'failed' | null;
  _generationProgress?: number;
}