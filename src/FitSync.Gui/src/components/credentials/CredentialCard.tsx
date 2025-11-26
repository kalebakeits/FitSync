import { ListItem, Fab, Tooltip, Box, Typography } from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import ServiceIcon from "./ServiceIcon";
import StatusIndicator from "./StatusIndicator";

interface CredentialCardProps {
  serviceType: string;
  username: string;
  updatedAt: string;
  enabled: boolean;
  onEdit: () => void;
  onDelete: () => void;
  isDeleting: boolean;
}

export default function CredentialCard({
  serviceType,
  username,
  updatedAt,
  enabled,
  onEdit,
  onDelete,
  isDeleting,
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
        mx: 2,
        width: "auto",
        py: 1.5,
        px: 2,
      }}
    >
      {/* Top Row: Service Name and Action Buttons */}
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
          <Tooltip title="Edit credentials">
            <Fab
              color="primary"
              onClick={onEdit}
              disabled={isDeleting}
              size="small"
              sx={{
                width: 25,
                height: 25,
                minHeight: 25,
              }}
            >
              <Edit fontSize="small" />
            </Fab>
          </Tooltip>
          <Tooltip title="Delete credentials">
            <Fab
              color="error"
              onClick={onDelete}
              disabled={isDeleting}
              size="small"
              sx={{
                width: 25,
                height: 25,
                minHeight: 25,
                mx: 0.5,
              }}
            >
              <Delete fontSize="small" />
            </Fab>
          </Tooltip>
        </Box>
      </Box>

      {/* Bottom Row: Icon and Details */}
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
            Username: {username}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Last updated: {new Date(updatedAt).toLocaleDateString()}
          </Typography>
        </Box>
      </Box>
    </ListItem>
  );
}
