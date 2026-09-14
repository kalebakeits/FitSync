import type { SxProps, Theme } from "@mui/material";
import type {
  ClassNameGenerator,
  DayCellInfo,
  DayHeaderDividerInfo,
  ViewDisplayInfo,
} from "@fullcalendar/react";

export interface CalendarEventSurface {
  background: string;
  foreground: string;
}

const weekViewType = "dayGridWeek";
const weekViewClass = "Calendar-weekView";
const dayColumnClass = "Calendar-dayColumn";
const weekendClass = "Calendar-dayWeekend";
const headerDividerClass = "Calendar-headerDivider";

export function calendarEventSurface(theme: Theme): CalendarEventSurface {
  const background =
    theme.palette.mode === "dark"
      ? theme.palette.grey[900]
      : theme.palette.grey[100];

  return { background, foreground: theme.palette.text.primary };
}

export const calendarDayCellClass: ClassNameGenerator<DayCellInfo> = (info) => {
  if (info.view.type !== weekViewType) return "";

  const isWeekend = !info.isToday && (info.dow === 0 || info.dow === 6);
  return isWeekend ? `${dayColumnClass} ${weekendClass}` : dayColumnClass;
};

export const calendarViewClass: ClassNameGenerator<ViewDisplayInfo> = (info) =>
  info.view.type === weekViewType ? weekViewClass : "";

export const calendarHeaderDividerClass: ClassNameGenerator<
  DayHeaderDividerInfo
> = () => headerDividerClass;

export function calendarSx(theme: Theme): SxProps<Theme> {
  const isDark = theme.palette.mode === "dark";
  const surface = calendarEventSurface(theme);

  return {
    position: "relative",
    height: "calc(100vh - 190px)",
    minHeight: 480,
    borderRadius: 1,
    overflow: "hidden",
    "--fc-classic-background": theme.palette.background.paper,
    "--fc-classic-foreground": theme.palette.text.primary,
    "--fc-classic-muted-foreground": theme.palette.text.secondary,
    "--fc-classic-faint-foreground": theme.palette.text.disabled,
    "--fc-classic-border": theme.palette.divider,
    "--fc-classic-strong-border": theme.palette.divider,
    "--fc-classic-faint": isDark
      ? "rgba(255,255,255,0.03)"
      : "rgba(0,0,0,0.03)",
    "--fc-classic-muted": theme.palette.action.hover,
    "--fc-classic-strong": theme.palette.action.selected,
    "--fc-classic-highlight": theme.palette.action.selected,
    "--fc-classic-today": isDark
      ? "rgba(144,202,249,0.08)"
      : "rgba(25,118,210,0.06)",
    "--fc-classic-now": theme.palette.error.main,
    "--fc-classic-event": surface.background,
    "--fc-classic-event-contrast": surface.foreground,
    "--fc-classic-primary": theme.palette.primary.main,
    "--fc-classic-primary-foreground": theme.palette.primary.contrastText,
    "--fc-classic-button": theme.palette.action.hover,
    "--fc-classic-button-border": theme.palette.divider,
    "--fc-classic-button-strong": theme.palette.action.selected,
    "--fc-classic-button-strong-border": theme.palette.divider,
    "--fc-classic-button-outline": theme.palette.divider,
    "--fc-classic-button-foreground": theme.palette.text.primary,
    [`& .${weekViewClass}`]: { "--fc-classic-border-style": "none" },
    [`& .${weekendClass}`]: { backgroundColor: "var(--fc-classic-faint)" },
    [`& .${dayColumnClass}`]: {
      borderLeft: `1px solid ${theme.palette.divider}`,
    },
    [`& .${dayColumnClass}:first-child`]: { borderLeft: "none" },
    "& .Calendar-eventGrabbed": {
      outline: `2px solid ${theme.palette.primary.main}`,
      outlineOffset: -2,
      borderRadius: 0.5,
    },
    [`& .${headerDividerClass}`]: {
      borderBottom: `1px solid ${theme.palette.divider}`,
    },
  };
}
