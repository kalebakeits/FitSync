import { Box, Paper, Stack } from "@mui/material";

interface DashboardColumnProps {
  header: React.ReactElement;
  body: React.ReactElement;
}

export default function DashboardColumn({
  header,
  body,
}: DashboardColumnProps) {
  return (
    <Box sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
      <Stack spacing={2} sx={{ height: "100%" }}>
        {header}
        <Paper sx={{ flex: 1, overflow: "auto" }}>{body}</Paper>
      </Stack>
    </Box>
  );
}
