import { Box, Typography } from "@mui/material";

interface SyncStatusIndicatorProps {
  isSyncing: boolean;
}

export default function SyncStatusIndicator({
  isSyncing,
}: SyncStatusIndicatorProps) {
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
      <Box
        sx={{
          width: 10,
          height: 10,
          borderRadius: "50%",
          backgroundColor: isSyncing ? "#4caf50" : "#f44336",
          boxShadow: isSyncing
            ? "0 0 10px rgba(76, 175, 80, 0.8)"
            : "0 0 10px rgba(244, 67, 54, 0.8)",
        }}
      />
      <Typography
        variant="body1"
        sx={{
          color: isSyncing ? "#4caf50" : "#f44336",
          fontWeight: 500,
        }}
      >
        {isSyncing ? "Syncing" : "Not Syncing"}
      </Typography>
    </Box>
  );
}
