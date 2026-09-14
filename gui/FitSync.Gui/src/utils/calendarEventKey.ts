import type { CalendarEventData } from "../types/calendar";

export function calendarEventKey(
  event: Pick<CalendarEventData, "kind" | "id">,
): string {
  return `${event.kind}:${event.id}`;
}

export const ADD_SLOT_PREFIX = "add:";

export function addSlotId(date: string): string {
  return `${ADD_SLOT_PREFIX}${date}`;
}

export function isAddSlotId(id: string): boolean {
  return id.startsWith(ADD_SLOT_PREFIX);
}

export function addSlotDate(id: string): string {
  return id.slice(ADD_SLOT_PREFIX.length);
}
