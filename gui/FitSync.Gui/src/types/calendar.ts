export type PublicationState = "Pending" | "Success" | "Failed";

export interface CalendarPublication {
  serviceType: string;
  status: PublicationState;
}

export interface CalendarEventData {
  kind: "scheduled" | "activity";
  id: string;
  workoutId: string | null;
  sport: number | null;
  title: string;
  date: string;
  plannedDurationSeconds: number | null;
  durationSeconds: number | null;
  distanceMeters: number | null;
  avgHeartRate: number | null;
  avgPower: number | null;
  linkedActivityId: string | null;
  scheduledWorkoutId: string | null;
  publications: CalendarPublication[];
}
