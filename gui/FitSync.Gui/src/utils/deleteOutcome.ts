import type { DeleteScheduledWorkoutResponse } from "../api/generated/fitSyncApi.schemas";
import { formatServiceList } from "./publishStatus";

export interface DeleteOutcomeNotice {
  severity: "warning" | "error";
  headline: string;
  listLabel: string;
  failedServiceTypes: string[];
  guidance: string | null;
  closeDrawer: boolean;
}

export function forceDeleteWarning(serviceTypes: string[]): string {
  return `If it can't be removed from ${formatServiceList(serviceTypes)}, you'll need to delete it there yourself.`;
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

  if (!response.deleted) {
    return {
      severity: "error",
      headline: "Still in your calendar.",
      listLabel: "Couldn't remove from:",
      failedServiceTypes: failed,
      guidance:
        "Delete again to retry, or tick the box to remove it from your calendar anyway.",
      closeDrawer: false,
    };
  }

  return {
    severity: "warning",
    headline: "Removed from your calendar.",
    listLabel: "Still on:",
    failedServiceTypes: failed,
    guidance: null,
    closeDrawer: true,
  };
}
