import { useMemo } from "react";
import { useGetApiActivities } from "../../api/generated/activities/activities";
import { useGetApiScheduledWorkouts } from "../../api/generated/scheduled-workouts/scheduled-workouts";
import type { CalendarRange } from "./useCalendarRange";
import type { CalendarEventData } from "../../types/calendar";

export function useCalendarItems({ from, to }: CalendarRange) {
  const scheduledQuery = useGetApiScheduledWorkouts({ from, to });
  const activitiesQuery = useGetApiActivities({ from, to });

  const events = useMemo<CalendarEventData[]>(() => {
    const scheduled = (scheduledQuery.data ?? [])
      .filter((workout) => !!workout.id && !!workout.scheduledDate)
      .map<CalendarEventData>((workout) => ({
        kind: "scheduled",
        id: workout.id!,
        workoutId: workout.workoutId ?? null,
        sport: workout.sport ?? null,
        title: workout.workoutName ?? "Workout",
        date: workout.scheduledDate!,
        plannedDurationSeconds: workout.plannedDurationSeconds ?? null,
        durationSeconds: null,
        distanceMeters: null,
        avgHeartRate: null,
        avgPower: null,
        linkedActivityId: workout.linkedActivityId ?? null,
        scheduledWorkoutId: null,
        publications:
          workout.publications
            ?.filter(
              (publication) =>
                !!publication.serviceType && !!publication.status,
            )
            .map((publication) => ({
              serviceType: publication.serviceType!,
              status: publication.status!,
            })) ?? [],
      }));

    const activities = (activitiesQuery.data?.items ?? [])
      .filter(
        (activity) =>
          !!activity.id &&
          !!activity.activityDate &&
          !activity.scheduledWorkoutId,
      )
      .map<CalendarEventData>((activity) => ({
        kind: "activity",
        id: activity.id!,
        workoutId: null,
        sport: activity.sport ?? null,
        title: activity.activityName ?? activity.originalFileName ?? "Activity",
        date: activity.activityDate!,
        plannedDurationSeconds: null,
        durationSeconds: activity.durationSeconds ?? null,
        distanceMeters: activity.distanceMeters ?? null,
        avgHeartRate: activity.avgHeartRate ?? null,
        avgPower: activity.avgPower ?? null,
        linkedActivityId: null,
        scheduledWorkoutId: activity.scheduledWorkoutId ?? null,
        publications: [],
      }));

    return [...scheduled, ...activities];
  }, [scheduledQuery.data, activitiesQuery.data]);

  return {
    events,
    isLoading: scheduledQuery.isLoading || activitiesQuery.isLoading,
    isError: scheduledQuery.isError || activitiesQuery.isError,
  };
}
