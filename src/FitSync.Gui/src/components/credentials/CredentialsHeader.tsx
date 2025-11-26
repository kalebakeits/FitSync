import { Box, Typography, Button } from "@mui/material";
import { Add } from "@mui/icons-material";

interface CredentialsHeaderProps {
  onAddClick: () => void;
}

export default function CredentialsHeader({
  onAddClick,
}: CredentialsHeaderProps) {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
      }}
    >
      <Typography variant="h5">Service Credentials</Typography>
      <Button variant="contained" startIcon={<Add />} onClick={onAddClick}>
        Add New
      </Button>
    </Box>
  );
}
