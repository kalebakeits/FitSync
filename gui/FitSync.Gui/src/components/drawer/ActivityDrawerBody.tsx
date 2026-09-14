import { Box, Stack, Typography } from "@mui/material";
import SummaryRow from "./SummaryRow";
import { activityMetrics } from "../../utils/activityMetrics";
import type { CalendarEventData } from "../../types/calendar";

export function ActualSummaryRows({ event }: { event: CalendarEventData }) {
  const metrics = activityMetrics(event);

  return (
    <Box>
      <SummaryRow label="Duration" value={metrics.duration} />
      <SummaryRow label="Distance" value={metrics.distance} />
      <SummaryRow label="Avg HR" value={metrics.heartRate} />
      <SummaryRow label="Avg power" value={metrics.power} />
    </Box>
  );
}

export default function ActivityDrawerBody({
  event,
}: {
  event: CalendarEventData;
}) {
  return (
    <Stack spacing={2}>
      <ActualSummaryRows event={event} />
      <Box
        sx={{
          border: 1,
          borderStyle: "dashed",
          borderColor: "divider",
          borderRadius: 1,
          px: 2,
          py: 3,
          textAlign: "center",
        }}
      >
        <Typography variant="body2" color="text.disabled">
          Time-series detail arrives in Phase 2.
        </Typography>
        <Typography variant="caption" color="text.disabled">
          Heart rate, power and pace traces will render here.
        </Typography>
      </Box>
    </Stack>
  );
}
