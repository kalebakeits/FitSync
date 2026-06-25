import { useState, useMemo, useCallback } from "react";
import { Calendar, dateFnsLocalizer } from "react-big-calendar";
import withDragAndDrop from "react-big-calendar/lib/addons/dragAndDrop";
import { format, parse, startOfWeek, getDay, startOfMonth, endOfMonth, subMonths, addMonths } from "date-fns";
import { enUS } from "date-fns/locale/en-US";
import { Box, Typography, useTheme } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import type { ScheduledWorkoutResponse } from "../../api/generated/fitSyncApi.schemas";
import {
  useGetApiScheduledWorkouts,
  usePatchApiScheduledWorkoutsId,
  getGetApiScheduledWorkoutsQueryKey,
} from "../../api/generated/scheduled-workouts/scheduled-workouts";
import AppLayout from "../../components/layout/AppLayout";
import ScheduledWorkoutDetailModal from "../../components/schedule/ScheduledWorkoutDetailModal";
import PublishToDateModal from "../../components/schedule/PublishToDateModal";
import "react-big-calendar/lib/css/react-big-calendar.css";
import "react-big-calendar/lib/addons/dragAndDrop/styles.css";

const localizer = dateFnsLocalizer({
  format,
  parse,
  startOfWeek: () => startOfWeek(new Date(), { weekStartsOn: 0 }),
  getDay,
  locales: { "en-US": enUS },
});

const DnDCalendar = withDragAndDrop<CalendarEvent>(Calendar);

interface CalendarEvent {
  id: string;
  title: string;
  start: Date;
  end: Date;
  resource: ScheduledWorkoutResponse;
}

function toEvents(workouts: ScheduledWorkoutResponse[]): CalendarEvent[] {
  return workouts
    .filter((w) => !!w.scheduledDate)
    .map((w) => {
      const date = new Date(w.scheduledDate!);
      return { id: w.id!, title: w.workoutName ?? "Workout", start: date, end: date, resource: w };
    });
}

function toLocalDateString(date: Date): string {
  return date.toLocaleDateString("en-CA");
}

export default function SchedulePage() {
  const theme = useTheme();
  const queryClient = useQueryClient();
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedWorkout, setSelectedWorkout] = useState<ScheduledWorkoutResponse | null>(null);
  const [publishDate, setPublishDate] = useState<string | null>(null);

  const from = format(startOfMonth(subMonths(currentDate, 1)), "yyyy-MM-dd");
  const to = format(endOfMonth(addMonths(currentDate, 1)), "yyyy-MM-dd");

  const { data = [] } = useGetApiScheduledWorkouts({ from, to });
  const events = useMemo(() => toEvents(data), [data]);

  const moveMutation = usePatchApiScheduledWorkoutsId({
    mutation: {
      onSuccess: () => queryClient.invalidateQueries({ queryKey: getGetApiScheduledWorkoutsQueryKey() }),
    },
  });

  const handleEventDrop = useCallback(
    ({ event, start }: { event: object; start: Date | string }) => {
      const e = event as CalendarEvent;
      const newDate = toLocalDateString(new Date(start));
      moveMutation.mutate({ id: e.id, data: { scheduledDate: newDate } });
    },
    [moveMutation]
  );

  const handleSelectEvent = useCallback((event: object) => {
    setSelectedWorkout((event as CalendarEvent).resource);
  }, []);

  const handleSelectSlot = useCallback(({ start }: { start: Date }) => {
    setPublishDate(toLocalDateString(start));
  }, []);

  const isDark = theme.palette.mode === "dark";

  const calendarSx = {
    height: "calc(100vh - 180px)",
    "& .rbc-calendar": { bgcolor: "background.paper", color: "text.primary" },
    "& .rbc-header": { borderColor: "divider", color: "text.secondary", py: 1 },
    "& .rbc-month-view, & .rbc-time-view, & .rbc-agenda-view": { borderColor: "divider" },
    "& .rbc-day-bg": { borderColor: "divider" },
    "& .rbc-off-range-bg": { bgcolor: isDark ? "rgba(255,255,255,0.03)" : "rgba(0,0,0,0.03)" },
    "& .rbc-today": { bgcolor: isDark ? "rgba(144,202,249,0.08)" : "rgba(25,118,210,0.06)" },
    "& .rbc-toolbar button": { color: "text.primary", borderColor: "divider" },
    "& .rbc-toolbar button:hover": { bgcolor: "action.hover" },
    "& .rbc-toolbar button.rbc-active": { bgcolor: "primary.main", color: "primary.contrastText", borderColor: "primary.main" },
    "& .rbc-event": { bgcolor: "primary.main", borderRadius: "4px" },
    "& .rbc-show-more": { color: "primary.main" },
    "& .rbc-addons-dnd .rbc-addons-dnd-drag-preview": { opacity: 0.6 },
  };

  return (
    <AppLayout>
      <Box sx={{ p: 3, width: "100%" }}>
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
          <Typography variant="h5" fontWeight="bold">Schedule</Typography>
        </Box>

        <Box sx={calendarSx}>
          <DnDCalendar
            localizer={localizer}
            events={events}
            date={currentDate}
            onNavigate={setCurrentDate}
            views={["month", "week", "agenda"]}
            defaultView="month"
            allDayAccessor={() => true}
            style={{ height: "100%" }}
            onEventDrop={handleEventDrop}
            onSelectEvent={handleSelectEvent}
            onSelectSlot={handleSelectSlot}
            selectable
            resizable={false}
          />
        </Box>
      </Box>

      <ScheduledWorkoutDetailModal
        scheduledWorkout={selectedWorkout}
        onClose={() => setSelectedWorkout(null)}
      />

      <PublishToDateModal
        open={publishDate !== null}
        initialDate={publishDate ?? ""}
        onClose={() => setPublishDate(null)}
      />
    </AppLayout>
  );
}
