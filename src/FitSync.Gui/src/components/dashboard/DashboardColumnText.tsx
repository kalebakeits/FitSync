import { Alert, Box } from "@mui/material";

interface DashboardColumnTextProps {
  children: React.ReactNode;
}

export default function DashboardColumnText({
  children,
}: DashboardColumnTextProps) {
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
      <Alert severity="info">{children}</Alert>
    </Box>
  );
}
