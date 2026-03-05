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
  Typography,
} from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import { useGetApiCredentialsAll } from "../../api/generated/credentials/credentials";
import {
  usePostApiActivitiesIdPush,
  getGetApiActivitiesQueryKey,
} from "../../api/generated/activities/activities";

interface PushToDestinationModalProps {
  open: boolean;
  activity: ActivityResponse;
  onClose: () => void;
}

export default function PushToDestinationModal({
  open,
  activity,
  onClose,
}: PushToDestinationModalProps) {
  const queryClient = useQueryClient();
  const [selected, setSelected] = useState("");

  const { data: allServices = [] } = useGetApiCredentialsAll();

  const alreadyPushed = new Set(
    activity.uploadStatuses?.map((u) => u.destinationServiceType) ?? [],
  );

  const availableDestinations = allServices.filter(
    (s) => s.isUploader && !alreadyPushed.has(s.serviceType ?? ""),
  );

  const pushMutation = usePostApiActivitiesIdPush({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiActivitiesQueryKey() });
        onClose();
        setSelected("");
      },
    },
  });

  const handleConfirm = () => {
    if (!selected) return;
    pushMutation.mutate({
      id: activity.id,
      data: { destinationServiceType: selected },
    });
  };

  const handleClose = () => {
    setSelected("");
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle>Push to destination</DialogTitle>
      <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
        {availableDestinations.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No additional destinations available. Connect more uploaders in the
            Integrations tab.
          </Typography>
        ) : (
          <FormControl fullWidth size="small">
            <InputLabel>Destination</InputLabel>
            <Select
              value={selected}
              label="Destination"
              onChange={(e) => setSelected(e.target.value)}
            >
              {availableDestinations.map((s) => (
                <MenuItem key={s.serviceType} value={s.serviceType ?? ""}>
                  {s.serviceType}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        {availableDestinations.length > 0 && (
          <Button
            variant="contained"
            onClick={handleConfirm}
            disabled={!selected || pushMutation.isPending}
          >
            Push
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}
