import { Box, Tooltip, Typography } from "@mui/material";
import { formatDuration, formatTotalDuration } from "../../utils/formatDuration";

interface Step {
  kind: "step" | "swimStep";
  name?: string;
  intensity?: number;
  durationType?: number;
  durationValue?: number;
  distance?: number;
  targetType?: number;
  targetZone?: number;
  targetLow?: number;
  targetHigh?: number;
}

interface RepeatItem {
  kind: "repeat";
  repeatCount?: number;
  steps?: WorkoutItem[];
}

type WorkoutItem = Step | RepeatItem;

interface Schema {
  kind?: string;
  items?: WorkoutItem[];
}

const SAMPLE_FTP = 250;
const SAMPLE_MAX_HR = 185;
const SAMPLE_THRESHOLD_PACE = 4.17; // m/s (~4:00/km)
const SAMPLE_MAX_CADENCE = 100;

const POWER_ZONES = [0.475, 0.65, 0.825, 0.975, 1.125, 1.35, 1.75];
const HR_ZONES = [0.3, 0.65, 0.75, 0.85, 0.95];

const INTENSITY_COLOR: Record<number, string> = {
  0: "#2196f3", 1: "#9e9e9e", 2: "#ff9800", 3: "#ff9800", 4: "#4caf50", 5: "#f44336", 6: "#9c27b0",
};

const INTENSITY_LABEL: Record<number, string> = {
  0: "Active", 1: "Rest", 2: "Warmup", 3: "Cooldown", 4: "Recovery", 5: "Interval", 6: "Other",
};

function avg(low: number, high?: number) { return high != null ? (low + high) / 2 : low; }

function intensityFraction(step: Step): number {
  const t = step.targetType;
  const isHr = t === 1;
  const isPower = t === 4 || (t != null && t >= 7 && t <= 10);
  const isSpeed = t === 0 || t === 12;
  const isCadence = t === 3;

  if (step.targetZone != null) {
    if (isPower) return Math.min(POWER_ZONES[step.targetZone - 1] ?? 0.5, 1.5) / 1.5;
    if (isHr) return HR_ZONES[step.targetZone - 1] ?? 0.5;
  }

  if (step.targetLow != null) {
    const m = avg(step.targetLow, step.targetHigh);
    if (isPower) return Math.min(m / SAMPLE_FTP, 1.5) / 1.5;
    if (isHr) return Math.min(m / SAMPLE_MAX_HR, 1);
    if (isSpeed) return Math.min(m / (SAMPLE_THRESHOLD_PACE * 1.1), 1);
    if (isCadence) return Math.min(m / SAMPLE_MAX_CADENCE, 1);
  }

  const byIntensity: Record<number, number> = { 0: 0.5, 1: 0.05, 2: 0.3, 3: 0.3, 4: 0.2, 5: 0.85, 6: 0.5 };
  return byIntensity[step.intensity ?? 0] ?? 0.5;
}

function formatTarget(step: Step): string | null {
  if (step.targetZone != null)
    return step.targetType === 1 ? `HR Z${step.targetZone}` : `Z${step.targetZone}`;
  if (step.targetLow != null && step.targetHigh != null)
    return step.targetLow === step.targetHigh ? `${step.targetLow}` : `${step.targetLow}–${step.targetHigh}`;
  return null;
}

const MAX_HEIGHT = 40;
const MIN_HEIGHT = 4;

function StepBar({ step }: { step: Step }) {
  const color = INTENSITY_COLOR[step.intensity ?? 0] ?? "#2196f3";
  const label = INTENSITY_LABEL[step.intensity ?? 0] ?? "Active";
  const duration = step.kind === "swimStep"
    ? formatDuration(undefined, undefined, step.distance)
    : formatDuration(step.durationType, step.durationValue);
  const target = formatTarget(step);
  const height = Math.max(MIN_HEIGHT, Math.round(intensityFraction(step) * MAX_HEIGHT));

  return (
    <Tooltip title={[step.name ?? label, duration, target].filter(Boolean).join(" · ")} arrow>
      <Box sx={{ height, minWidth: 4, flex: 1, borderRadius: 0.5, bgcolor: color, opacity: 0.85, cursor: "default", alignSelf: "flex-end" }} />
    </Tooltip>
  );
}

function flattenToSteps(items: WorkoutItem[]): Step[] {
  const result: Step[] = [];
  for (const item of items) {
    if (item.kind === "repeat") {
      const inner = ((item as RepeatItem).steps ?? []).filter((s): s is Step => s.kind === "step" || s.kind === "swimStep");
      for (let i = 0; i < ((item as RepeatItem).repeatCount ?? 1); i++)
        result.push(...inner);
    } else {
      result.push(item as Step);
    }
  }
  return result;
}

function totalDurationMs(steps: Step[]): number {
  return steps.reduce((sum, s) => {
    if (s.durationType === 0 && s.durationValue != null) return sum + s.durationValue;
    return sum;
  }, 0);
}

interface WorkoutPreviewProps {
  schema?: unknown;
}

export default function WorkoutPreview({ schema }: WorkoutPreviewProps) {
  if (!schema) return null;

  let parsed: Schema;
  try {
    parsed = (typeof schema === "string" ? JSON.parse(schema) : schema) as Schema;
  } catch {
    return null;
  }

  const steps = flattenToSteps(parsed.items ?? []);
  if (steps.length === 0) return null;

  const totalLabel = formatTotalDuration(totalDurationMs(steps));

  return (
    <Box sx={{ mt: 1 }}>
      <Box sx={{ display: "flex", alignItems: "flex-end", gap: 0.5, height: MAX_HEIGHT }}>
        {steps.map((step, i) => <StepBar key={i} step={step} />)}
      </Box>
      {totalLabel && (
        <Typography variant="caption" color="text.disabled" sx={{ display: "block", mt: 0.5 }}>
          {totalLabel}
        </Typography>
      )}
    </Box>
  );
}
