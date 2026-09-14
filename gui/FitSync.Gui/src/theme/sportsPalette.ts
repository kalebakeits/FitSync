import type { PaletteMode } from "@mui/material";
import type { SportsPalette } from "./palette";

const LIGHT_SPORTS: SportsPalette = {
  swim: { main: "#0891b2", contrastText: "#ffffff" },
  bike: { main: "#16a34a", contrastText: "#ffffff" },
  run: { main: "#ea580c", contrastText: "#ffffff" },
  other: { main: "#64748b", contrastText: "#ffffff" },
};

const DARK_SPORTS: SportsPalette = {
  swim: { main: "#22d3ee", contrastText: "#ffffff" },
  bike: { main: "#4ade80", contrastText: "#ffffff" },
  run: { main: "#fb923c", contrastText: "#ffffff" },
  other: { main: "#94a3b8", contrastText: "#ffffff" },
};

export function sportsPalette(mode: PaletteMode): SportsPalette {
  return mode === "dark" ? DARK_SPORTS : LIGHT_SPORTS;
}
