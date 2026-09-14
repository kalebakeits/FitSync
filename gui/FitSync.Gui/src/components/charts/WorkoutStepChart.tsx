import { useMemo, useRef } from "react";
import { Box, Typography, useTheme } from "@mui/material";
import type { EChartsCoreOption } from "echarts/core";
import type {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  CustomSeriesRenderItemReturn,
} from "echarts";
import { useECharts } from "../../hooks/useECharts";
import { useSportColour } from "../../hooks/useSportColour";
import { useTrainingTargets } from "../../hooks/useTrainingTargets";
import { formatTotalDuration } from "../../utils/formatDuration";
import { secondsToPaceDisplay } from "../../utils/paceFormat";
import {
  flattenSteps,
  parseWorkoutSchema,
  stepDurationSeconds,
  stepIntensityColour,
  stepIntensityFraction,
  stepLabel,
  stepTargetLabel,
} from "../../utils/workoutSchema";
import type { TrainingTargets, WorkoutStep } from "../../utils/workoutSchema";

interface WorkoutStepChartProps {
  schema?: unknown;
  sport?: number | null;
}

interface ResolvedStep {
  step: WorkoutStep;
  startSeconds: number;
  endSeconds: number;
  intensity: number;
  colour: string;
}

const CHART_HEIGHT = 150;
const AXIS_COLOUR = "#9e9e9e";
const STEP_GAP_PX = 1;
const Y_HEADROOM = 1.15;

function stepTooltip(step: WorkoutStep, targets: TrainingTargets): string {
  const duration = secondsToPaceDisplay(stepDurationSeconds(step, targets));
  const zone = stepTargetLabel(step);
  const intensity = Math.round(stepIntensityFraction(step, targets) * 100);
  return [stepLabel(step), duration, zone, `${intensity}%`]
    .filter(Boolean)
    .join(" · ");
}

function renderStep(
  _params: CustomSeriesRenderItemParams,
  api: CustomSeriesRenderItemAPI,
): CustomSeriesRenderItemReturn {
  const startX = api.coord([api.value(0), api.value(2)])[0];
  const endX = api.coord([api.value(1), api.value(2)])[0];
  const topY = api.coord([api.value(0), api.value(2)])[1];
  const baseY = api.coord([api.value(0), 0])[1];

  return {
    type: "rect",
    shape: {
      x: startX,
      y: topY,
      width: Math.max(endX - startX - STEP_GAP_PX, STEP_GAP_PX),
      height: Math.max(baseY - topY, STEP_GAP_PX),
    },
    style: api.style(),
  };
}

export default function WorkoutStepChart({
  schema,
  sport,
}: WorkoutStepChartProps) {
  const theme = useTheme();
  const targets = useTrainingTargets();
  const { main } = useSportColour(sport);
  const containerRef = useRef<HTMLDivElement | null>(null);

  const resolvedSteps = useMemo<ResolvedStep[]>(() => {
    let elapsed = 0;
    return flattenSteps(parseWorkoutSchema(schema)).map((step) => {
      const duration = stepDurationSeconds(step, targets);
      const resolved: ResolvedStep = {
        step,
        startSeconds: elapsed,
        endSeconds: elapsed + duration,
        intensity: stepIntensityFraction(step, targets),
        colour: stepIntensityColour(step, targets, theme),
      };
      elapsed += duration;
      return resolved;
    });
  }, [schema, targets, theme]);

  const option = useMemo<EChartsCoreOption>(() => {
    const totalSeconds = resolvedSteps.at(-1)?.endSeconds ?? 0;
    const peakIntensity = resolvedSteps.reduce(
      (peak, resolved) => Math.max(peak, resolved.intensity),
      0,
    );

    return {
      animation: false,
      grid: { left: 8, right: 8, top: 12, bottom: 8, containLabel: true },
      tooltip: {
        trigger: "item",
        formatter: (params: { dataIndex: number }) => {
          const resolved = resolvedSteps[params.dataIndex];
          return resolved ? stepTooltip(resolved.step, targets) : "";
        },
      },
      xAxis: {
        type: "value",
        min: 0,
        max: totalSeconds,
        axisLabel: {
          color: AXIS_COLOUR,
          formatter: (value: number) => formatTotalDuration(value * 1000),
        },
        axisLine: { lineStyle: { color: main } },
        axisTick: { show: false },
        splitLine: { show: false },
      },
      yAxis: {
        type: "value",
        min: 0,
        max: Math.max(peakIntensity, 1) * Y_HEADROOM,
        axisLabel: {
          color: AXIS_COLOUR,
          formatter: (value: number) => `${Math.round(value * 100)}%`,
        },
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: { lineStyle: { color: theme.palette.divider } },
      },
      series: [
        {
          type: "custom",
          renderItem: renderStep,
          encode: { x: [0, 1], y: 2 },
          data: resolvedSteps.map((resolved) => ({
            value: [
              resolved.startSeconds,
              resolved.endSeconds,
              resolved.intensity,
            ],
            itemStyle: { color: resolved.colour, borderRadius: 2 },
          })),
        },
      ],
    };
  }, [resolvedSteps, targets, theme, main]);

  useECharts(containerRef, option, [option]);

  if (resolvedSteps.length === 0) {
    return (
      <Box
        sx={{
          border: 1,
          borderColor: "divider",
          borderRadius: 1,
          p: 2,
          textAlign: "center",
        }}
      >
        <Typography variant="body2" color="text.disabled">
          No structure
        </Typography>
      </Box>
    );
  }

  return (
    <Box ref={containerRef} sx={{ width: "100%", height: CHART_HEIGHT }} />
  );
}
