import { useState } from "react";
import { Box, Typography } from "@mui/material";
import { IconCircleCheck, IconLink } from "@tabler/icons-react";
import SportIcon from "../workouts/SportIcon";
import { useSportColour } from "../../hooks/useSportColour";
import { formatTotalDuration } from "../../utils/formatDuration";
import type { CalendarEventData } from "../../types/calendar";

interface CalendarEventProps {
  event: CalendarEventData;
}

function renderStatus(event: CalendarEventData) {
  if (event.kind === "activity") return <IconCircleCheck size={14} />;
  if (event.linkedActivityId) return <IconLink size={14} />;
  return (
    <Box
      component="span"
      sx={{
        display: "block",
        width: 6,
        height: 6,
        borderRadius: "50%",
        bgcolor: "currentColor",
      }}
    />
  );
}

export default function CalendarEvent({ event }: CalendarEventProps) {
  const { main } = useSportColour(event.sport);
  const [hovered, setHovered] = useState(false);
  const seconds = event.durationSeconds ?? event.plannedDurationSeconds;

  return (
    <Box
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      sx={{
        display: "flex",
        alignItems: "center",
        gap: 0.75,
        minWidth: 0,
        minHeight: 36,
        py: 0.25,
        color: "text.primary",
      }}
    >
      <Box
        sx={{
          width: 4,
          alignSelf: "stretch",
          borderRadius: 0.5,
          bgcolor: main,
          flexShrink: 0,
        }}
      />
      <SportIcon sport={event.sport} size={18} sx={{ flexShrink: 0 }} />
      <Typography
        variant="caption"
        noWrap
        sx={{ flexGrow: 1, minWidth: 0, lineHeight: 1.4 }}
      >
        {event.title}
      </Typography>
      {hovered && seconds !== null && (
        <Typography
          variant="caption"
          sx={{ flexShrink: 0, color: "text.secondary" }}
        >
          {formatTotalDuration(seconds * 1000)}
        </Typography>
      )}
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          flexShrink: 0,
          color: "text.secondary",
        }}
      >
        {renderStatus(event)}
      </Box>
    </Box>
  );
}
