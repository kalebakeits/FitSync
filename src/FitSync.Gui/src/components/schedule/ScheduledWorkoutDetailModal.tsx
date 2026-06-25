import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
  Box,
  Chip,
} from "@mui/material";
import { Delete } from "@mui/icons-material";
import type { ScheduledWorkoutResponse } from "../../api/generated/fitSyncApi.schemas";
import { useGetApiWorkoutsId } from "../../api/generated/workouts/workouts";
import {
  useDeleteApiScheduledWorkoutsId,
  getGetApiScheduledWorkoutsQueryKey,
} from "../../api/generated/scheduled-workouts/scheduled-workouts";
import { useQueryClient } from "@tanstack/react-query";
import WorkoutPreview from "../workouts/WorkoutPreview";
import SportIcon from "../workouts/SportIcon";

interface Props {
  scheduledWorkout: ScheduledWorkoutResponse | null;
  onClose: () => void;
}

export default function ScheduledWorkoutDetailModal({ scheduledWorkout, onClose }: Props) {
  const queryClient = useQueryClient();
  const open = scheduledWorkout !== null;

  const { data: workout } = useGetApiWorkoutsId(scheduledWorkout?.workoutId ?? "", {
    query: { enabled: open && !!scheduledWorkout?.workoutId },
  });

  const deleteMutation = useDeleteApiScheduledWorkoutsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiScheduledWorkoutsQueryKey() });
        onClose();
      },
    },
  });

  if (!scheduledWorkout) return null;

  const date = scheduledWorkout.scheduledDate
    ? new Date(scheduledWorkout.scheduledDate).toLocaleDateString(undefined, {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric",
      })
    : "";

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <SportIcon sport={scheduledWorkout.sport} sx={{ color: "text.secondary" }} />
          {scheduledWorkout.workoutName ?? "Workout"}
        </Box>
      </DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {date}
        </Typography>
        {scheduledWorkout.serviceType && (
          <Chip label={scheduledWorkout.serviceType} size="small" sx={{ mb: 2 }} />
        )}
        {workout && <WorkoutPreview schema={workout.schema} />}
      </DialogContent>
      <DialogActions>
        <Button
          color="error"
          startIcon={<Delete />}
          onClick={() => deleteMutation.mutate({ id: scheduledWorkout.id! })}
          disabled={deleteMutation.isPending}
        >
          Remove from calendar
        </Button>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
