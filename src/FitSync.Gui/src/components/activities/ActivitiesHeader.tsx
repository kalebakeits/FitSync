import { Box, Typography, Button } from "@mui/material";
import { Refresh } from "@mui/icons-material";

interface ActivitiesHeaderProps {
  onRefreshClick: () => void;
}

export default function ActivitiesHeader({
  onRefreshClick,
}: ActivitiesHeaderProps) {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
      }}
    >
      <Typography variant="h5">Imported Activities</Typography>
      <Button
        variant="contained"
        startIcon={<Refresh />}
        onClick={onRefreshClick}
      >
        Refresh
      </Button>
    </Box>
  );
}
