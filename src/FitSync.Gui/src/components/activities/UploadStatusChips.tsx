import { Box, Chip, Tooltip } from "@mui/material";
import type { UploadStatusEntry } from "../../api/generated/fitSyncApi.schemas";

const MAX_VISIBLE = 2;

function getChipColor(
  status: number,
): "default" | "warning" | "success" | "error" {
  if (status === 0) return "default"; // Pending
  if (status === 1 || status === 2) return "warning"; // Claimed / Processing
  if (status === 3 || status === 5) return "success"; // Uploaded / Already Exists
  return "error"; // Failed / ServiceUnavailable
}

function getStatusLabel(status: number): string {
  switch (status) {
    case 0:
      return "Pending";
    case 1:
      return "Claimed";
    case 2:
      return "Processing";
    case 3:
      return "Uploaded";
    case 4:
      return "Failed";
    case 5:
      return "Already Exists";
    case 6:
      return "Service Unavailable";
    default:
      return "Unknown";
  }
}

interface UploadStatusChipsProps {
  uploadStatuses: UploadStatusEntry[];
}

export default function UploadStatusChips({
  uploadStatuses,
}: UploadStatusChipsProps) {
  if (uploadStatuses.length === 0) {
    return null;
  }

  const visible = uploadStatuses.slice(0, MAX_VISIBLE);
  const overflow = uploadStatuses.slice(MAX_VISIBLE);

  return (
    <Box sx={{ display: "flex", gap: 0.5, flexWrap: "wrap", alignItems: "center" }}>
      {visible.map((u) => {
        const chipLabel = `${u.destinationServiceType}: ${getStatusLabel(u.status)}`;
        const tooltipTitle = u.lastError ?? getStatusLabel(u.status);
        return (
          <Tooltip key={u.destinationServiceType} title={tooltipTitle}>
            <Chip
              label={chipLabel}
              color={getChipColor(u.status)}
              size="small"
            />
          </Tooltip>
        );
      })}
      {overflow.length > 0 && (
        <Tooltip
          title={overflow
            .map((u) => `${u.destinationServiceType}: ${getStatusLabel(u.status)}`)
            .join(", ")}
        >
          <Chip label={`+${overflow.length}`} size="small" />
        </Tooltip>
      )}
    </Box>
  );
}
