export interface SportColour {
  main: string;
  contrastText: string;
}

export interface SportsPalette {
  swim: SportColour;
  bike: SportColour;
  run: SportColour;
  other: SportColour;
}

declare module "@mui/material/styles" {
  interface Palette {
    sports: SportsPalette;
  }

  interface PaletteOptions {
    sports?: SportsPalette;
  }
}
