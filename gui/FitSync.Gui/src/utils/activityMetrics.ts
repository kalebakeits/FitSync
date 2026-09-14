import { formatDistanceMetres, formatTotalDuration } from "./formatDuration";
import type { CalendarEventData } from "../types/calendar";

export interface ActivityMetrics {
  duration: string;
  distance: string;
  heartRate: string;
  power: string;
}

export function activityMetrics(event: CalendarEventData): ActivityMetrics {
  return {
    duration:
      event.durationSeconds != null
        ? formatTotalDuration(event.durationSeconds * 1000) || "—"
        : "—",
    distance:
      event.distanceMeters != null
        ? formatDistanceMetres(event.distanceMeters)
        : "—",
    heartRate: event.avgHeartRate != null ? `${event.avgHeartRate} bpm` : "—",
    power: event.avgPower != null ? `${event.avgPower} W` : "—",
  };
}
