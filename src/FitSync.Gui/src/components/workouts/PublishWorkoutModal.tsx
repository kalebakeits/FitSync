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
import type { WorkoutResponse } from "../../api/generated/fitSyncApi.schemas";
import { useGetApiConnections } from "../../api/generated/connections/connections";
import { usePostApiWorkoutsPublishWorkoutId } from "../../api/generated/workout-publishing/workout-publishing";

const PUBLISHING_SUPPORTED = new Set(["Wahoo", "Garmin"]);

function toLocalDateString(date: Date): string {
  return date.toLocaleDateString("en-CA");
}

interface PublishWorkoutModalProps {
  open: boolean;
  workout: WorkoutResponse | null;
  onClose: () => void;
}

export default function PublishWorkoutModal({ open, workout, onClose }: PublishWorkoutModalProps) {
  const [selected, setSelected] = useState("");
  const [scheduledDate, setScheduledDate] = useState(toLocalDateString(new Date()));

  const { data: connections = [] } = useGetApiConnections();

  const publishableDestinations = connections.filter(
    (c) => c.connected && c.enabled && PUBLISHING_SUPPORTED.has(c.serviceType ?? ""),
  );

  const publishMutation = usePostApiWorkoutsPublishWorkoutId({
    mutation: {
      onSuccess: () => {
        setSelected("");
        setScheduledDate(toLocalDateString(new Date()));
        onClose();
      },
    },
  });

  const handleClose = () => {
    setSelected("");
    setScheduledDate(toLocalDateString(new Date()));
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle>Publish workout</DialogTitle>
      <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
        {publishableDestinations.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No supported destinations connected. Connect Wahoo or Garmin in the Integrations tab.
          </Typography>
        ) : (
          <>
            <FormControl fullWidth size="small">
              <InputLabel>Destination</InputLabel>
              <Select
                value={selected}
                label="Destination"
                onChange={(e) => setSelected(e.target.value)}
              >
                {publishableDestinations.map((c) => (
                  <MenuItem key={c.serviceType} value={c.serviceType ?? ""}>
                    {c.serviceType}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Scheduled date"
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
          disabled={!selected || !scheduledDate || publishMutation.isPending}
          onClick={() =>
            publishMutation.mutate({
              workoutId: workout?.id ?? "",
              data: { serviceType: selected, scheduledDate },
            })
          }
        >
          Publish
        </Button>
      </DialogActions>
    </Dialog>
  );
}
