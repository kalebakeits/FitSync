import { Box, Skeleton, Stack, Typography } from "@mui/material";
import { useGetApiWorkoutsId } from "../../api/generated/workouts/workouts";
import { useTrainingTargets } from "../../hooks/useTrainingTargets";
import { formatTotalDuration } from "../../utils/formatDuration";
import {
  computeWorkoutSummary,
  parseWorkoutSchema,
} from "../../utils/workoutSchema";
import WorkoutStepChart from "../charts/WorkoutStepChart";
import { publicationStatusLabel } from "../../utils/publishStatus";
import SummaryRow from "./SummaryRow";
import type { CalendarPublication } from "../../types/calendar";

export function PlannedSummary({
  schema,
  sport,
  publications,
}: {
  schema?: unknown;
  sport?: number | null;
  publications: CalendarPublication[];
}) {
  const targets = useTrainingTargets();
  const summary = computeWorkoutSummary(parseWorkoutSchema(schema), targets);

  return (
    <Stack spacing={2}>
      <Box>
        <SummaryRow
          label="Planned duration"
          value={
            formatTotalDuration(summary.totalDurationSeconds * 1000) || "—"
          }
        />
        <SummaryRow label="Planned load" value={`${summary.load}`} />
        <SummaryRow
          label="Avg intensity"
          value={
            summary.totalDurationSeconds > 0
              ? `${Math.round(summary.avgIntensity * 100)}%`
              : "—"
          }
        />
        <SummaryRow label="Steps" value={`${summary.stepCount}`} />
        {publications.length === 0 ? (
          <SummaryRow label="Device" value="Not on a device yet" />
        ) : (
          publications.map((publication) => (
            <SummaryRow
              key={publication.serviceType}
              label={publication.serviceType}
              value={publicationStatusLabel(publication.status)}
            />
          ))
        )}
      </Box>
      <WorkoutStepChart schema={schema} sport={sport ?? null} />
    </Stack>
  );
}

interface WorkoutDrawerBodyProps {
  workoutId: string | null;
  sport?: number | null;
  publications: CalendarPublication[];
}

export default function WorkoutDrawerBody({
  workoutId,
  sport,
  publications,
}: WorkoutDrawerBodyProps) {
  const { data, isPending } = useGetApiWorkoutsId(workoutId ?? "", {
    query: { enabled: Boolean(workoutId) },
  });

  if (!workoutId) {
    return (
      <Typography variant="body2" color="text.secondary">
        Workout structure unavailable.
      </Typography>
    );
  }

  if (isPending) {
    return (
      <Stack spacing={1}>
        <Skeleton variant="text" width="60%" />
        <Skeleton variant="text" width="40%" />
        <Skeleton variant="rounded" height={120} />
      </Stack>
    );
  }

  if (!data) {
    return (
      <Typography variant="body2" color="text.secondary">
        Workout structure unavailable.
      </Typography>
    );
  }

  return (
    <PlannedSummary
      schema={data.schema}
      sport={sport}
      publications={publications}
    />
  );
}
