import { Box, IconButton, Typography } from "@mui/material";
import { IconPlus } from "@tabler/icons-react";

interface DayCellHeaderProps {
  date: string;
  label: string;
  isToday: boolean;
  isOther: boolean;
  showAdd: boolean;
  onAdd: (date: string) => void;
}

export default function DayCellHeader({
  date,
  label,
  isToday,
  isOther,
  showAdd,
  onAdd,
}: DayCellHeaderProps) {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        width: "100%",
        gap: 0.5,
        "& .DayCellHeader-add": { opacity: 0, transition: "opacity 120ms" },
        "&:hover .DayCellHeader-add, & .DayCellHeader-add:focus-visible": {
          opacity: 1,
        },
      }}
    >
      <Typography
        variant="caption"
        sx={{
          fontWeight: isToday ? 700 : 500,
          color: isOther ? "text.disabled" : "text.secondary",
        }}
      >
        {label}
      </Typography>

      {showAdd && (
        <IconButton
          className="DayCellHeader-add"
          size="small"
          aria-label={`Schedule a workout on ${date}`}
          onClick={(event) => {
            event.stopPropagation();
            onAdd(date);
          }}
          sx={{ p: 0.25 }}
        >
          <IconPlus size={14} />
        </IconButton>
      )}
    </Box>
  );
}
