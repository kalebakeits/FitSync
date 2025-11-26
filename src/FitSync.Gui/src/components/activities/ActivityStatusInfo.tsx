import {
  CheckCircle,
  CloudDone,
  Error,
  HourglassEmpty,
  Upload,
} from "@mui/icons-material";

export const getStatusInfo = (status: number) => {
  switch (status) {
    case 0:
      return {
        label: "Pending",
        color: "default" as const,
        icon: <HourglassEmpty fontSize="small" />,
      };
    case 1:
      return {
        label: "Fetched",
        color: "info" as const,
        icon: <CloudDone fontSize="small" />,
      };
    case 2:
      return {
        label: "Uploading",
        color: "warning" as const,
        icon: <Upload fontSize="small" />,
      };
    case 3:
      return {
        label: "Completed",
        color: "success" as const,
        icon: <CheckCircle fontSize="small" />,
      };
    case 4:
      return {
        label: "Error",
        color: "error" as const,
        icon: <Error fontSize="small" />,
      };
    default:
      return {
        label: "Unknown",
        color: "default" as const,
        icon: undefined,
      };
  }
};
