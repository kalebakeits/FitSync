import { Button, IconButton, Tooltip, useMediaQuery, useTheme } from "@mui/material";
import type { ButtonProps } from "@mui/material";
import type { ReactElement } from "react";

interface ResponsiveButtonProps extends Omit<ButtonProps, "startIcon"> {
  icon: ReactElement;
  label: string;
}

export default function ResponsiveButton({ icon, label, ...props }: ResponsiveButtonProps) {
  const compact = useMediaQuery(useTheme().breakpoints.down("md"));

  if (compact) {
    return (
      <Tooltip title={label}>
        <span>
          <IconButton onClick={props.onClick} disabled={props.disabled} size="small" color={props.color as any}>
            {icon}
          </IconButton>
        </span>
      </Tooltip>
    );
  }

  return (
    <Button size="small" startIcon={icon} {...props}>
      {label}
    </Button>
  );
}
