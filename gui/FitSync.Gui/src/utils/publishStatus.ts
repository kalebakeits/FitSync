import type { PublicationState } from "../types/calendar";

export function publicationStatusLabel(state: PublicationState): string {
  if (state === "Success") return "Sent";
  if (state === "Pending") return "Sending…";
  return "Failed";
}
