import { Box, ListItem, ListItemText, Typography } from "@mui/material";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import UploadStatusChips from "./UploadStatusChips";
import ActivityActionsMenu from "./ActivityActionsMenu";

interface ActivityListItemProps {
  activity: ActivityResponse;
  onRetry?: (activityId: string) => void;
  onDelete?: (activityId: string) => void;
}

export default function ActivityListItem({
  activity,
  onRetry,
  onDelete,
}: ActivityListItemProps) {
  return (
    <ListItem
      sx={{
        border: 1,
        borderColor: "divider",
        borderRadius: 1,
        mx: 2,
        my: 0.5,
        width: "auto",
      }}
      secondaryAction={
        (onRetry || onDelete) && (
          <ActivityActionsMenu
            activity={activity}
            onRetry={() => onRetry?.(activity.id)}
            onDelete={() => onDelete?.(activity.id)}
          />
        )
      }
    >
      <ListItemText
        primary={
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              gap: 1,
            }}
          >
            <Typography variant="h6">
              {activity.activityName ||
                activity.originalFileName ||
                "Unnamed Activity"}
            </Typography>
          </Box>
        }
        secondary={
          <Box sx={{ display: "flex", flexDirection: "column", gap: 0.5 }}>
            <Typography variant="body2" color="text.secondary">
              Source: {activity.source} •{" "}
              {new Date(activity.activityDate).toLocaleDateString()}
            </Typography>
            <UploadStatusChips uploadStatuses={activity.uploadStatuses ?? []} />
          </Box>
        }
      />
    </ListItem>
  );
}
