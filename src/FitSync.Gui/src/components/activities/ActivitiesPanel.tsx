import { useQueryClient } from "@tanstack/react-query";
import ActivitiesPanelContent from "./ActivitiesPanelContent";
import ActivitiesHeader from "./ActivitiesHeader";
import DashboardColumn from "../dashboard/DashboardColumn";
import { getGetApiActivitiesQueryKey } from "../../api/generated/activities/activities";
import { usePostApiFetchersTrigger } from "../../api/generated/fetchers/fetchers";

export default function ActivitiesPanel() {
  const queryClient = useQueryClient();

  const handleRefresh = () => {
    queryClient.invalidateQueries({ queryKey: getGetApiActivitiesQueryKey() });
  };

  const triggerFetchMutation = usePostApiFetchersTrigger({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiActivitiesQueryKey() });
      },
    },
  });

  const handleTriggerFetch = () => {
    triggerFetchMutation.mutate();
  };

  return (
    <DashboardColumn
      header={
        <ActivitiesHeader
          onRefreshClick={handleRefresh}
          onTriggerFetch={handleTriggerFetch}
          isTriggeringFetch={triggerFetchMutation.isPending}
        />
      }
      body={<ActivitiesPanelContent />}
    />
  );
}
