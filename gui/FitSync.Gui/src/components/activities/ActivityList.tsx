import { List } from "@mui/material";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import ActivityListItem from "./ActivityListItem";

interface ActivityListProps {
  activities: ActivityResponse[];
  onDelete?: (activityId: string) => void;
  onRetry?: (activityId: string) => void;
}

export default function ActivityList({
  activities,
  onDelete,
  onRetry,
}: ActivityListProps) {
  return (
    <List sx={{ width: "auto", height: "100%", overflowY: "scroll" }}>
      {activities.map((activity) => {
        return (
          <ActivityListItem
            key={activity.id}
            activity={activity}
            onDelete={onDelete}
            onRetry={onRetry}
          />
        );
      })}
    </List>
  );
}
