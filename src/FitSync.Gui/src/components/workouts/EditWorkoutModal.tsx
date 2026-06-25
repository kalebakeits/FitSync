import { useState, useEffect } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import type { WorkoutResponse } from "../../api/generated/fitSyncApi.schemas";

interface EditWorkoutModalProps {
  workout: WorkoutResponse | null;
  isPending: boolean;
  onSave: (name: string, tags: string[]) => void;
  onClose: () => void;
}

export default function EditWorkoutModal({ workout, isPending, onSave, onClose }: EditWorkoutModalProps) {
  const [name, setName] = useState("");
  const [tags, setTags] = useState("");

  useEffect(() => {
    if (workout) {
      setName(workout.name ?? "");
      setTags((workout.tags ?? []).join(", "));
    }
  }, [workout]);

  const handleSave = () => {
    onSave(name, tags.split(",").map((t) => t.trim()).filter(Boolean));
  };

  return (
    <Dialog open={!!workout} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Edit Workout</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label="Name" value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <TextField
            label="Tags (comma separated)"
            value={tags}
            onChange={(e) => setTags(e.target.value)}
            fullWidth
            placeholder="e.g. threshold, long run, swim"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={handleSave} disabled={isPending}>
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}
