import { Box } from "@mui/material";
import { DirectionsBike, Watch } from "@mui/icons-material";

interface ServiceIconProps {
  serviceType: string;
  size?: number;
}

const SERVICE_CONFIG: Record<string, { color: string; icon: React.ReactNode }> = {
  zwift: { color: "#FC6719", icon: null },
  garmin: { color: "#007DBC", icon: null },
  wahoo: { color: "#E61F26", icon: null },
};

export default function ServiceIcon({ serviceType, size = 40 }: ServiceIconProps) {
  const key = serviceType.toLowerCase();
  const color = SERVICE_CONFIG[key]?.color ?? "#757575";
  const iconSize = size * 0.55;

  const icon = (() => {
    switch (key) {
      case "zwift": return <DirectionsBike sx={{ fontSize: iconSize, color }} />;
      case "garmin": return <Watch sx={{ fontSize: iconSize, color }} />;
      case "wahoo": return <DirectionsBike sx={{ fontSize: iconSize, color }} />;
      default: return <DirectionsBike sx={{ fontSize: iconSize, color }} />;
    }
  })();

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        width: size,
        height: size,
        borderRadius: 1,
        backgroundColor: "action.hover",
        flexShrink: 0,
      }}
    >
      {icon}
    </Box>
  );
}
