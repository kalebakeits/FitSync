import { Box } from "@mui/material";
import { IconPlus } from "@tabler/icons-react";

export default function AddSlotRow() {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: 26,
        borderRadius: 0.5,
        color: "text.disabled",
        cursor: "pointer",
        transition: "color 120ms, background-color 120ms",
        "&:hover": {
          color: "primary.main",
          bgcolor: "action.hover",
        },
      }}
    >
      <IconPlus size={20} stroke={1.75} />
    </Box>
  );
}
