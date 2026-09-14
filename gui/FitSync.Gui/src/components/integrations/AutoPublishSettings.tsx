import { Box, ToggleButtonGroup, Typography } from "@mui/material";
import { useAutoPublishSettings } from "../../hooks/useAutoPublishSettings";
import { AUTO_PUBLISH_CATEGORIES } from "../../utils/sportCategory";
import type { SportCategory } from "../../utils/sportCategory";
import SportCategoryToggle from "./SportCategoryToggle";

interface AutoPublishSettingsProps {
  serviceType: string;
}

export default function AutoPublishSettings({
  serviceType,
}: AutoPublishSettingsProps) {
  const { enabledCategories, save, isLoading, isError, isSaving } =
    useAutoPublishSettings(serviceType);

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        flexWrap: "wrap",
        gap: 1.5,
        px: 2,
        pb: 0.5,
      }}
    >
      <Typography variant="body2" color="text.secondary">
        Auto-publish
      </Typography>
      <ToggleButtonGroup
        size="small"
        value={enabledCategories}
        disabled={isLoading || isError || isSaving}
        aria-label={`Auto-publish sports for ${serviceType}`}
        onChange={(_event, next: SportCategory[]) => save(next)}
      >
        {AUTO_PUBLISH_CATEGORIES.map((category) => (
          <SportCategoryToggle key={category} category={category} />
        ))}
      </ToggleButtonGroup>
      {isError && (
        <Typography variant="caption" color="error.main">
          Could not load auto-publish settings
        </Typography>
      )}
    </Box>
  );
}
