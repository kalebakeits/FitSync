import { Box, Typography } from "@mui/material";
import type { FetcherStatusResponse } from "../../api/generated/fitSyncApi.schemas";

interface SyncStatusIndicatorProps {
  fetchers: FetcherStatusResponse[];
}

function computeOverall(fetchers: FetcherStatusResponse[]): "green" | "amber" | "red" {
  if (fetchers.length === 0 || fetchers.every((f) => f.status === "grey" || f.status === "red")) return "red";
  if (fetchers.every((f) => f.status === "green")) return "green";
  return "amber";
}

const labels = { green: "Syncing", amber: "Partial Sync", red: "Not Syncing" };
const colors = { green: "#4caf50", amber: "#ff9800", red: "#f44336" };

export default function SyncStatusIndicator({ fetchers }: SyncStatusIndicatorProps) {
  const status = computeOverall(fetchers);
  const color = colors[status];

  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
      <Box
        sx={{
          width: 10,
          height: 10,
          borderRadius: "50%",
          backgroundColor: color,
          boxShadow: `0 0 10px ${color}cc`,
        }}
      />
      <Typography variant="body1" sx={{ color, fontWeight: 500 }}>
        {labels[status]}
      </Typography>
    </Box>
  );
}
