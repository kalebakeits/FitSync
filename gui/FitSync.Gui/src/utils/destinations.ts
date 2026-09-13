import type { AvailableServiceResponse } from "../api/generated/fitSyncApi.schemas";

export function getSelectableDestinations(
  services: AvailableServiceResponse[],
  sourceServiceType: string,
): AvailableServiceResponse[] {
  return services.filter(
    (s) => Boolean(s.isUploader) && (s.serviceType ?? "") !== sourceServiceType,
  );
}
