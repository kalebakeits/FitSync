import { useQueryClient } from "@tanstack/react-query";
import ActivitiesPanelContent from "./ActivitiesPanelContent";
import ActivitiesHeader from "./ActivitiesHeader";
import DashboardColumn from "../dashboard/DashboardColumn";
import { getGetApiActivitiesQueryKey } from "../../api/generated/activities/activities";

export default function ActivitiesPanel() {
  const queryClient = useQueryClient();

  const handleRefresh = () => {
    queryClient.invalidateQueries({ queryKey: getGetApiActivitiesQueryKey() });
  };

  return (
    <DashboardColumn
      header={<ActivitiesHeader onRefreshClick={handleRefresh} />}
      body={<ActivitiesPanelContent />}
    />
  );
}
