export interface Activity {
  id: string;
  externalActivityId: string;
  source: string;
  status: number;
  originalFileName?: string;
  activityDate: string;
  activityName?: string;
  lastError?: string;
  createdAt: string;
  updatedAt: string;
}
