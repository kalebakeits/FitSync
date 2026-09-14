import type { DeleteScheduledWorkoutResponse } from "../api/generated/fitSyncApi.schemas";

export interface DeleteOutcomeNotice {
  severity: "warning" | "error";
  headline: string;
  failedServiceTypes: string[];
  closeDrawer: boolean;
}

export function failedServiceTypes(
  response: DeleteScheduledWorkoutResponse,
): string[] {
  const failed: string[] = [];
  for (const publication of response.publications ?? []) {
    const serviceType = publication.serviceType;
    if (!publication.succeeded && serviceType) failed.push(serviceType);
  }
  return failed;
}

export function describeDeleteOutcome(
  response: DeleteScheduledWorkoutResponse,
): DeleteOutcomeNotice | null {
  if (!response.found) return null;

  const failed = failedServiceTypes(response);
  if (failed.length === 0) return null;

  const deleted = response.deleted === true;

  return {
    severity: deleted ? "warning" : "error",
    headline: "Failed to delete from",
    failedServiceTypes: failed,
    closeDrawer: deleted,
  };
}
