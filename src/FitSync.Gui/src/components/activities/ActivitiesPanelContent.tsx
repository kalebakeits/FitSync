import { useState } from "react";
import { Box, Button, Typography } from "@mui/material";
import { NavigateBefore, NavigateNext } from "@mui/icons-material";
import { useQueryClient } from "@tanstack/react-query";
import {
  useGetApiActivities,
  useDeleteApiActivitiesId,
  usePostApiActivitiesIdRetry,
  getGetApiActivitiesQueryKey,
} from "../../api/generated/activities/activities";
import EmptyActivityList from "./EmptyActivities";
import DashboardColumnLoading from "../dashboard/DashboardColumnLoading";
import DashboardColumnError from "../dashboard/DashboardColumnError";
import ActivityList from "./ActivityList";

const PAGE_SIZE = 3;

export default function ActivitiesPanelContent() {
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useGetApiActivities(
    {
      limit: PAGE_SIZE,
      offset: (page - 1) * PAGE_SIZE,
    },
    {
      query: { refetchInterval: 60000 },
    },
  );

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiActivitiesQueryKey() });

  const deleteMutation = useDeleteApiActivitiesId({
    mutation: { onSuccess: invalidate },
  });

  const retryMutation = usePostApiActivitiesIdRetry({
    mutation: { onSuccess: invalidate },
  });

  const handleDelete = (activityId: string) => {
    deleteMutation.mutate({ id: activityId });
  };

  const handleRetry = (activityId: string) => {
    retryMutation.mutate({ id: activityId });
  };

  if (isLoading) {
    return <DashboardColumnLoading />;
  }

  if (error) {
    return (
      <DashboardColumnError>Failed to load activities</DashboardColumnError>
    );
  }

  const activities = data?.items ?? [];
  const total = data?.total ?? 0;
  const totalPages = Math.ceil(total / PAGE_SIZE);

  if (total === 0) {
    return <EmptyActivityList />;
  }

  return (
    <Box sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
      <Box sx={{ flexGrow: 1, overflowY: "auto" }}>
        <ActivityList
          activities={activities}
          onDelete={handleDelete}
          onRetry={handleRetry}
        />
      </Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          px: 2,
          py: 1,
          borderTop: 1,
          borderColor: "divider",
          backgroundColor: "background.paper",
        }}
      >
        <Button
          size="small"
          startIcon={<NavigateBefore />}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page === 1}
        >
          Previous
        </Button>
        <Typography variant="body2" color="text.secondary">
          Page {page} of {totalPages}
        </Typography>
        <Button
          size="small"
          endIcon={<NavigateNext />}
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          disabled={page >= totalPages}
        >
          Next
        </Button>
      </Box>
    </Box>
  );
}
