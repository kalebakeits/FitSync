import { Box, Typography } from "@mui/material";
import type { FetcherStatusResponse } from "../../api/generated/fitSyncApi.schemas";
import ServiceIcon from "../credentials/ServiceIcon";
import TrafficLight from "./TrafficLight";

type FetcherStatus = "green" | "amber" | "red";

interface FetcherRowProps {
  fetcher: FetcherStatusResponse;
}

export default function FetcherRow({ fetcher }: FetcherRowProps) {
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
        <Typography variant="body2" fontWeight={500}>
          {fetcher.serviceType}
        </Typography>
      </Box>
      <TrafficLight status={(fetcher.status ?? "red") as FetcherStatus} />
    </Box>
  );
}
