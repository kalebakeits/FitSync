import { List, Box, Typography } from "@mui/material";
import CredentialCard from "./CredentialCard";
import DashboardColumnText from "../dashboard/DashboardColumnText";
import DashboardColumnLoading from "../dashboard/DashboardColumnLoading";

interface Credential {
  serviceType: string;
  username: string;
  createdAt: string;
  updatedAt: string;
  enabled: boolean;
}

interface CredentialsContentProps {
  credentials: Credential[];
  isLoading: boolean;
  isDeleting: boolean;
  onEdit: (credential: Credential) => void;
  onDelete: (serviceType: string) => void;
}

export default function CredentialsContent({
  credentials,
  isLoading,
  isDeleting,
  onEdit,
  onDelete,
}: CredentialsContentProps) {
  if (isLoading) {
    return <DashboardColumnLoading />;
  }

  if (credentials.length === 0) {
    return (
      <DashboardColumnText>
        Add at least 2 services to get started.
      </DashboardColumnText>
    );
  }

  return (
    <Box>
      <List sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
        {credentials.map((credential) => (
          <CredentialCard
            key={credential.serviceType}
            serviceType={credential.serviceType}
            username={credential.username}
            updatedAt={credential.updatedAt}
            enabled={credential.enabled}
            onEdit={() => onEdit(credential)}
            onDelete={() => onDelete(credential.serviceType)}
            isDeleting={isDeleting}
          />
        ))}
      </List>
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: "block", mt: 2, px: 2, pb: 2 }}
      >
        Note: Credentials are stored encrypted. Accounts with 2FA enabled will
        not work.
      </Typography>
    </Box>
  );
}
