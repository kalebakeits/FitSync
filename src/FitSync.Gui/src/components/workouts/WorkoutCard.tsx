import { Card, CardContent, CardActions, Chip, IconButton, Stack, Typography, Box } from "@mui/material";
import { Delete, Download, Edit, Upload } from "@mui/icons-material";
import type { WorkoutResponse } from "../../api/generated/fitSyncApi.schemas";
import SportIcon from "./SportIcon";
import WorkoutPreview from "./WorkoutPreview";

interface WorkoutCardProps {
  workout: WorkoutResponse;
  onEdit: (workout: WorkoutResponse) => void;
  onDelete: (workout: WorkoutResponse) => void;
  onDownload: (workout: WorkoutResponse) => void;
  onPublish: (workout: WorkoutResponse) => void;
}

export default function WorkoutCard({ workout, onEdit, onDelete, onDownload, onPublish }: WorkoutCardProps) {
  return (
    <Card variant="outlined" sx={{ height: "100%", display: "flex", flexDirection: "column" }}>
      <CardContent sx={{ flexGrow: 1 }}>
        <Box sx={{ display: "flex", alignItems: "flex-start", gap: 1, mb: 1 }}>
          <SportIcon sport={workout.sport} sx={{ fontSize: 20, color: "text.secondary", mt: 0.3, flexShrink: 0 }} />
          <Typography variant="h6" fontWeight="medium" noWrap sx={{ flexGrow: 1 }}>
            {workout.name}
          </Typography>
        </Box>
        <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1 }}>
          {new Date(workout.createdAt!).toLocaleDateString()}
        </Typography>
        {(workout.tags ?? []).length > 0 && (
          <Stack direction="row" flexWrap="wrap" gap={0.5} sx={{ mb: 1 }}>
            {(workout.tags ?? []).map((tag) => (
              <Chip key={tag} label={tag} size="small" />
            ))}
          </Stack>
        )}
        <WorkoutPreview schema={workout.schema} />
      </CardContent>
      <CardActions sx={{ justifyContent: "flex-end" }}>
        <IconButton size="small" title="Publish to device" onClick={() => onPublish(workout)}>
          <Upload fontSize="small" />
        </IconButton>
        <IconButton size="small" onClick={() => onDownload(workout)}>
          <Download fontSize="small" />
        </IconButton>
        <IconButton size="small" onClick={() => onEdit(workout)}>
          <Edit fontSize="small" />
        </IconButton>
        <IconButton size="small" color="error" onClick={() => onDelete(workout)}>
          <Delete fontSize="small" />
        </IconButton>
      </CardActions>
    </Card>
  );
}
