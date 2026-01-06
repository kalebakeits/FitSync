import { Box, Typography, Button } from "@mui/material";
import { Refresh, Sync } from "@mui/icons-material";

interface ActivitiesHeaderProps {
  onRefreshClick: () => void;
  onTriggerFetch?: () => void;
  isTriggeringFetch?: boolean;
}

export default function ActivitiesHeader({
  onRefreshClick,
  onTriggerFetch,
  isTriggeringFetch = false,
}: ActivitiesHeaderProps) {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        gap: 1,
      }}
    >
      <Typography variant="h5">Imported Activities</Typography>
      <Box sx={{ display: "flex", gap: 1 }}>
        {onTriggerFetch && (
          <Button
            variant="outlined"
            startIcon={<Sync />}
            onClick={onTriggerFetch}
            disabled={isTriggeringFetch}
          >
            {isTriggeringFetch ? "Syncing..." : "Sync Now"}
          </Button>
        )}
        <Button
          variant="contained"
          startIcon={<Refresh />}
          onClick={onRefreshClick}
        >
          Refresh
        </Button>
      </Box>
    </Box>
  );
}
