import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Typography } from "@mui/material";
import type { WorkoutResponse } from "../../api/generated/fitSyncApi.schemas";

interface DeleteWorkoutModalProps {
  workout: WorkoutResponse | null;
  isPending: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteWorkoutModal({ workout, isPending, onConfirm, onClose }: DeleteWorkoutModalProps) {
  return (
    <Dialog open={!!workout} onClose={onClose}>
      <DialogTitle>Delete Workout</DialogTitle>
      <DialogContent>
        <Typography>
          Are you sure you want to delete <strong>{workout?.name}</strong>? This cannot be undone.
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button color="error" variant="contained" disabled={isPending} onClick={onConfirm}>
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}
