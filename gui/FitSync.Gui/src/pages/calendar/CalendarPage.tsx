import { useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Alert, Box } from "@mui/material";
import CalendarShell from "../../components/calendar/CalendarShell";
import WorkoutDrawer from "../../components/drawer/WorkoutDrawer";
import WorkoutPicker from "../../components/calendar/WorkoutPicker";
import { useCalendarItems } from "./useCalendarItems";
import { useCalendarRange } from "./useCalendarRange";
import { useRescheduleMutation } from "./useRescheduleMutation";
import type { CalendarEventData } from "../../types/calendar";

export default function CalendarPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { range, handleDatesSet } = useCalendarRange();
  const { events, isLoading, isError } = useCalendarItems(range);
  const { reschedule } = useRescheduleMutation(range);
  const [scheduleDate, setScheduleDate] = useState<string | null>(null);

  const selectedEvent = useMemo(
    () =>
      events.find(
        (event) =>
          event.id === searchParams.get("event") &&
          event.kind === searchParams.get("kind"),
      ) ?? null,
    [events, searchParams],
  );

  const openEvent = (event: CalendarEventData) => {
    setSearchParams({ event: event.id, kind: event.kind });
  };

  const closeEvent = () => setSearchParams({});

  return (
    <Box sx={{ width: "100%" }}>
      {isError && (
        <Alert severity="error" sx={{ mx: 3, mt: 3 }}>
          Failed to load scheduled workouts or activities.
        </Alert>
      )}

      <CalendarShell
        events={events}
        isLoading={isLoading}
        onDatesSet={handleDatesSet}
        onSelectEvent={openEvent}
        onReschedule={(event, date) =>
          reschedule({ id: event.id, data: { scheduledDate: date } })
        }
        onScheduleAt={setScheduleDate}
      />

      <WorkoutDrawer event={selectedEvent} onClose={closeEvent} />

      {scheduleDate && (
        <WorkoutPicker
          date={scheduleDate}
          onClose={() => setScheduleDate(null)}
        />
      )}
    </Box>
  );
}
