import { Box, CircularProgress } from "@mui/material";

export default function DashboardColumnLoading() {
  return (
    <Box
      sx={{
        flexGrow: 1,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        height: "100%",
        p: 3,
      }}
    >
      <CircularProgress />
    </Box>
  );
}
