import { Box, Button, List, Typography } from "@mui/material";
import { Add } from "@mui/icons-material";
import type { ConnectionResponse } from "../../api/generated/fitSyncApi.schemas";
import CredentialCard from "../credentials/CredentialCard";

interface ConnectedServicesSectionProps {
  connections: ConnectionResponse[];
  isProcessing: boolean;
  onAdd: () => void;
  onEdit: (connection: ConnectionResponse) => void;
  onDisconnect: (serviceType: string) => void;
}

export default function ConnectedServicesSection({
  connections,
  isProcessing,
  onAdd,
  onEdit,
  onDisconnect,
}: ConnectedServicesSectionProps) {
  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
        <Typography variant="h6">Connected Services</Typography>
        <Button size="small" variant="contained" startIcon={<Add />} onClick={onAdd}>
          Add
        </Button>
      </Box>
      <List sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
        {connections.map((c) => (
          <CredentialCard
            key={c.serviceType}
            serviceType={c.serviceType ?? ""}
            displayName={c.displayName}
            updatedAt={c.updatedAt ?? ""}
            enabled={c.enabled ?? true}
            onEdit={c.authType !== "oauth" ? () => onEdit(c) : undefined}
            onDisconnect={() => onDisconnect(c.serviceType ?? "")}
            isProcessing={isProcessing}
          />
        ))}
      </List>
    </Box>
  );
}
