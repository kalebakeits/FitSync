import { useQueryClient } from "@tanstack/react-query";
import {
  getGetApiScheduledWorkoutsQueryKey,
  usePatchApiScheduledWorkoutsId,
} from "../../api/generated/scheduled-workouts/scheduled-workouts";
import type {
  GetApiScheduledWorkoutsParams,
  ScheduledWorkoutResponse,
} from "../../api/generated/fitSyncApi.schemas";

export function useRescheduleMutation(params: GetApiScheduledWorkoutsParams) {
  const queryClient = useQueryClient();
  const queryKey = getGetApiScheduledWorkoutsQueryKey(params);

  const mutation = usePatchApiScheduledWorkoutsId({
    mutation: {
      onMutate: async ({ id, data }) => {
        await queryClient.cancelQueries({ queryKey });
        const previous =
          queryClient.getQueryData<ScheduledWorkoutResponse[]>(queryKey);

        queryClient.setQueryData<ScheduledWorkoutResponse[]>(
          queryKey,
          (current) =>
            (current ?? []).map((workout) =>
              workout.id === id
                ? { ...workout, scheduledDate: data.scheduledDate }
                : workout,
            ),
        );

        return { previous };
      },
      onError: (_error, _variables, context) => {
        if (context?.previous) {
          queryClient.setQueryData(queryKey, context.previous);
        }
      },
      onSettled: () => {
        queryClient.invalidateQueries({ queryKey });
      },
    },
  });

  return { reschedule: mutation.mutate, isRescheduling: mutation.isPending };
}
