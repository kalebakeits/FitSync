import { useMemo } from "react";
import { useGetApiTrainingProfile } from "../api/generated/training-profile/training-profile";
import type { TrainingTargets } from "../utils/workoutSchema";

const FALLBACK_FTP = 250;
const FALLBACK_THRESHOLD_HR = 170;
const FALLBACK_THRESHOLD_PACE_SECONDS = 300;

export type { TrainingTargets };

export function useTrainingTargets(): TrainingTargets {
  const { data } = useGetApiTrainingProfile();

  const ftp = data?.ftpWatts ?? FALLBACK_FTP;
  const thresholdHr =
    data?.runningThresholdHr ??
    data?.cyclingThresholdHr ??
    data?.swimThresholdHr ??
    FALLBACK_THRESHOLD_HR;
  const thresholdPaceSeconds =
    data?.runningThresholdPaceSeconds ?? FALLBACK_THRESHOLD_PACE_SECONDS;

  return useMemo(
    () => ({ ftp, thresholdHr, thresholdPaceSeconds }),
    [ftp, thresholdHr, thresholdPaceSeconds],
  );
}
