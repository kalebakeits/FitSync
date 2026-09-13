import { Box, Alert } from "@mui/material";

interface DashboardColumnErrorProps {
  children: React.ReactNode;
}

export default function DashboardColumnError({
  children,
}: DashboardColumnErrorProps) {
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
      <Alert severity="error">{children}</Alert>
    </Box>
  );
}
