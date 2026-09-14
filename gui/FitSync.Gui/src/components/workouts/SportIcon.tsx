import { SvgIcon } from "@mui/material";
import type { SvgIconProps } from "@mui/material";
import {
  IconBike,
  IconRun,
  IconStretching,
  IconSwimming,
} from "@tabler/icons-react";
import { sportCategory } from "../../utils/sportCategory";
import type { SportCategory } from "../../utils/sportCategory";

const SPORT_ICONS: Record<SportCategory, typeof IconSwimming> = {
  swim: IconSwimming,
  bike: IconBike,
  run: IconRun,
  other: IconStretching,
};

interface SportIconProps extends SvgIconProps {
  sport?: number | null;
  size?: number;
}

export default function SportIcon({
  sport,
  size = 20,
  sx,
  ...props
}: SportIconProps) {
  const Icon = SPORT_ICONS[sportCategory(sport)];

  return (
    <SvgIcon sx={{ fontSize: size, ...sx }} {...props}>
      <Icon size="100%" />
    </SvgIcon>
  );
}
