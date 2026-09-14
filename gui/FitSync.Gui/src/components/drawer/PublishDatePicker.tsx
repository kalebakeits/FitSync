import { useState } from "react";
import {
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import { getGetApiScheduledWorkoutsQueryKey } from "../../api/generated/scheduled-workouts/scheduled-workouts";
import { usePostApiWorkoutsPublishWorkoutId } from "../../api/generated/workout-publishing/workout-publishing";
import { usePublishableDestinations } from "../../hooks/usePublishableDestinations";

interface PublishDatePickerProps {
  workoutId: string | null;
  scheduledWorkoutId: string;
  defaultDate: string;
  onClose: () => void;
}

export default function PublishDatePicker({
  workoutId,
  scheduledWorkoutId,
  defaultDate,
  onClose,
}: PublishDatePickerProps) {
  const queryClient = useQueryClient();
  const [serviceType, setServiceType] = useState("");
  const [scheduledDate, setScheduledDate] = useState(defaultDate);

  const destinations = usePublishableDestinations();

  const publishMutation = usePostApiWorkoutsPublishWorkoutId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiScheduledWorkoutsQueryKey(),
        });
        onClose();
      },
    },
  });

  if (destinations.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        No devices connected yet.
      </Typography>
    );
  }

  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      spacing={1}
      alignItems={{ sm: "center" }}
    >
      <TextField
        label="Date"
        type="date"
        size="small"
        value={scheduledDate}
        onChange={(changeEvent) => setScheduledDate(changeEvent.target.value)}
        slotProps={{ inputLabel: { shrink: true } }}
        sx={{ flex: 1, minWidth: 150 }}
      />
      <FormControl size="small" sx={{ flex: 1, minWidth: 140 }}>
        <InputLabel>Destination</InputLabel>
        <Select
          value={serviceType}
          label="Destination"
          onChange={(changeEvent) => setServiceType(changeEvent.target.value)}
        >
          {destinations.map((destination) => (
            <MenuItem
              key={destination.serviceType}
              value={destination.serviceType ?? ""}
            >
              {destination.serviceType}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <Button
        variant="contained"
        disabled={
          !workoutId ||
          !serviceType ||
          !scheduledDate ||
          publishMutation.isPending
        }
        onClick={() =>
          publishMutation.mutate({
            workoutId: workoutId ?? "",
            data: { serviceType, scheduledDate, scheduledWorkoutId },
          })
        }
      >
        Publish
      </Button>
    </Stack>
  );
}
