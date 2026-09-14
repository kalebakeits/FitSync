import { Box, Skeleton, Stack, Typography } from "@mui/material";
import { useGetApiActivitiesId } from "../../api/generated/activities/activities";
import { useGetApiWorkoutsId } from "../../api/generated/workouts/workouts";
import { useTrainingTargets } from "../../hooks/useTrainingTargets";
import { formatTotalDuration } from "../../utils/formatDuration";
import {
  computeWorkoutSummary,
  parseWorkoutSchema,
} from "../../utils/workoutSchema";
import WorkoutStepChart from "../charts/WorkoutStepChart";
import { activityMetrics } from "../../utils/activityMetrics";
import type { CalendarEventData } from "../../types/calendar";

function ComparisonRow({
  label,
  planned,
  actual,
}: {
  label: string;
  planned: string;
  actual: string;
}) {
  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: "1fr 1fr 1fr",
        gap: 1,
        py: 0.5,
      }}
    >
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" fontWeight="medium" textAlign="right">
        {planned}
      </Typography>
      <Typography variant="body2" fontWeight="medium" textAlign="right">
        {actual}
      </Typography>
    </Box>
  );
}

export default function LinkedDrawerBody({
  event,
}: {
  event: CalendarEventData;
}) {
  const targets = useTrainingTargets();
  const { data: workout, isPending: workoutPending } = useGetApiWorkoutsId(
    event.workoutId ?? "",
    { query: { enabled: Boolean(event.workoutId) } },
  );
  const { data: activity } = useGetApiActivitiesId(
    event.linkedActivityId ?? "",
    {
      query: { enabled: Boolean(event.linkedActivityId) },
    },
  );

  const summary = computeWorkoutSummary(
    parseWorkoutSchema(workout?.schema),
    targets,
  );
  const actual = activityMetrics(event);

  const plannedDuration =
    formatTotalDuration(summary.totalDurationSeconds * 1000) || "—";
  const plannedIntensity =
    summary.totalDurationSeconds > 0
      ? `${Math.round(summary.avgIntensity * 100)}%`
      : "—";

  return (
    <Stack spacing={2}>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: "1fr 1fr 1fr",
          gap: 1,
          pb: 0.5,
        }}
      >
        <Typography variant="caption" color="text.secondary">
          Metric
        </Typography>
        <Typography variant="caption" color="text.secondary" textAlign="right">
          Planned
        </Typography>
        <Typography variant="caption" color="text.secondary" textAlign="right">
          Completed
        </Typography>
      </Box>
      <ComparisonRow
        label="Duration"
        planned={plannedDuration}
        actual={actual.duration}
      />
      <ComparisonRow label="Distance" planned="—" actual={actual.distance} />
      <ComparisonRow label="Load" planned={`${summary.load}`} actual="—" />
      <ComparisonRow
        label="Avg intensity"
        planned={plannedIntensity}
        actual="—"
      />
      <ComparisonRow label="Avg HR" planned="—" actual={actual.heartRate} />
      <ComparisonRow label="Avg power" planned="—" actual={actual.power} />

      {workoutPending ? (
        <Skeleton variant="rounded" height={120} />
      ) : (
        <WorkoutStepChart schema={workout?.schema} sport={event.sport} />
      )}

      {activity && (
        <Typography variant="caption" color="text.disabled">
          Linked from {activity.source ?? "device"}
        </Typography>
      )}
    </Stack>
  );
}
