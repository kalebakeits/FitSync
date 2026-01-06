import { Box, Typography, ListItem, ListItemText, Chip, IconButton } from "@mui/material";
import { Delete } from "@mui/icons-material";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import { getStatusInfo } from "./ActivityStatusInfo";

interface ActivityListItemProps {
  activity: ActivityResponse;
  onDelete?: (activityId: string) => void;
}

function getUserFriendlyError(error: string): string {
  if (
    error.includes("409") ||
    error.toLowerCase().includes("conflict") ||
    error.toLowerCase().includes("duplicate")
  ) {
    return "Duplicate";
  }
  return "Application Error";
}

export default function ActivityListItem({
  activity,
  onDelete,
}: ActivityListItemProps) {
  const statusInfo = getStatusInfo(activity.status);

  const handleDelete = () => {
    if (
      onDelete &&
      confirm(
        `Are you sure you want to delete this activity? It will be re-fetched on the next sync.`
      )
    ) {
      onDelete(activity.id);
    }
  };

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
        onDelete && (
          <IconButton
            edge="end"
            aria-label="delete"
            onClick={handleDelete}
            color="error"
            size="small"
          >
            <Delete />
          </IconButton>
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
            <Chip
              label={statusInfo.label}
              color={statusInfo.color}
              size="small"
              icon={statusInfo.icon}
            />
          </Box>
        }
        secondary={
          <Box>
            <Typography variant="body2" color="text.secondary">
              Source: {activity.source} •{" "}
              {new Date(activity.activityDate).toLocaleDateString()}
            </Typography>
            {activity.lastError && (
              <Typography variant="caption" color="error">
                Error: {getUserFriendlyError(activity.lastError)}
              </Typography>
            )}
          </Box>
        }
      />
    </ListItem>
  );
}
