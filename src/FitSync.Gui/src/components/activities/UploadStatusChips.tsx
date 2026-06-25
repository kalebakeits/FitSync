import { Box, Chip, Tooltip } from "@mui/material";
import type { UploadStatusEntry } from "../../api/generated/fitSyncApi.schemas";

const MAX_VISIBLE = 2;

type UploadStatusValue = NonNullable<UploadStatusEntry["status"]>;

function getChipColor(
  status: UploadStatusValue,
): "default" | "warning" | "success" | "error" {
  if (status === "Pending") return "default";
  if (status === "Claimed" || status === "Processing") return "warning";
  if (status === "Uploaded" || status === "Conflict") return "success";
  return "error";
}

function getStatusLabel(status: UploadStatusValue): string {
  if (status === "ServiceUnavailable") return "Service Unavailable";
  return status;
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
        const chipLabel = `${u.destinationServiceType}: ${getStatusLabel(u.status!)}`;
        const tooltipTitle = u.lastError ?? getStatusLabel(u.status!);
        return (
          <Tooltip key={u.destinationServiceType} title={tooltipTitle}>
            <Chip
              label={chipLabel}
              color={getChipColor(u.status!)}
              size="small"
            />
          </Tooltip>
        );
      })}
      {overflow.length > 0 && (
        <Tooltip
          title={overflow
            .map((u) => `${u.destinationServiceType}: ${getStatusLabel(u.status!)}`)
            .join(", ")}
        >
          <Chip label={`+${overflow.length}`} size="small" />
        </Tooltip>
      )}
    </Box>
  );
}
