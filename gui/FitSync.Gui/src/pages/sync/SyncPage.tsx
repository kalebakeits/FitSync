import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Box, IconButton, Stack, Typography } from "@mui/material";
import { HelpOutline } from "@mui/icons-material";
import ActivitiesPanel from "../../components/activities/ActivitiesPanel";
import SyncStatusIndicator from "../../components/dashboard/SyncStatusIndicator";
import FetcherStatusPanel from "../../components/dashboard/FetcherStatusPanel";
import SyncHelpModal from "../../components/notices/SyncHelpModal";
import { useGetApiConnectionsStatus } from "../../api/generated/connections/connections";

export default function SyncPage() {
  const navigate = useNavigate();
  const [helpOpen, setHelpOpen] = useState(false);

  const { data: fetchers = [] } = useGetApiConnectionsStatus({
    query: { refetchInterval: 10000 },
  });

  return (
    <Box sx={{ p: 3, width: "100%" }}>
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        flexWrap="wrap"
        gap={1}
        sx={{ mb: 2 }}
      >
        <Typography variant="h5" fontWeight="bold">
          Sync
        </Typography>
        <Stack direction="row" alignItems="center" spacing={1}>
          <IconButton
            size="small"
            aria-label="How syncing works"
            onClick={() => setHelpOpen(true)}
          >
            <HelpOutline />
          </IconButton>
          <SyncStatusIndicator fetchers={fetchers} />
        </Stack>
      </Stack>

      <SyncHelpModal open={helpOpen} onClose={() => setHelpOpen(false)} />

      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
          gap: 3,
          alignItems: "start",
        }}
      >
        <FetcherStatusPanel
          onOpenSettings={() => navigate("/settings?tab=integrations")}
        />
        <ActivitiesPanel />
      </Box>
    </Box>
  );
}
