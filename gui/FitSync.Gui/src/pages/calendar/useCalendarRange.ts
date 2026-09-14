import { useCallback, useState } from "react";
import { addDays, format, startOfWeek } from "date-fns";
import type { DatesSetInfo } from "@fullcalendar/react";

export interface CalendarRange {
  from: string;
  to: string;
}

function toRange(start: Date, end: Date): CalendarRange {
  return { from: format(start, "yyyy-MM-dd"), to: format(end, "yyyy-MM-dd") };
}

function currentWeekRange(): CalendarRange {
  const start = startOfWeek(new Date(), { weekStartsOn: 1 });
  return toRange(start, addDays(start, 7));
}

export function useCalendarRange() {
  const [range, setRange] = useState<CalendarRange>(currentWeekRange);

  const handleDatesSet = useCallback((info: DatesSetInfo) => {
    const next = toRange(info.start, info.end);
    setRange((current) =>
      current.from === next.from && current.to === next.to ? current : next,
    );
  }, []);

  return { range, handleDatesSet };
}
