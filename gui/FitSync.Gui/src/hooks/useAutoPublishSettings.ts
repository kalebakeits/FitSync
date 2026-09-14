import { useQueryClient } from "@tanstack/react-query";
import {
  getGetApiAutoPublishQueryKey,
  useGetApiAutoPublish,
  usePutApiAutoPublish,
} from "../api/generated/auto-publish/auto-publish";
import { AUTO_PUBLISH_CATEGORIES } from "../utils/sportCategory";
import type { SportCategory } from "../utils/sportCategory";
import {
  isAutoPublishEnabled,
  toAutoPublishRequest,
  withAutoPublishCategories,
} from "../utils/autoPublish";

export function useAutoPublishSettings(serviceType: string) {
  const queryClient = useQueryClient();
  const { data: settings = [], isLoading, isError } = useGetApiAutoPublish();

  const saveMutation = usePutApiAutoPublish({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiAutoPublishQueryKey(),
        });
      },
    },
  });

  const enabledCategories = AUTO_PUBLISH_CATEGORIES.filter((category) =>
    isAutoPublishEnabled(settings, serviceType, category),
  );

  // The PUT is replace-all, so saving before the current settings have loaded
  // would erase every other service's entries.
  const save = (categories: SportCategory[]) => {
    saveMutation.mutate({
      data: {
        settings: toAutoPublishRequest(
          withAutoPublishCategories(settings, serviceType, categories),
        ),
      },
    });
  };

  return {
    enabledCategories,
    save,
    isLoading,
    isError,
    isSaving: saveMutation.isPending,
  };
}
