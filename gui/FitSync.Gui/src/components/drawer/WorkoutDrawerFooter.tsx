import { useState } from "react";
import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  Stack,
  Typography,
} from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import { getGetApiActivitiesQueryKey } from "../../api/generated/activities/activities";
import { useDeleteApiActivitiesId } from "../../api/generated/activities/activities";
import { getGetApiScheduledWorkoutsQueryKey } from "../../api/generated/scheduled-workouts/scheduled-workouts";
import { useDeleteApiScheduledWorkoutsId } from "../../api/generated/scheduled-workouts/scheduled-workouts";
import ConfirmModal from "../ConfirmModal";
import DeleteOutcomeAlert from "./DeleteOutcomeAlert";
import PublishDatePicker from "./PublishDatePicker";
import {
  describeDeleteOutcome,
  forceDeleteWarning,
} from "../../utils/deleteOutcome";
import type { DeleteOutcomeNotice } from "../../utils/deleteOutcome";
import type { CalendarEventData } from "../../types/calendar";

interface WorkoutDrawerFooterProps {
  event: CalendarEventData;
  onClose: () => void;
}

export default function WorkoutDrawerFooter({
  event,
  onClose,
}: WorkoutDrawerFooterProps) {
  const queryClient = useQueryClient();
  const [confirming, setConfirming] = useState(false);
  const [force, setForce] = useState(false);
  const [notice, setNotice] = useState<DeleteOutcomeNotice | null>(null);

  const removeScheduled = useDeleteApiScheduledWorkoutsId({
    mutation: {
      onSuccess: (response) => {
        queryClient.invalidateQueries({
          queryKey: getGetApiScheduledWorkoutsQueryKey(),
        });
        const outcome = describeDeleteOutcome(response);
        if (!outcome) {
          onClose();
          return;
        }
        setNotice(outcome);
        if (outcome.closeDrawer) onClose();
      },
    },
  });

  const removeActivity = useDeleteApiActivitiesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiActivitiesQueryKey(),
        });
        onClose();
      },
    },
  });

  const isScheduled = event.kind === "scheduled";
  const publishedServiceTypes = event.publications.map((p) => p.serviceType);
  const canForce = isScheduled && publishedServiceTypes.length > 0;

  const openConfirm = () => {
    setForce(false);
    setNotice(null);
    setConfirming(true);
  };

  const closeConfirm = () => {
    setForce(false);
    setConfirming(false);
  };

  const handleConfirm = () => {
    closeConfirm();
    if (isScheduled) {
      removeScheduled.mutate({ id: event.id, params: { force } });
      return;
    }
    removeActivity.mutate({ id: event.id });
  };

  return (
    <Box
      sx={{
        px: 3,
        py: 2,
        borderTop: 1,
        borderColor: "divider",
        bgcolor: "background.paper",
      }}
    >
      {notice && <DeleteOutcomeAlert notice={notice} />}

      {isScheduled ? (
        <Stack spacing={1.5}>
          <PublishDatePicker
            workoutId={event.workoutId}
            scheduledWorkoutId={event.id}
            defaultDate={event.date}
            onClose={onClose}
          />
          <Button
            size="small"
            color="error"
            onClick={openConfirm}
            sx={{ alignSelf: "flex-start" }}
          >
            Remove from calendar
          </Button>
        </Stack>
      ) : (
        <Button
          size="small"
          color="error"
          onClick={openConfirm}
          sx={{ alignSelf: "flex-start" }}
        >
          Delete activity
        </Button>
      )}

      <ConfirmModal
        open={confirming}
        title={isScheduled ? "Remove scheduled workout" : "Delete activity"}
        message={
          isScheduled
            ? `Remove "${event.title}" from the calendar?`
            : `Permanently delete "${event.title}" and its uploaded file?`
        }
        confirmLabel={isScheduled ? "Remove" : "Delete"}
        severity="error"
        onConfirm={handleConfirm}
        onClose={closeConfirm}
      >
        {canForce && (
          <Box sx={{ mt: 2 }}>
            <FormControlLabel
              sx={{ alignItems: "flex-start", ml: 0 }}
              control={
                <Checkbox
                  size="small"
                  checked={force}
                  onChange={(e) => setForce(e.target.checked)}
                />
              }
              label={
                <Typography variant="body2">
                  Remove from my calendar anyway
                </Typography>
              }
            />
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{ display: "block" }}
            >
              {forceDeleteWarning(publishedServiceTypes)}
            </Typography>
          </Box>
        )}
      </ConfirmModal>
    </Box>
  );
}
