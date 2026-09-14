import { useMemo, useState } from "react";
import { Box, useTheme } from "@mui/material";
import { format } from "date-fns";
import FullCalendar from "@fullcalendar/react";
import dayGridPlugin from "@fullcalendar/react/daygrid";
import interactionPlugin from "@fullcalendar/react/interaction";
import classicTheme from "@fullcalendar/react/themes/classic";
import "@fullcalendar/react/skeleton.css";
import "@fullcalendar/react/themes/classic/theme.css";
import "@fullcalendar/react/themes/classic/palette.css";
import AddSlotRow from "./AddSlotRow";
import CalendarEvent from "./CalendarEvent";
import DayCellHeader from "./DayCellHeader";
import {
  calendarDayCellClass,
  calendarEventSurface,
  calendarHeaderDividerClass,
  calendarSx,
  calendarViewClass,
} from "./calendarSx";
import {
  addSlotDate,
  addSlotId,
  calendarEventKey,
  isAddSlotId,
} from "../../utils/calendarEventKey";
import { listDaysBetween } from "../../utils/calendarDays";
import { useKeyboardReschedule } from "../../hooks/useKeyboardReschedule";
import type { ReactNode, RefObject } from "react";
import type { CalendarEventData } from "../../types/calendar";
import type {
  CalendarRef,
  DatesSetInfo,
  EventClickInfo,
  EventDropInfo,
  EventInput,
} from "@fullcalendar/react";

interface CalendarGridProps {
  calendarRef: RefObject<CalendarRef | null>;
  events: CalendarEventData[];
  onDatesSet: (info: DatesSetInfo) => void;
  onSelectEvent: (event: CalendarEventData) => void;
  onReschedule: (event: CalendarEventData, date: string) => void;
  onScheduleAt: (date: string) => void;
}

const weekViewType = "dayGridWeek";

function isAddSlot(value: unknown): boolean {
  if (typeof value !== "object" || value === null || !("id" in value))
    return false;
  const { id } = value;
  return typeof id === "string" && isAddSlotId(id);
}

function renderEventContent(
  id: string,
  byKey: Map<string, CalendarEventData>,
): ReactNode {
  if (isAddSlotId(id)) return <AddSlotRow />;

  const event = byKey.get(id);
  return event ? <CalendarEvent event={event} /> : true;
}

export default function CalendarGrid({
  calendarRef,
  events,
  onDatesSet,
  onSelectEvent,
  onReschedule,
  onScheduleAt,
}: CalendarGridProps) {
  const theme = useTheme();
  const { background, foreground } = calendarEventSurface(theme);
  const [viewType, setViewType] = useState(weekViewType);
  const [visibleDays, setVisibleDays] = useState<string[]>([]);

  const byKey = useMemo(
    () => new Map(events.map((event) => [calendarEventKey(event), event])),
    [events],
  );

  const {
    grabbed,
    announcement,
    containerRef,
    onEventDidMount,
    onEventWillUnmount,
  } = useKeyboardReschedule({ events, onReschedule });

  const handleDatesSet = (info: DatesSetInfo) => {
    setViewType(info.view.type);
    setVisibleDays(listDaysBetween(info.start, info.end));
    onDatesSet(info);
  };

  const addSlots = useMemo<EventInput[]>(
    () =>
      viewType !== weekViewType
        ? []
        : visibleDays.map((day) => ({
            id: addSlotId(day),
            start: day,
            allDay: true,
            editable: false,
            color: "transparent",
          })),
    [viewType, visibleDays],
  );

  const calendarEvents = useMemo<EventInput[]>(
    () => [
      ...addSlots,
      ...events.map((event) => {
        const key = calendarEventKey(event);
        const isGrabbed = grabbed?.id === key;

        return {
          id: key,
          title: event.title,
          start: isGrabbed ? grabbed.date : event.date,
          allDay: true,
          editable: event.kind === "scheduled",
          className: isGrabbed ? "Calendar-eventGrabbed" : undefined,
          color: background,
          contrastColor: foreground,
        };
      }),
    ],
    [addSlots, events, background, foreground, grabbed],
  );

  const renderDayLabel = (
    day: Date,
    label: string,
    isToday: boolean,
    isOther: boolean,
    showAdd: boolean,
  ) => (
    <DayCellHeader
      date={format(day, "yyyy-MM-dd")}
      label={label}
      isToday={isToday}
      isOther={isOther}
      showAdd={showAdd}
      onAdd={onScheduleAt}
    />
  );

  return (
    <Box ref={containerRef} sx={calendarSx(theme)}>
      <Box
        role="status"
        aria-live="polite"
        sx={{
          position: "absolute",
          width: 1,
          height: 1,
          p: 0,
          m: -1,
          overflow: "hidden",
          clip: "rect(0 0 0 0)",
          whiteSpace: "nowrap",
          border: 0,
        }}
      >
        {announcement}
      </Box>
      <FullCalendar
        ref={calendarRef}
        plugins={[dayGridPlugin, interactionPlugin, classicTheme]}
        initialView={weekViewType}
        headerToolbar={false}
        height="100%"
        firstDay={1}
        dayMaxEvents={viewType === weekViewType ? false : 3}
        expandRows
        colorScheme={theme.palette.mode}
        editable
        events={calendarEvents}
        datesSet={handleDatesSet}
        eventOrder={(a: unknown, b: unknown) =>
          Number(isAddSlot(b)) - Number(isAddSlot(a))
        }
        eventClick={(info: EventClickInfo) => {
          const id = info.event.id;
          if (isAddSlotId(id)) {
            onScheduleAt(addSlotDate(id));
            return;
          }
          const event = byKey.get(id);
          if (event) onSelectEvent(event);
        }}
        eventDrop={(info: EventDropInfo) => {
          const event = byKey.get(info.event.id);
          if (event && info.event.start) {
            onReschedule(event, format(info.event.start, "yyyy-MM-dd"));
          }
        }}
        eventAllow={(_span, movingEvent) => {
          const id = movingEvent?.id ?? "";
          return !id.startsWith("activity:") && !isAddSlotId(id);
        }}
        eventDidMount={onEventDidMount}
        eventWillUnmount={onEventWillUnmount}
        eventContent={(info) => renderEventContent(info.event.id, byKey)}
        dayCellClass={calendarDayCellClass}
        viewClass={calendarViewClass}
        dayHeaderDividerClass={calendarHeaderDividerClass}
        dayCellTopContent={(info) =>
          renderDayLabel(
            info.date,
            info.dayNumberText,
            info.isToday,
            info.isOther,
            true,
          )
        }
        dayHeaderContent={(info) =>
          info.view.type === weekViewType
            ? renderDayLabel(
                info.date,
                info.text,
                info.isToday,
                info.isOther,
                false,
              )
            : true
        }
      />
    </Box>
  );
}
