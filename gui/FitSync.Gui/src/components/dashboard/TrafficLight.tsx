import { Box, Typography } from "@mui/material";

interface TrafficLightProps {
  color: string;
  label: string;
}

export default function TrafficLight({ color, label }: TrafficLightProps) {
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
      <Box
        sx={{
          width: 10,
          height: 10,
          borderRadius: "50%",
          backgroundColor: color,
          boxShadow: `0 0 8px ${color}cc`,
        }}
      />
      <Typography variant="body2" sx={{ color, fontWeight: 500 }}>
        {label}
      </Typography>
    </Box>
  );
}
