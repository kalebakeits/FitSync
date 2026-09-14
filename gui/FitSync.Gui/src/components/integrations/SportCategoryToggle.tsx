import { ToggleButton } from "@mui/material";
import { useSportColour } from "../../hooks/useSportColour";
import {
  sportCategoryFitCode,
  sportCategoryLabel,
} from "../../utils/sportCategory";
import type { SportCategory } from "../../utils/sportCategory";

interface SportCategoryToggleProps {
  category: SportCategory;
}

export default function SportCategoryToggle({
  category,
}: SportCategoryToggleProps) {
  const { main, contrastText } = useSportColour(sportCategoryFitCode(category));

  return (
    <ToggleButton
      value={category}
      aria-label={`Auto-publish ${sportCategoryLabel(category)}`}
      sx={{
        "&.Mui-selected": { bgcolor: main, color: contrastText },
        "&.Mui-selected:hover": { bgcolor: main },
      }}
    >
      {sportCategoryLabel(category)}
    </ToggleButton>
  );
}
