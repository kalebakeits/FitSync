import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Box, Typography } from "@mui/material";
import { useTheme, useMediaQuery } from "@mui/material";
import { HelpOutline } from "@mui/icons-material";
import { IconButton } from "@mui/material";
import { useAuth } from "../../contexts/AuthContext";
import AppLayout from "../../components/layout/AppLayout";
import ActivitiesPanel from "../../components/activities/ActivitiesPanel";
import SyncStatusIndicator from "../../components/dashboard/SyncStatusIndicator";
import FetcherStatusPanel from "../../components/dashboard/FetcherStatusPanel";
import SyncHelpModal from "../../components/notices/SyncHelpModal";
import Footer from "../../components/Footer";
import { useGetApiConnectionsStatus } from "../../api/generated/connections/connections";

export default function DashboardPage() {
  const { user } = useAuth();
  const theme = useTheme();
  const isLandscape = useMediaQuery(theme.breakpoints.up("md"));
  const [helpOpen, setHelpOpen] = useState(false);
  const navigate = useNavigate();

  const { data: fetchers = [] } = useGetApiConnectionsStatus({ query: { refetchInterval: 10000 } });

  return (
    <AppLayout>
      <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100%", p: 3, width: "100%" }}>
        <Box sx={{ mb: 2, display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <Typography variant="subtitle1" color="text.secondary">
            Welcome back, {user?.username}
          </Typography>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <IconButton onClick={() => setHelpOpen(true)}>
              <HelpOutline />
            </IconButton>
            <SyncStatusIndicator fetchers={fetchers} />
          </Box>
        </Box>

        <SyncHelpModal open={helpOpen} onClose={() => setHelpOpen(false)} />

        {isLandscape ? (
          <Box sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 3 }}>
            <FetcherStatusPanel onOpenSettings={() => navigate("/settings?tab=integrations")} />
            <ActivitiesPanel />
          </Box>
        ) : (
          <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
            <FetcherStatusPanel onOpenSettings={() => navigate("/settings?tab=integrations")} />
            <ActivitiesPanel />
          </Box>
        )}

        <Footer />
      </Box>
    </AppLayout>
  );
}
