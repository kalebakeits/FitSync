import { List } from "@mui/material";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import ActivityListItem from "./ActivityListItem";

interface ActivityListProps {
  activities: ActivityResponse[];
}

export default function ActivityList({ activities }: ActivityListProps) {
  return (
    <List sx={{ width: "auto", height: "100%", overflowY: "scroll" }}>
      {activities.map((activity) => {
        return <ActivityListItem activity={activity} />;
      })}
    </List>
  );
}
