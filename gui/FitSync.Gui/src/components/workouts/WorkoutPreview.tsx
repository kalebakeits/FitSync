import { Box, Tooltip, Typography, useTheme } from "@mui/material";
import {
  formatDuration,
  formatTotalDuration,
} from "../../utils/formatDuration";
import { useTrainingTargets } from "../../hooks/useTrainingTargets";
import {
  computeWorkoutSummary,
  flattenSteps,
  parseWorkoutSchema,
  stepIntensityColour,
  stepIntensityFraction,
  stepLabel,
  stepTargetLabel,
} from "../../utils/workoutSchema";
import type { TrainingTargets, WorkoutStep } from "../../utils/workoutSchema";
import type { Theme } from "@mui/material/styles";

const MAX_HEIGHT = 40;
const MIN_HEIGHT = 4;

function StepBar({
  step,
  targets,
  theme,
}: {
  step: WorkoutStep;
  targets: TrainingTargets;
  theme: Theme;
}) {
  const duration =
    step.kind === "swimStep"
      ? formatDuration(undefined, undefined, step.distance)
      : formatDuration(step.durationType, step.durationValue);
  const height = Math.max(
    MIN_HEIGHT,
    Math.round(stepIntensityFraction(step, targets) * MAX_HEIGHT),
  );

  return (
    <Tooltip
      title={[stepLabel(step), duration, stepTargetLabel(step)]
        .filter(Boolean)
        .join(" · ")}
      arrow
    >
      <Box
        sx={{
          height,
          minWidth: 4,
          flex: 1,
          borderRadius: 0.5,
          bgcolor: stepIntensityColour(step, targets, theme),
          opacity: 0.85,
          cursor: "default",
          alignSelf: "flex-end",
        }}
      />
    </Tooltip>
  );
}

interface WorkoutPreviewProps {
  schema?: unknown;
}

export default function WorkoutPreview({ schema }: WorkoutPreviewProps) {
  const theme = useTheme();
  const targets = useTrainingTargets();

  const parsed = parseWorkoutSchema(schema);
  const steps = flattenSteps(parsed);
  if (steps.length === 0) return null;

  const totalLabel = formatTotalDuration(
    computeWorkoutSummary(parsed, targets).totalDurationSeconds * 1000,
  );

  return (
    <Box sx={{ mt: 1 }}>
      <Box
        sx={{
          display: "flex",
          alignItems: "flex-end",
          gap: 0.5,
          height: MAX_HEIGHT,
        }}
      >
        {steps.map((step, index) => (
          <StepBar key={index} step={step} targets={targets} theme={theme} />
        ))}
      </Box>
      {totalLabel && (
        <Typography
          variant="caption"
          color="text.disabled"
          sx={{ display: "block", mt: 0.5 }}
        >
          {totalLabel}
        </Typography>
      )}
    </Box>
  );
}
