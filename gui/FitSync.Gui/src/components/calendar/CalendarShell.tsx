import { useRef, useState } from "react";
import {
  Box,
  IconButton,
  LinearProgress,
  Stack,
  Typography,
} from "@mui/material";
import { IconChevronLeft, IconChevronRight } from "@tabler/icons-react";
import CalendarGrid from "./CalendarGrid";
import ViewSwitcher from "./ViewSwitcher";
import type { CalendarView } from "./ViewSwitcher";
import type { CalendarEventData } from "../../types/calendar";
import type { CalendarRef, DatesSetInfo } from "@fullcalendar/react";

interface CalendarShellProps {
  events: CalendarEventData[];
  isLoading: boolean;
  onDatesSet: (info: DatesSetInfo) => void;
  onSelectEvent: (event: CalendarEventData) => void;
  onReschedule: (event: CalendarEventData, date: string) => void;
  onScheduleAt: (date: string) => void;
}

export default function CalendarShell({
  events,
  isLoading,
  onDatesSet,
  onSelectEvent,
  onReschedule,
  onScheduleAt,
}: CalendarShellProps) {
  const calendarRef = useRef<CalendarRef>(null);
  const [view, setView] = useState<CalendarView>("dayGridWeek");

  const handleDatesSet = (info: DatesSetInfo) => {
    setView(info.view.type === "dayGridMonth" ? "dayGridMonth" : "dayGridWeek");
    onDatesSet(info);
  };

  return (
    <Box sx={{ width: "100%", p: 3 }}>
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        flexWrap="wrap"
        gap={1}
        sx={{ mb: 2 }}
      >
        <Typography variant="h5" fontWeight="bold">
          Calendar
        </Typography>
        <Stack direction="row" alignItems="center" spacing={1}>
          <IconButton
            size="small"
            aria-label="Previous"
            onClick={() => calendarRef.current?.getApi().prev()}
          >
            <IconChevronLeft size={18} />
          </IconButton>
          <IconButton
            size="small"
            aria-label="Today"
            onClick={() => calendarRef.current?.getApi().today()}
          >
            <Typography variant="caption">Today</Typography>
          </IconButton>
          <IconButton
            size="small"
            aria-label="Next"
            onClick={() => calendarRef.current?.getApi().next()}
          >
            <IconChevronRight size={18} />
          </IconButton>
          <ViewSwitcher
            view={view}
            onChange={(next) => {
              calendarRef.current?.getApi().changeView(next);
              setView(next);
            }}
          />
        </Stack>
      </Stack>

      {isLoading && <LinearProgress sx={{ mb: 1 }} />}

      <CalendarGrid
        calendarRef={calendarRef}
        events={events}
        onDatesSet={handleDatesSet}
        onSelectEvent={onSelectEvent}
        onReschedule={onReschedule}
        onScheduleAt={onScheduleAt}
      />
    </Box>
  );
}
