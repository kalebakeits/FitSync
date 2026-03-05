import { Box, Typography } from "@mui/material";

const colorMap = {
  green: { hex: "#4caf50", label: "All destinations healthy" },
  amber: { hex: "#ff9800", label: "Some destinations failing" },
  red: { hex: "#f44336", label: "All destinations failing" },
  grey: { hex: "#9e9e9e", label: "No destinations configured" },
} as const;

interface TrafficLightProps {
  status: keyof typeof colorMap;
}

export default function TrafficLight({ status }: TrafficLightProps) {
  const { hex, label } = colorMap[status];
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
      <Box
        sx={{
          width: 10,
          height: 10,
          borderRadius: "50%",
          backgroundColor: hex,
          boxShadow: `0 0 8px ${hex}cc`,
        }}
      />
      <Typography variant="body2" sx={{ color: hex, fontWeight: 500 }}>
        {label}
      </Typography>
    </Box>
  );
}
