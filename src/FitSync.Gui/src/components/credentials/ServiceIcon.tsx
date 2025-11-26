import { Box } from "@mui/material";
import { DirectionsBike, Watch } from "@mui/icons-material";

interface ServiceIconProps {
  serviceType: string;
  size?: number;
}

export default function ServiceIcon({
  serviceType,
  size = 40,
}: ServiceIconProps) {
  const getIcon = () => {
    switch (serviceType.toLowerCase()) {
      case "zwift":
        return <DirectionsBike sx={{ fontSize: size, color: "#FC6719" }} />;
      case "garmin":
        return <Watch sx={{ fontSize: size, color: "#007DBC" }} />;
      default:
        return <DirectionsBike sx={{ fontSize: size }} />;
    }
  };

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
      {getIcon()}
    </Box>
  );
}
