import type { AvailableServiceResponse } from "../api/generated/fitSyncApi.schemas";

export function getPublishingServiceTypes(
  services: AvailableServiceResponse[],
): string[] {
  return services
    .filter((service) => Boolean(service.supportsWorkoutPublishing))
    .map((service) => service.serviceType ?? "");
}
