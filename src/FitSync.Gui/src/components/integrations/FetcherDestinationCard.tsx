import { Box, Chip, Typography } from "@mui/material";
import { Warning } from "@mui/icons-material";
import type {
  AvailableServiceResponse,
  ConnectionResponse,
} from "../../api/generated/fitSyncApi.schemas";
import DestinationToggle from "./DestinationToggle";

interface FetcherDestinationCardProps {
  fetcherServiceType: string;
  uploaders: AvailableServiceResponse[];
  connections: ConnectionResponse[];
  enabledDestinations: string[];
  isPending: boolean;
  onToggle: (dest: string, enabled: boolean) => void;
}

export default function FetcherDestinationCard({
  fetcherServiceType,
  uploaders,
  connections,
  enabledDestinations,
  isPending,
  onToggle,
}: FetcherDestinationCardProps) {
  const hasDestinations = enabledDestinations.length > 0;

  return (
    <Box
      sx={{
        border: 1,
        borderColor: hasDestinations ? "divider" : "warning.main",
        borderRadius: 1,
        p: 2,
      }}
    >
      <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
        <Typography variant="subtitle2" fontWeight="bold">
          {fetcherServiceType}
        </Typography>
        {!hasDestinations && (
          <Chip icon={<Warning />} label="No destinations" color="warning" size="small" />
        )}
      </Box>
      {uploaders.map((u) => (
        <DestinationToggle
          key={u.serviceType}
          serviceType={u.serviceType ?? ""}
          enabled={enabledDestinations.includes(u.serviceType ?? "")}
          connected={connections.some((c) => c.serviceType === u.serviceType)}
          disabled={isPending}
          onChange={(enabled) => onToggle(u.serviceType ?? "", enabled)}
        />
      ))}
    </Box>
  );
}
