export interface Goal {
  id: string;
  title: string;
  description: string;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}