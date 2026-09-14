import type { PublicationState } from "../types/calendar";

export function publicationStatusLabel(state: PublicationState): string {
  if (state === "Success") return "Sent";
  if (state === "Pending") return "Sending…";
  return "Failed";
}

export function formatServiceList(serviceTypes: string[]): string {
  const last = serviceTypes[serviceTypes.length - 1];
  if (serviceTypes.length === 1) return last;
  return `${serviceTypes.slice(0, -1).join(", ")} and ${last}`;
}
