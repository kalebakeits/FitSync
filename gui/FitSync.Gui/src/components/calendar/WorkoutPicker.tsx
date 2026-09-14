import { useState } from "react";
import {
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  List,
  ListItemButton,
  Box,
  TextField,
  Typography,
} from "@mui/material";
import { format, parseISO } from "date-fns";
import { useQueryClient } from "@tanstack/react-query";
import SportIcon from "../workouts/SportIcon";
import WorkoutPreview from "../workouts/WorkoutPreview";
import { useGetApiWorkouts } from "../../api/generated/workouts/workouts";
import { usePostApiWorkoutsPublishWorkoutId } from "../../api/generated/workout-publishing/workout-publishing";
import { getGetApiScheduledWorkoutsQueryKey } from "../../api/generated/scheduled-workouts/scheduled-workouts";

interface WorkoutPickerProps {
  date: string;
  onClose: () => void;
}

export default function WorkoutPicker({ date, onClose }: WorkoutPickerProps) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");

  const { data, isLoading } = useGetApiWorkouts({
    search: search || undefined,
  });

  const workouts = data?.items ?? [];

  const scheduleMutation = usePostApiWorkoutsPublishWorkoutId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiScheduledWorkoutsQueryKey(),
        });
        onClose();
      },
    },
  });

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Schedule a workout</DialogTitle>
      <DialogContent
        sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}
      >
        <Typography variant="body2" color="text.secondary">
          {format(parseISO(date), "EEE d MMM yyyy")}
        </Typography>

        <TextField
          label="Search"
          size="small"
          fullWidth
          value={search}
          onChange={(changeEvent) => setSearch(changeEvent.target.value)}
        />

        {isLoading ? (
          <CircularProgress size={24} sx={{ alignSelf: "center", my: 2 }} />
        ) : workouts.length === 0 ? (
          <Typography variant="body2" color="text.disabled">
            No workouts in your library.
          </Typography>
        ) : (
          <List sx={{ maxHeight: 420, overflowY: "auto", py: 0 }}>
            {workouts.map((workout) => (
              <ListItemButton
                key={workout.id}
                disabled={scheduleMutation.isPending}
                sx={{ alignItems: "flex-start", py: 1.5, px: 1 }}
                onClick={() =>
                  scheduleMutation.mutate({
                    workoutId: workout.id ?? "",
                    data: { serviceType: null, scheduledDate: date },
                  })
                }
              >
                <Box sx={{ width: "100%", minWidth: 0 }}>
                  <Box
                    sx={{
                      display: "flex",
                      alignItems: "center",
                      gap: 1,
                      mb: 1,
                    }}
                  >
                    <SportIcon
                      sport={workout.sport}
                      size={18}
                      sx={{ color: "text.secondary", flexShrink: 0 }}
                    />
                    <Typography variant="body2" fontWeight="medium" noWrap>
                      {workout.name}
                    </Typography>
                  </Box>
                  <WorkoutPreview schema={workout.schema} />
                </Box>
              </ListItemButton>
            ))}
          </List>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
      </DialogActions>
    </Dialog>
  );
}
