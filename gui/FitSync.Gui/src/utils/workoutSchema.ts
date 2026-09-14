import type { Theme } from "@mui/material/styles";

export interface TrainingTargets {
  ftp: number;
  thresholdHr: number;
  thresholdPaceSeconds: number;
}

export interface WorkoutStep {
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

export interface WorkoutRepeat {
  kind: "repeat";
  repeatCount?: number;
  steps?: WorkoutItem[];
}

export type WorkoutItem = WorkoutStep | WorkoutRepeat;

export interface WorkoutSchema {
  kind?: string;
  items?: WorkoutItem[];
}

export interface WorkoutSummary {
  totalDurationSeconds: number;
  stepCount: number;
  avgIntensity: number;
  load: number;
}

const TIME_DURATION = 0;
const DISTANCE_DURATION = 1;
const CENTIMETRES_PER_METRE = 100;
const SECONDS_PER_HOUR = 3600;

const POWER_ZONES = [0.475, 0.65, 0.825, 0.975, 1.125, 1.35, 1.75];
const POWER_CEILING = 1.5;
const HR_ZONES = [0.3, 0.65, 0.75, 0.85, 0.95];

const INTENSITY_FRACTIONS = [0.5, 0.05, 0.3, 0.3, 0.2, 0.85, 0.5];
const INTENSITY_LABELS = [
  "Active",
  "Rest",
  "Warmup",
  "Cooldown",
  "Recovery",
  "Interval",
  "Other",
];

const BAND_WIDTH = 0.2;
const RAMP_LIGHT = ["#2196f3", "#4caf50", "#ffb300", "#fb8c00", "#e53935"];
const RAMP_DARK = ["#64b5f6", "#66bb6a", "#ffd54f", "#ffa726", "#ef5350"];

function midpoint(low: number, high?: number): number {
  return high != null ? (low + high) / 2 : low;
}

function secondsPerMetre(targets: TrainingTargets): number {
  return targets.thresholdPaceSeconds / 1000;
}

export function parseWorkoutSchema(schema: unknown): WorkoutSchema | null {
  let value: unknown = schema;
  if (typeof value === "string") {
    try {
      value = JSON.parse(value);
    } catch {
      return null;
    }
  }
  if (typeof value !== "object" || value === null) return null;
  return value as WorkoutSchema;
}

export function flattenSteps(schema: WorkoutSchema | null): WorkoutStep[] {
  const result: WorkoutStep[] = [];
  for (const item of schema?.items ?? []) {
    if (item.kind !== "repeat") {
      result.push(item);
      continue;
    }
    const inner = (item.steps ?? []).filter(
      (child): child is WorkoutStep =>
        child.kind === "step" || child.kind === "swimStep",
    );
    for (let round = 0; round < (item.repeatCount ?? 1); round++) {
      result.push(...inner);
    }
  }
  return result;
}

export function stepIntensityFraction(
  step: WorkoutStep,
  targets: TrainingTargets,
): number {
  const targetType = step.targetType;
  const isHr = targetType === 1;
  const isPower =
    targetType === 4 ||
    (targetType != null && targetType >= 7 && targetType <= 10);
  const isSpeed = targetType === 0 || targetType === 12;

  if (step.targetZone != null) {
    if (isPower) {
      const zone = POWER_ZONES[step.targetZone - 1] ?? 0.5;
      return Math.min(zone, POWER_CEILING) / POWER_CEILING;
    }
    if (isHr) return HR_ZONES[step.targetZone - 1] ?? 0.5;
  }
  if (step.targetLow != null) {
    const value = midpoint(step.targetLow, step.targetHigh);
    if (isPower) {
      return Math.min(value / targets.ftp, POWER_CEILING) / POWER_CEILING;
    }
    if (isHr) return Math.min(value / targets.thresholdHr, 1);
    if (isSpeed) return Math.min(value / (secondsPerMetre(targets) * 1.1), 1);
  }
  return INTENSITY_FRACTIONS[step.intensity ?? 0] ?? 0.5;
}

export function stepDurationSeconds(
  step: WorkoutStep,
  targets: TrainingTargets,
): number {
  if (step.kind === "swimStep") {
    return (step.distance ?? 0) * secondsPerMetre(targets);
  }
  if (step.durationValue == null) return 0;
  if (step.durationType === TIME_DURATION) return step.durationValue / 1000;
  if (step.durationType === DISTANCE_DURATION) {
    return (
      (step.durationValue / CENTIMETRES_PER_METRE) * secondsPerMetre(targets)
    );
  }
  return 0;
}

export function stepIntensityColour(
  step: WorkoutStep,
  targets: TrainingTargets,
  theme: Theme,
): string {
  const fraction = stepIntensityFraction(step, targets);
  const ramp = theme.palette.mode === "dark" ? RAMP_DARK : RAMP_LIGHT;
  return ramp[Math.min(Math.floor(fraction / BAND_WIDTH), ramp.length - 1)];
}

export function stepTargetLabel(step: WorkoutStep): string | null {
  if (step.targetZone != null) {
    return step.targetType === 1
      ? `HR Z${step.targetZone}`
      : `Z${step.targetZone}`;
  }
  if (step.targetLow != null && step.targetHigh != null) {
    return step.targetLow === step.targetHigh
      ? `${step.targetLow}`
      : `${step.targetLow}–${step.targetHigh}`;
  }
  return null;
}

export function stepLabel(step: WorkoutStep): string {
  if (step.name) return step.name;
  return INTENSITY_LABELS[step.intensity ?? 0] ?? "Active";
}

export function computeWorkoutSummary(
  schema: WorkoutSchema | null,
  targets: TrainingTargets,
): WorkoutSummary {
  const steps = flattenSteps(schema);
  let totalDurationSeconds = 0;
  let weightedIntensity = 0;
  let load = 0;

  for (const step of steps) {
    const seconds = stepDurationSeconds(step, targets);
    const fraction = stepIntensityFraction(step, targets);
    totalDurationSeconds += seconds;
    weightedIntensity += seconds * fraction;
    load += (seconds / SECONDS_PER_HOUR) * fraction * fraction * 100;
  }

  return {
    totalDurationSeconds,
    stepCount: steps.length,
    avgIntensity:
      totalDurationSeconds > 0 ? weightedIntensity / totalDurationSeconds : 0,
    load: Math.round(load),
  };
}
