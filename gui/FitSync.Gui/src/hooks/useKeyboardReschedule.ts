import { useCallback, useRef, useState } from "react";
import { addDays, format, parseISO } from "date-fns";
import { calendarEventKey } from "../utils/calendarEventKey";
import type { CalendarEventData } from "../types/calendar";

export interface GrabbedEvent {
  id: string;
  originalDate: string;
  date: string;
  title: string;
}

interface KeyboardRescheduleOptions {
  events: CalendarEventData[];
  onReschedule: (event: CalendarEventData, date: string) => void;
}

const DAY_OFFSETS: Record<string, number> = {
  ArrowRight: 1,
  ArrowLeft: -1,
  ArrowDown: 7,
  ArrowUp: -7,
};

interface EventMountInfo {
  event: { id: string };
  el: HTMLElement;
}

function focusableAncestor(el: HTMLElement): HTMLElement {
  return el.closest<HTMLElement>("[tabindex]") ?? el;
}

export function useKeyboardReschedule({
  events,
  onReschedule,
}: KeyboardRescheduleOptions) {
  const [grabbed, setGrabbed] = useState<GrabbedEvent | null>(null);
  const [announcement, setAnnouncement] = useState("");
  const listeners = useRef(
    new Map<
      string,
      { target: HTMLElement; listener: (event: KeyboardEvent) => void }
    >(),
  );

  const containerRef = useRef<HTMLDivElement | null>(null);

  const latest = useRef({ events, onReschedule, grabbed });
  latest.current = { events, onReschedule, grabbed };

  const handleKeyDown = useCallback(
    (domEvent: KeyboardEvent, eventId: string) => {
      const current = latest.current;
      const event = current.events.find(
        (candidate) => calendarEventKey(candidate) === eventId,
      );

      if (!event || event.kind !== "scheduled") return;

      if (domEvent.key === " ") {
        domEvent.preventDefault();
        domEvent.stopPropagation();

        if (current.grabbed === null) {
          setGrabbed({
            id: eventId,
            originalDate: event.date,
            date: event.date,
            title: event.title,
          });
          setAnnouncement(
            `${event.title} picked up. Arrow keys move it a day, up and down move a week, space drops it, escape cancels.`,
          );
          return;
        }

        if (current.grabbed.id !== eventId) return;

        current.onReschedule(event, current.grabbed.date);
        setAnnouncement(`${event.title} moved to ${current.grabbed.date}.`);
        setGrabbed(null);
        return;
      }

      if (current.grabbed === null || current.grabbed.id !== eventId) return;

      const offset = DAY_OFFSETS[domEvent.key];

      if (offset !== undefined) {
        domEvent.preventDefault();
        domEvent.stopPropagation();
        const date = format(
          addDays(parseISO(current.grabbed.date), offset),
          "yyyy-MM-dd",
        );
        setGrabbed({ ...current.grabbed, date });
        setAnnouncement(`${event.title} moved to ${date}.`);
        return;
      }

      if (domEvent.key === "Escape") {
        domEvent.preventDefault();
        domEvent.stopPropagation();
        setGrabbed(null);
        setAnnouncement(
          `${event.title} returned to ${current.grabbed.originalDate}.`,
        );
      }
    },
    [],
  );

  const onEventDidMount = useCallback(
    (info: EventMountInfo) => {
      const target = focusableAncestor(info.el);
      const listener = (domEvent: KeyboardEvent) =>
        handleKeyDown(domEvent, info.event.id);
      listeners.current.set(info.event.id, { target, listener });
      target.addEventListener("keydown", listener);
      target.dataset.rescheduleEvent = info.event.id;

      if (latest.current.grabbed?.id === info.event.id) {
        target.focus();
      }
    },
    [handleKeyDown],
  );

  const onEventWillUnmount = useCallback((info: EventMountInfo) => {
    const entry = listeners.current.get(info.event.id);
    if (!entry) return;
    entry.target.removeEventListener("keydown", entry.listener);
    if (entry.target.dataset.rescheduleEvent === info.event.id) {
      delete entry.target.dataset.rescheduleEvent;
    }
    listeners.current.delete(info.event.id);
  }, []);

  return {
    grabbed,
    announcement,
    containerRef,
    onEventDidMount,
    onEventWillUnmount,
  };
}
