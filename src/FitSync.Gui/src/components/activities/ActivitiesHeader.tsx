import { Box, Typography } from "@mui/material";
import { Refresh, Sync } from "@mui/icons-material";
import ResponsiveButton from "../ResponsiveButton";

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
    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 1 }}>
      <Typography variant="h5">Imported Activities</Typography>
      <Box sx={{ display: "flex", gap: 1 }}>
        {onTriggerFetch && (
          <ResponsiveButton
            icon={<Sync fontSize="small" />}
            label={isTriggeringFetch ? "Syncing..." : "Sync Now"}
            variant="outlined"
            onClick={onTriggerFetch}
            disabled={isTriggeringFetch}
          />
        )}
        <ResponsiveButton
          icon={<Refresh fontSize="small" />}
          label="Refresh"
          variant="contained"
          onClick={onRefreshClick}
        />
      </Box>
    </Box>
  );
}
