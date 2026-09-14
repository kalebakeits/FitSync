import { useTheme } from "@mui/material";
import { sportCategory } from "../utils/sportCategory";
import type { SportCategory } from "../utils/sportCategory";
import type { SportColour } from "../theme/palette";

export interface SportColourResult extends SportColour {
  category: SportCategory;
}

export function useSportColour(
  fitCode: number | null | undefined,
): SportColourResult {
  const theme = useTheme();
  const category = sportCategory(fitCode);
  const { main, contrastText } = theme.palette.sports[category];

  return { main, contrastText, category };
}
