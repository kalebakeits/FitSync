import { useState } from "react";
import { Box, Typography, TextField, InputAdornment, Grid, Button, Skeleton, Alert } from "@mui/material";
import { Search, Add } from "@mui/icons-material";
import { useQueryClient } from "@tanstack/react-query";
import {
  useGetApiWorkouts,
  useDeleteApiWorkoutsId,
  usePutApiWorkoutsId,
  getGetApiWorkoutsQueryKey,
} from "../../api/generated/workouts/workouts";
import type { WorkoutResponse, UpdateWorkoutRequest } from "../../api/generated/fitSyncApi.schemas";
import AppLayout from "../../components/layout/AppLayout";
import WorkoutCard from "../../components/workouts/WorkoutCard";
import EditWorkoutModal from "../../components/workouts/EditWorkoutModal";
import DeleteWorkoutModal from "../../components/workouts/DeleteWorkoutModal";
import NewWorkoutModal from "../../components/workouts/NewWorkoutModal";
import PublishWorkoutModal from "../../components/workouts/PublishWorkoutModal";

export default function WorkoutsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [newOpen, setNewOpen] = useState(false);
  const [editWorkout, setEditWorkout] = useState<WorkoutResponse | null>(null);
  const [deleteWorkout, setDeleteWorkout] = useState<WorkoutResponse | null>(null);
  const [publishWorkout, setPublishWorkout] = useState<WorkoutResponse | null>(null);

  const { data, isLoading, error } = useGetApiWorkouts({ search: search || undefined });

  const deleteMutation = useDeleteApiWorkoutsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiWorkoutsQueryKey() });
        setDeleteWorkout(null);
      },
    },
  });

  const updateMutation = usePutApiWorkoutsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiWorkoutsQueryKey() });
        setEditWorkout(null);
      },
    },
  });

  const handleSave = (name: string, tags: string[]) => {
    if (!editWorkout?.id) return;
    const request: UpdateWorkoutRequest = { name, tags };
    updateMutation.mutate({ id: editWorkout.id, data: request });
  };

  const handleDownload = (workout: WorkoutResponse) => {
    window.open(`/api/Workouts/${workout.id}/download`, "_blank");
  };

  return (
    <AppLayout>
      <Box sx={{ p: 3, width: "100%" }}>
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
          <Typography variant="h5" fontWeight="bold">
            Workout Library
          </Typography>
          <Button variant="contained" startIcon={<Add />} onClick={() => setNewOpen(true)}>
            New Workout
          </Button>
        </Box>

        <TextField
          fullWidth
          placeholder="Search workouts..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ mb: 3 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            },
          }}
        />

        {!!error && <Alert severity="error" sx={{ mb: 2 }}>Failed to load workouts.</Alert>}

        {isLoading ? (
          <Grid container spacing={2}>
            {[...Array(6)].map((_, i) => (
              <Grid key={i} size={{ xs: 12, sm: 6, md: 4 }}>
                <Skeleton variant="rectangular" height={160} sx={{ borderRadius: 1 }} />
              </Grid>
            ))}
          </Grid>
        ) : (data?.items?.length ?? 0) === 0 ? (
          <Box sx={{ textAlign: "center", py: 8 }}>
            <Typography color="text.secondary">No workouts yet. Generate one to get started.</Typography>
          </Box>
        ) : (
          <Grid container spacing={2}>
            {data?.items?.map((workout) => (
              <Grid key={workout.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <WorkoutCard
                  workout={workout}
                  onEdit={setEditWorkout}
                  onDelete={setDeleteWorkout}
                  onDownload={handleDownload}
                  onPublish={setPublishWorkout}
                />
              </Grid>
            ))}
          </Grid>
        )}

        <NewWorkoutModal open={newOpen} onClose={() => setNewOpen(false)} />

        <PublishWorkoutModal
          open={publishWorkout !== null}
          workout={publishWorkout}
          onClose={() => setPublishWorkout(null)}
        />

        <EditWorkoutModal
          workout={editWorkout}
          isPending={updateMutation.isPending}
          onSave={handleSave}
          onClose={() => setEditWorkout(null)}
        />

        <DeleteWorkoutModal
          workout={deleteWorkout}
          isPending={deleteMutation.isPending}
          onConfirm={() => deleteWorkout?.id && deleteMutation.mutate({ id: deleteWorkout.id })}
          onClose={() => setDeleteWorkout(null)}
        />
      </Box>
    </AppLayout>
  );
}
