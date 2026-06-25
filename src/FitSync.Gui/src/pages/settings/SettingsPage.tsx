import { useSearchParams, useNavigate } from "react-router-dom";
import {
  Box,
  Divider,
  List,
  ListItemButton,
  ListItemText,
  Typography,
  Button,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import { DeleteForever } from "@mui/icons-material";
import UpdateUsernameForm from "../../components/profile/UpdateUsernameForm";
import UpdateEmailForm from "../../components/profile/UpdateEmailForm";
import UpdatePasswordForm from "../../components/profile/UpdatePasswordForm";
import TrainingProfileForm from "../../components/profile/TrainingProfileForm";
import IntegrationsTab from "../../components/integrations/IntegrationsTab";
import McpTab from "../../components/mcp/McpTab";
import DeleteAccountModal from "../../components/account/DeleteAccountModal";
import { useState } from "react";
import { useDeleteApiAccount } from "../../api/generated/account/account";
import { useAuth } from "../../contexts/AuthContext";

export const SETTINGS_TABS = [
  { key: "account", label: "Account" },
  { key: "password", label: "Password" },
  { key: "integrations", label: "Integrations" },
  { key: "training", label: "Training Profile" },
  { key: "mcp", label: "MCP" },
];

export default function SettingsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [deleteOpen, setDeleteOpen] = useState(false);
  const navigate = useNavigate();
  const { logout } = useAuth();

  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));
  const activeTab = searchParams.get("tab") ?? "account";

  const deleteMutation = useDeleteApiAccount({
    mutation: {
      onSuccess: () => {
        logout();
        navigate("/login");
      },
    },
  });

  const setTab = (key: string) => setSearchParams({ tab: key }, { replace: true });

  return (
    <Box sx={{ display: "flex", height: "100%", minHeight: 0 }}>
      {!isMobile && (
        <Box
          sx={{
            width: 200,
            flexShrink: 0,
            borderRight: 1,
            borderColor: "divider",
            display: "flex",
            flexDirection: "column",
          }}
        >
          <Typography variant="overline" sx={{ px: 2, pt: 2, pb: 1, color: "text.secondary" }}>
            Settings
          </Typography>
          <List dense disablePadding sx={{ flexGrow: 1 }}>
            {SETTINGS_TABS.map((t) => (
              <ListItemButton
                key={t.key}
                selected={activeTab === t.key}
                onClick={() => setTab(t.key)}
                sx={{ borderRadius: 1, mx: 1, mb: 0.5 }}
              >
                <ListItemText primary={t.label} />
              </ListItemButton>
            ))}
          </List>
        </Box>
      )}

      <Box sx={{ flexGrow: 1, overflowY: "auto", p: 4, maxWidth: 640 }}>
        {activeTab === "account" && (
          <Box sx={{ display: "flex", flexDirection: "column", gap: 4 }}>
            <UpdateUsernameForm />
            <Divider />
            <UpdateEmailForm />
            <Divider />
            <Button
              variant="outlined"
              color="error"
              startIcon={<DeleteForever />}
              onClick={() => setDeleteOpen(true)}
            >
              Delete Account
            </Button>
          </Box>
        )}
        {activeTab === "password" && <UpdatePasswordForm />}
        {activeTab === "integrations" && <IntegrationsTab />}
        {activeTab === "training" && <TrainingProfileForm />}
        {activeTab === "mcp" && <McpTab />}
      </Box>

      <DeleteAccountModal
        open={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        onConfirm={() => deleteMutation.mutate()}
        isDeleting={deleteMutation.isPending}
        error={deleteMutation.isError ? "Failed to delete account. Please try again." : undefined}
      />
    </Box>
  );
}
