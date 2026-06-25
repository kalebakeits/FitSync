import {
  DirectionsBike,
  DirectionsRun,
  Pool,
  Rowing,
  Hiking,
  FitnessCenter,
  SportsGymnastics,
} from "@mui/icons-material";
import type { SvgIconProps } from "@mui/material";

// FIT Sport enum values
const CYCLING = [2, 21]; // Cycling, EBiking
const RUNNING = [1];
const SWIMMING = [5, 85]; // Swimming, PoolApnea
const ROWING = [15];
const WALKING = [11, 17]; // Walking, Hiking
const TRANSITION = [3, 18]; // Transition, Multisport

interface SportIconProps extends SvgIconProps {
  sport?: number;
}

export default function SportIcon({ sport, ...props }: SportIconProps) {
  if (sport === undefined) return <FitnessCenter {...props} />;
  if (CYCLING.includes(sport)) return <DirectionsBike {...props} />;
  if (RUNNING.includes(sport)) return <DirectionsRun {...props} />;
  if (SWIMMING.includes(sport)) return <Pool {...props} />;
  if (ROWING.includes(sport)) return <Rowing {...props} />;
  if (WALKING.includes(sport)) return <Hiking {...props} />;
  if (TRANSITION.includes(sport)) return <SportsGymnastics {...props} />;
  return <FitnessCenter {...props} />;
}
