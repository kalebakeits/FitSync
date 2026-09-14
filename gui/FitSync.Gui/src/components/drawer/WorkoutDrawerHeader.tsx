import { Box, Chip, IconButton, Stack, Typography } from "@mui/material";
import { Close } from "@mui/icons-material";
import { format, parseISO } from "date-fns";
import SportIcon from "../workouts/SportIcon";
import { useSportColour } from "../../hooks/useSportColour";
import type { CalendarEventData } from "../../types/calendar";

const CATEGORY_LABEL: Record<string, string> = {
  swim: "Swim",
  bike: "Bike",
  run: "Run",
  other: "Workout",
};

interface WorkoutDrawerHeaderProps {
  event: CalendarEventData;
  onClose: () => void;
}

export default function WorkoutDrawerHeader({
  event,
  onClose,
}: WorkoutDrawerHeaderProps) {
  const { main, contrastText, category } = useSportColour(event.sport);

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "flex-start",
        gap: 1.5,
        px: 3,
        py: 2,
        borderBottom: 1,
        borderColor: "divider",
      }}
    >
      <SportIcon sport={event.sport} size={28} sx={{ color: main, mt: 0.25 }} />
      <Box sx={{ flexGrow: 1, minWidth: 0 }}>
        <Typography variant="h6" fontWeight="medium" noWrap>
          {event.title}
        </Typography>
        <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 0.5 }}>
          <Chip
            size="small"
            label={CATEGORY_LABEL[category]}
            sx={{ bgcolor: main, color: contrastText }}
          />
          <Typography variant="caption" color="text.secondary">
            {format(parseISO(event.date), "EEE d MMM")}
          </Typography>
        </Stack>
      </Box>
      <IconButton size="small" onClick={onClose} aria-label="Close">
        <Close fontSize="small" />
      </IconButton>
    </Box>
  );
}
