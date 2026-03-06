import { Box, Button, Divider, Typography } from "@mui/material";
import { Settings } from "@mui/icons-material";
import { useGetApiConnectionsStatus } from "../../api/generated/connections/connections";
import DashboardColumn from "./DashboardColumn";
import FetcherRow from "./FetcherRow";
import ResponsiveButton from "../ResponsiveButton";

interface FetcherStatusPanelProps {
  onOpenSettings: () => void;
}

export default function FetcherStatusPanel({ onOpenSettings }: FetcherStatusPanelProps) {
  const { data: fetchers = [] } = useGetApiConnectionsStatus({ query: { refetchInterval: 10000 } });

  return (
    <DashboardColumn
      header={
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <Typography variant="h5">Fetchers</Typography>
          <ResponsiveButton icon={<Settings fontSize="small" />} label="Manage" onClick={onOpenSettings} />
        </Box>
      }
      body={
        fetchers.length === 0 ? (
          <Box sx={{ p: 2 }}>
            <Typography variant="body2" color="text.secondary">
              No fetchers connected yet.{" "}
              <Button size="small" onClick={onOpenSettings}>
                Set up integrations
              </Button>
            </Typography>
          </Box>
        ) : (
          <Box sx={{ px: 2, py: 1 }}>
            {fetchers.map((f, i) => (
              <Box key={f.serviceType}>
                <FetcherRow fetcher={f} />
                {i < fetchers.length - 1 && <Divider />}
              </Box>
            ))}
          </Box>
        )
      }
    />
  );
}
