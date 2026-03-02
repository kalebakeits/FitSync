import { List, Box, Typography } from "@mui/material";
import CredentialCard from "./CredentialCard";
import DashboardColumnText from "../dashboard/DashboardColumnText";
import DashboardColumnLoading from "../dashboard/DashboardColumnLoading";
import type { ConnectionResponse } from "../../api/generated/fitSyncApi.schemas";

interface CredentialsContentProps {
  connections: ConnectionResponse[];
  isLoading: boolean;
  isProcessing: boolean;
  onEdit: (connection: ConnectionResponse) => void;
  onDisconnect: (serviceType: string) => void;
}

export default function CredentialsContent({
  connections,
  isLoading,
  isProcessing,
  onEdit,
  onDisconnect,
}: CredentialsContentProps) {
  if (isLoading) {
    return <DashboardColumnLoading />;
  }

  return (
    <Box>
      <List sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
        {connections.map((connection) => (
          <CredentialCard
            key={connection.serviceType}
            serviceType={connection.serviceType ?? ""}
            displayName={connection.displayName}
            updatedAt={connection.updatedAt ?? ""}
            enabled={connection.enabled ?? true}
            onEdit={connection.authType !== "oauth" ? () => onEdit(connection) : undefined}
            onDisconnect={() => onDisconnect(connection.serviceType ?? "")}
            isProcessing={isProcessing}
          />
        ))}
      </List>
      {connections.length === 0 && (
        <DashboardColumnText>
          Add at least 2 services to get started.
        </DashboardColumnText>
      )}
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: "block", mt: 2, px: 2, pb: 2 }}
      >
        Note: Credentials are stored encrypted. Garmin accounts with 2FA
        enabled will not work.
      </Typography>
    </Box>
  );
}
