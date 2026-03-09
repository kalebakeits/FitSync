import { Box, Typography } from "@mui/material";
import {
  type FetcherStatusResponse,
} from "../../api/generated/fitSyncApi.schemas";
import ServiceIcon from "../credentials/ServiceIcon";
import TrafficLight from "./TrafficLight";

type FetcherStatus = "green" | "amber" | "red" | "grey";

function toFetcherStatus(status?: string): FetcherStatus {
  if (status === "green" || status === "amber" || status === "red" || status === "grey") {
    return status;
  }

  return "red";
}

const reasonLabels: Partial<Record<FetcherStatusResponse["reason"], string>> = {
  FetcherUnhealthy: "Fetcher error",
  NoDestinations: "No destinations",
  AllDestinationsUnhealthy: "All destinations unhealthy",
  SomeDestinationsUnhealthy: "Some destinations unhealthy",
  None: "Live",
};

const statusColors: Record<FetcherStatus, string> = {
  green: "#4caf50",
  amber: "#ff9800",
  red: "#f44336",
  grey: "#9e9e9e",
};

interface FetcherRowProps {
  fetcher: FetcherStatusResponse;
}

export default function FetcherRow({ fetcher }: FetcherRowProps) {
  const status = toFetcherStatus(fetcher.status);
  const reasonLabel = reasonLabels[fetcher.reason] || "Unknown Status";

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        py: 1,
      }}
    >
      <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
        <ServiceIcon serviceType={fetcher.serviceType ?? ""} size={28} />
        <Box>
          <Typography variant="body2" fontWeight={500}>
            {fetcher.serviceType}
          </Typography>
          {reasonLabel && (
            <Typography variant="caption" color="text.secondary">
              {reasonLabel}
            </Typography>
          )}
        </Box>
      </Box>
      <TrafficLight
        color={statusColors[status]}
        label={reasonLabel}
      />
    </Box>
  );
}
