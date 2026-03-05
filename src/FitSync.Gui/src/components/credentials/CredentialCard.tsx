import { ListItem, IconButton, Tooltip, Box, Typography } from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import ServiceIcon from "./ServiceIcon";
import StatusIndicator from "./StatusIndicator";

interface CredentialCardProps {
  serviceType: string;
  displayName?: string | null;
  updatedAt: string;
  enabled: boolean;
  onEdit?: () => void;
  onDisconnect: () => void;
  isProcessing: boolean;
}

export default function CredentialCard({
  serviceType,
  displayName,
  updatedAt,
  enabled,
  onEdit,
  onDisconnect,
  isProcessing,
}: CredentialCardProps) {
  return (
    <ListItem
      sx={{
        border: 1,
        borderColor: "divider",
        borderRadius: 1,
        display: "flex",
        flexDirection: "column",
        alignItems: "stretch",
        gap: 0.75,
        my: 0.5,
        width: "auto",
        py: 1.5,
        px: 2,
      }}
    >
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          height: "fit-content",
        }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
          <Typography variant="h6" component="div" sx={{ fontWeight: "bold" }}>
            {serviceType}
          </Typography>
          <StatusIndicator enabled={enabled} />
        </Box>
        <Box sx={{ display: "flex", gap: 0.5 }}>
          {onEdit && (
            <Tooltip title="Edit credentials">
              <IconButton color="primary" onClick={onEdit} disabled={isProcessing} size="small">
                <Edit fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          <Tooltip title="Disconnect">
            <IconButton color="error" onClick={onDisconnect} disabled={isProcessing} size="small">
              <Delete fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: "auto 1fr",
          gap: 2,
          alignItems: "center",
        }}
      >
        <ServiceIcon serviceType={serviceType} />
        <Box sx={{ minWidth: 0 }}>
          {displayName && (
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
                mb: 0.25,
              }}
            >
              Username: {displayName}
            </Typography>
          )}
          <Typography variant="caption" color="text.secondary">
            Last updated: {new Date(updatedAt).toLocaleDateString()}
          </Typography>
        </Box>
      </Box>
    </ListItem>
  );
}
