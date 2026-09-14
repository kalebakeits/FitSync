export type SportCategory = "swim" | "bike" | "run" | "other";

const SWIM_CODES: number[] = [5, 85];
const BIKE_CODES: number[] = [2, 15, 21];
const RUN_CODES: number[] = [1, 11, 17];

export const AUTO_PUBLISH_CATEGORIES: SportCategory[] = ["swim", "bike", "run"];

const CATEGORY_LABELS: Record<SportCategory, string> = {
  swim: "Swim",
  bike: "Bike",
  run: "Run",
  other: "Other",
};

const CATEGORY_FIT_CODES: Record<SportCategory, number> = {
  swim: SWIM_CODES[0],
  bike: BIKE_CODES[0],
  run: RUN_CODES[0],
  other: 0,
};

export function sportCategoryLabel(category: SportCategory): string {
  return CATEGORY_LABELS[category];
}

export function sportCategoryFitCode(category: SportCategory): number {
  return CATEGORY_FIT_CODES[category];
}

export function sportCategory(
  fitCode: number | null | undefined,
): SportCategory {
  if (fitCode === null || fitCode === undefined) return "other";
  if (SWIM_CODES.includes(fitCode)) return "swim";
  if (BIKE_CODES.includes(fitCode)) return "bike";
  if (RUN_CODES.includes(fitCode)) return "run";
  return "other";
}
