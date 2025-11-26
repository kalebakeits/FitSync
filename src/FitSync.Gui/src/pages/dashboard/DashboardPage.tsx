import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Container,
  Box,
  Typography,
  AppBar,
  Toolbar,
  Button,
  IconButton,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import {
  Brightness4,
  Brightness7,
  Logout,
  HelpOutline,
  Settings,
} from "@mui/icons-material";
import { useAuth } from "../../contexts/AuthContext";
import { useAppTheme } from "../../contexts/ThemeContext";
import CredentialsManager from "../../components/credentials/CredentialsManager";
import ActivitiesPanel from "../../components/activities/ActivitiesPanel";
import ProfileSettingsModal from "../../components/profile/ProfileSettingsModal";
import SyncStatusIndicator from "../../components/dashboard/SyncStatusIndicator";
import SyncHelpModal from "../../components/notices/SyncHelpModal";
import Footer from "../../components/Footer";
import { useGetApiCredentials } from "../../api/generated/credentials/credentials";
import type { CredentialResponse } from "../../api/generated/fitSyncApi.schemas";

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const { mode, toggleTheme } = useAppTheme();
  const navigate = useNavigate();
  const theme = useTheme();
  const isLandscape = useMediaQuery(theme.breakpoints.up("md"));
  const [helpModalOpen, setHelpModalOpen] = useState(false);
  const [settingsModalOpen, setSettingsModalOpen] = useState(false);

  const { data: credentials } = useGetApiCredentials();
  const credentialsList =
    (credentials as unknown as CredentialResponse[]) || [];
  const enabledCount = credentialsList.filter((cred) => cred.enabled).length;
  const isSyncing = enabledCount >= 2;

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <AppBar position="static" elevation={2}>
        <Toolbar>
          <Typography
            variant="h5"
            component="div"
            sx={{ flexGrow: 1, fontWeight: "bold", letterSpacing: 0.5 }}
          >
            FitSync
          </Typography>
          <IconButton color="inherit" onClick={() => setHelpModalOpen(true)}>
            <HelpOutline />
          </IconButton>
          <IconButton
            color="inherit"
            onClick={() => setSettingsModalOpen(true)}
          >
            <Settings />
          </IconButton>
          <IconButton color="inherit" onClick={toggleTheme}>
            {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
          <Button
            color="inherit"
            startIcon={<Logout />}
            onClick={handleLogout}
          />
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ py: 3, flexGrow: 1 }}>
        <Box
          sx={{
            mb: 2,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Typography variant="subtitle1" color="text.secondary">
            Welcome back, {user?.username}
          </Typography>
          <SyncStatusIndicator isSyncing={isSyncing} />
        </Box>

        <SyncHelpModal
          open={helpModalOpen}
          onClose={() => setHelpModalOpen(false)}
        />
        <ProfileSettingsModal
          open={settingsModalOpen}
          onClose={() => setSettingsModalOpen(false)}
        />

        {isLandscape ? (
          <Box sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 3 }}>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
              <CredentialsManager />
            </Box>
            <Box>
              <ActivitiesPanel />
            </Box>
          </Box>
        ) : (
          <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
            <CredentialsManager />
            <ActivitiesPanel />
          </Box>
        )}
      </Container>
      <Footer />
    </Box>
  );
}
