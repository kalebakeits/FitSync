import { Box, Typography } from "@mui/material";

interface StatusIndicatorProps {
  enabled: boolean;
}

export default function StatusIndicator({ enabled }: StatusIndicatorProps) {
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
      <Box
        sx={{
          width: 8,
          height: 8,
          borderRadius: "50%",
          backgroundColor: enabled ? "#4caf50" : "#f44336",
          boxShadow: enabled
            ? "0 0 8px rgba(76, 175, 80, 0.8)"
            : "0 0 8px rgba(244, 67, 54, 0.8)",
        }}
      />
      <Typography
        variant="body2"
        sx={{
          color: enabled ? "#4caf50" : "#f44336",
          fontWeight: 500,
        }}
      >
        {enabled ? "Enabled" : "Disabled"}
      </Typography>
    </Box>
  );
}
