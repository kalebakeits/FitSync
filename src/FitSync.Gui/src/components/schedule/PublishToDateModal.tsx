import { useState } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import { useGetApiWorkouts } from "../../api/generated/workouts/workouts";
import { usePostApiWorkoutsPublishWorkoutId } from "../../api/generated/workout-publishing/workout-publishing";
import { useGetApiConnections } from "../../api/generated/connections/connections";
import { getGetApiScheduledWorkoutsQueryKey } from "../../api/generated/scheduled-workouts/scheduled-workouts";

const PUBLISHING_SUPPORTED = new Set(["Wahoo", "Garmin"]);

interface Props {
  open: boolean;
  initialDate: string;
  onClose: () => void;
}

export default function PublishToDateModal({ open, initialDate, onClose }: Props) {
  const queryClient = useQueryClient();
  const [workoutId, setWorkoutId] = useState("");
  const [serviceType, setServiceType] = useState("");
  const [scheduledDate, setScheduledDate] = useState(initialDate);

  const { data: workoutsData } = useGetApiWorkouts({ limit: 200, offset: 0 }, {
    query: { enabled: open },
  });
  const workouts = workoutsData?.items ?? [];

  const { data: connections = [] } = useGetApiConnections({ query: { enabled: open } });
  const destinations = connections.filter(
    (c) => c.connected && c.enabled && PUBLISHING_SUPPORTED.has(c.serviceType ?? "")
  );

  const publishMutation = usePostApiWorkoutsPublishWorkoutId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiScheduledWorkoutsQueryKey() });
        handleClose();
      },
    },
  });

  const handleClose = () => {
    setWorkoutId("");
    setServiceType("");
    setScheduledDate(initialDate);
    publishMutation.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle>Schedule workout</DialogTitle>
      <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
        {destinations.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No supported destinations connected. Connect Wahoo or Garmin in the Integrations tab.
          </Typography>
        ) : (
          <>
            <FormControl fullWidth size="small">
              <InputLabel>Workout</InputLabel>
              <Select value={workoutId} label="Workout" onChange={(e) => setWorkoutId(e.target.value)}>
                {workouts.map((w) => (
                  <MenuItem key={w.id} value={w.id ?? ""}>{w.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl fullWidth size="small">
              <InputLabel>Destination</InputLabel>
              <Select value={serviceType} label="Destination" onChange={(e) => setServiceType(e.target.value)}>
                {destinations.map((c) => (
                  <MenuItem key={c.serviceType} value={c.serviceType ?? ""}>{c.serviceType}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Date"
              type="date"
              size="small"
              fullWidth
              value={scheduledDate}
              onChange={(e) => setScheduledDate(e.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!workoutId || !serviceType || !scheduledDate || publishMutation.isPending}
          onClick={() => publishMutation.mutate({ workoutId, data: { serviceType, scheduledDate } })}
        >
          Schedule
        </Button>
      </DialogActions>
    </Dialog>
  );
}
