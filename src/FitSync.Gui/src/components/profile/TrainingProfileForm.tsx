import { useState, useEffect } from "react";
import { Stack, TextField, Typography, Button, Alert, Divider, useMediaQuery, useTheme } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import {
  useGetApiTrainingProfile,
  usePutApiTrainingProfile,
  getGetApiTrainingProfileQueryKey,
} from "../../api/generated/training-profile/training-profile";
import type { UpsertTrainingProfileRequest } from "../../api/generated/fitSyncApi.schemas";
import InfoTooltip from "../InfoTooltip";
import PaceField from "./PaceField";

export default function TrainingProfileForm() {
  const queryClient = useQueryClient();
  const { data, isLoading } = useGetApiTrainingProfile();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  const [ftpWatts, setFtpWatts] = useState("");
  const [cyclingThresholdHr, setCyclingThresholdHr] = useState("");
  const [cyclingMaxHr, setCyclingMaxHr] = useState("");
  const [runningThresholdHr, setRunningThresholdHr] = useState("");
  const [runningMaxHr, setRunningMaxHr] = useState("");
  const [runningThresholdPaceSeconds, setRunningThresholdPaceSeconds] = useState("");
  const [poolLengthMetres, setPoolLengthMetres] = useState("");
  const [swimThresholdHr, setSwimThresholdHr] = useState("");
  const [swimCssSeconds, setSwimCssSeconds] = useState("");

  useEffect(() => {
    if (!data) return;
    setFtpWatts(data.ftpWatts?.toString() ?? "");
    setCyclingThresholdHr(data.cyclingThresholdHr?.toString() ?? "");
    setCyclingMaxHr(data.cyclingMaxHr?.toString() ?? "");
    setRunningThresholdHr(data.runningThresholdHr?.toString() ?? "");
    setRunningMaxHr(data.runningMaxHr?.toString() ?? "");
    setRunningThresholdPaceSeconds(data.runningThresholdPaceSeconds?.toString() ?? "");
    setPoolLengthMetres(data.poolLengthMetres?.toString() ?? "");
    setSwimThresholdHr(data.swimThresholdHr?.toString() ?? "");
    setSwimCssSeconds(data.swimCssSeconds?.toString() ?? "");
  }, [data]);

  const mutation = usePutApiTrainingProfile({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiTrainingProfileQueryKey() });
      },
    },
  });

  const handleSave = () => {
    const request: UpsertTrainingProfileRequest = {
      ftpWatts: ftpWatts ? parseInt(ftpWatts) : null,
      cyclingThresholdHr: cyclingThresholdHr ? parseInt(cyclingThresholdHr) : null,
      cyclingMaxHr: cyclingMaxHr ? parseInt(cyclingMaxHr) : null,
      runningThresholdHr: runningThresholdHr ? parseInt(runningThresholdHr) : null,
      runningMaxHr: runningMaxHr ? parseInt(runningMaxHr) : null,
      runningThresholdPaceSeconds: runningThresholdPaceSeconds ? parseInt(runningThresholdPaceSeconds) : null,
      poolLengthMetres: poolLengthMetres ? parseFloat(poolLengthMetres) : null,
      swimThresholdHr: swimThresholdHr ? parseInt(swimThresholdHr) : null,
      swimCssSeconds: swimCssSeconds ? parseInt(swimCssSeconds) : null,
    };
    mutation.mutate({ data: request });
  };

  if (isLoading) return null;

  const rowDirection = isMobile ? "column" : "row";

  return (
    <Stack spacing={2} sx={{ width: "100%" }}>
      <Typography variant="subtitle2" color="text.secondary">Cycling</Typography>
      <Stack direction={rowDirection} spacing={2} sx={{ width: "100%" }}>
        <TextField
          label="FTP"
          value={ftpWatts}
          onChange={(e) => setFtpWatts(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Functional Threshold Power — max sustainable power for ~1 hour." />,
            },
          }}
        />
        <TextField
          label="Threshold HR"
          value={cyclingThresholdHr}
          onChange={(e) => setCyclingThresholdHr(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Heart rate at your cycling lactate threshold." />,
            },
          }}
        />
        <TextField
          label="Max HR"
          value={cyclingMaxHr}
          onChange={(e) => setCyclingMaxHr(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Maximum heart rate for cycling." />,
            },
          }}
        />
      </Stack>

      <Divider />

      <Typography variant="subtitle2" color="text.secondary">Running</Typography>
      <Stack direction={rowDirection} spacing={2} sx={{ width: "100%" }}>
        <TextField
          label="Threshold HR"
          value={runningThresholdHr}
          onChange={(e) => setRunningThresholdHr(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Heart rate at your running lactate threshold." />,
            },
          }}
        />
        <TextField
          label="Max HR"
          value={runningMaxHr}
          onChange={(e) => setRunningMaxHr(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Maximum heart rate for running." />,
            },
          }}
        />
        <PaceField
          label="Threshold Pace"
          tooltip="Your threshold pace — pace you can sustain for ~1 hour. Used to calculate pace zones."
          valueSeconds={runningThresholdPaceSeconds}
          onChangeSeconds={setRunningThresholdPaceSeconds}
        />
      </Stack>

      <Divider />

      <Typography variant="subtitle2" color="text.secondary">Swimming</Typography>
      <Stack direction={rowDirection} spacing={2} sx={{ width: "100%" }}>
        <TextField
          label="Pool length"
          value={poolLengthMetres}
          onChange={(e) => setPoolLengthMetres(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Length of your pool in metres. Used for pool swim workouts." />,
            },
          }}
        />
        <TextField
          label="Threshold HR"
          value={swimThresholdHr}
          onChange={(e) => setSwimThresholdHr(e.target.value)}
          type="number"
          size="small"
          fullWidth
          slotProps={{
            input: {
              endAdornment: <InfoTooltip title="Heart rate at your swimming lactate threshold." />,
            },
          }}
        />
        <PaceField
          label="CSS"
          tooltip="Critical Swim Speed — max sustainable pace for 400m+. Used to calculate swim pace zones."
          valueSeconds={swimCssSeconds}
          onChangeSeconds={setSwimCssSeconds}
        />
      </Stack>

      {mutation.isError && <Alert severity="error">Failed to save.</Alert>}
      {mutation.isSuccess && <Alert severity="success">Saved.</Alert>}

      <Button variant="contained" onClick={handleSave} disabled={mutation.isPending}>
        Save
      </Button>
    </Stack>
  );
}
