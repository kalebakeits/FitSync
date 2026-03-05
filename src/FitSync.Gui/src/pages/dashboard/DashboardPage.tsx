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
import { Brightness4, Brightness7, Logout, HelpOutline, Settings } from "@mui/icons-material";
import { useAuth } from "../../contexts/AuthContext";
import { useAppTheme } from "../../contexts/ThemeContext";
import ActivitiesPanel from "../../components/activities/ActivitiesPanel";
import ProfileSettingsModal from "../../components/profile/ProfileSettingsModal";
import SyncStatusIndicator from "../../components/dashboard/SyncStatusIndicator";
import FetcherStatusPanel from "../../components/dashboard/FetcherStatusPanel";
import SyncHelpModal from "../../components/notices/SyncHelpModal";
import Footer from "../../components/Footer";
import { useGetApiConnectionsStatus } from "../../api/generated/connections/connections";

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const { mode, toggleTheme } = useAppTheme();
  const navigate = useNavigate();
  const theme = useTheme();
  const isLandscape = useMediaQuery(theme.breakpoints.up("md"));
  const [helpOpen, setHelpOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [settingsTab, setSettingsTab] = useState(0);

  const { data: fetchers = [] } = useGetApiConnectionsStatus({ query: { refetchInterval: 10000 } });

  const openSettingsAt = (tab: number) => {
    setSettingsTab(tab);
    setSettingsOpen(true);
  };

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <AppBar position="static" elevation={2}>
        <Toolbar>
          <Typography variant="h5" component="div" sx={{ flexGrow: 1, fontWeight: "bold", letterSpacing: 0.5 }}>
            FitSync
          </Typography>
          <IconButton color="inherit" onClick={() => setHelpOpen(true)}>
            <HelpOutline />
          </IconButton>
          <IconButton color="inherit" onClick={() => openSettingsAt(0)}>
            <Settings />
          </IconButton>
          <IconButton color="inherit" onClick={toggleTheme}>
            {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
          <Button color="inherit" startIcon={<Logout />} onClick={() => { logout(); navigate("/login"); }} />
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ py: 3, flexGrow: 1 }}>
        <Box sx={{ mb: 2, display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <Typography variant="subtitle1" color="text.secondary">
            Welcome back, {user?.username}
          </Typography>
          <SyncStatusIndicator fetchers={fetchers} />
        </Box>

        <SyncHelpModal open={helpOpen} onClose={() => setHelpOpen(false)} />
        <ProfileSettingsModal
          key={settingsTab}
          open={settingsOpen}
          onClose={() => setSettingsOpen(false)}
          initialTab={settingsTab}
        />

        {isLandscape ? (
          <Box sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 3 }}>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
              <FetcherStatusPanel onOpenSettings={() => openSettingsAt(2)} />
            </Box>
            <Box>
              <ActivitiesPanel />
            </Box>
          </Box>
        ) : (
          <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
            <FetcherStatusPanel onOpenSettings={() => openSettingsAt(2)} />
            <ActivitiesPanel />
          </Box>
        )}
      </Container>
      <Footer />
    </Box>
  );
}
