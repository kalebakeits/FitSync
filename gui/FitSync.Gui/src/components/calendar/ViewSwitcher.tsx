import { ToggleButton, ToggleButtonGroup } from "@mui/material";

export type CalendarView = "dayGridWeek" | "dayGridMonth";

interface ViewSwitcherProps {
  view: CalendarView;
  onChange: (view: CalendarView) => void;
}

export default function ViewSwitcher({ view, onChange }: ViewSwitcherProps) {
  return (
    <ToggleButtonGroup
      exclusive
      size="small"
      value={view}
      aria-label="Calendar view"
      onChange={(_event, next: CalendarView | null) => {
        if (next) onChange(next);
      }}
    >
      <ToggleButton value="dayGridWeek" aria-label="Week view">
        Week
      </ToggleButton>
      <ToggleButton value="dayGridMonth" aria-label="Month view">
        Month
      </ToggleButton>
    </ToggleButtonGroup>
  );
}
