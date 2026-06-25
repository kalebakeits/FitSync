import { Tooltip, IconButton } from "@mui/material";
import { InfoOutlined } from "@mui/icons-material";

export default function InfoTooltip({ title }: { title: string }) {
  return (
    <Tooltip title={title} arrow placement="right">
      <IconButton size="small" sx={{ ml: 0.5, opacity: 0.5, "&:hover": { opacity: 1 } }}>
        <InfoOutlined fontSize="inherit" />
      </IconButton>
    </Tooltip>
  );
}
