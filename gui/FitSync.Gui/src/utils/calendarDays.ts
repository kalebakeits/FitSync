import { addDays, format, isBefore } from "date-fns";

export function listDaysBetween(start: Date, end: Date): string[] {
  const days: string[] = [];
  let cursor = start;

  while (isBefore(cursor, end)) {
    days.push(format(cursor, "yyyy-MM-dd"));
    cursor = addDays(cursor, 1);
  }

  return days;
}
