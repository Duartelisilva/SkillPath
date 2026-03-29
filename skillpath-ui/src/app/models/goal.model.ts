export interface Goal {
  id: string;
  title: string;
  description: string;
  status: string;
  skillCount: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}