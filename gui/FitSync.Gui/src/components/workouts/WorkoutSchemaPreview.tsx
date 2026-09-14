import { Box, Typography } from "@mui/material";
import WorkoutPreview from "./WorkoutPreview";

interface WorkoutSchemaPreviewProps {
  schema: unknown;
}

function schemaName(schema: unknown): string | null {
  if (typeof schema !== "object" || schema === null) return null;
  const name = "name" in schema ? schema.name : null;
  return typeof name === "string" && name.trim() ? name : null;
}

export default function WorkoutSchemaPreview({
  schema,
}: WorkoutSchemaPreviewProps) {
  const name = schemaName(schema);

  return (
    <Box sx={{ mt: 2 }}>
      {name && (
        <Typography variant="subtitle1" fontWeight="medium" noWrap>
          {name}
        </Typography>
      )}
      <WorkoutPreview schema={schema} />
    </Box>
  );
}
