import { Box, Typography, ListItem, ListItemText, Chip } from "@mui/material";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import { getStatusInfo } from "./ActivityStatusInfo";

interface ActivityListItemProps {
  activity: ActivityResponse;
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

export default function ActivityListItem({ activity }: ActivityListItemProps) {
  const statusInfo = getStatusInfo(activity.status);

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
