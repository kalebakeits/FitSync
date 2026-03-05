import { FormControlLabel, Switch, Tooltip, Typography } from "@mui/material";

interface DestinationToggleProps {
  serviceType: string;
  enabled: boolean;
  connected: boolean;
  disabled: boolean;
  onChange: (enabled: boolean) => void;
}

export default function DestinationToggle({
  serviceType,
  enabled,
  connected,
  disabled,
  onChange,
}: DestinationToggleProps) {
  return (
    <Tooltip title={!connected ? `Connect ${serviceType} first` : ""}>
      <FormControlLabel
        control={
          <Switch
            checked={enabled}
            disabled={!connected || disabled}
            onChange={(e) => onChange(e.target.checked)}
          />
        }
        label={
          <Typography variant="body2" color={connected ? "text.primary" : "text.disabled"}>
            {serviceType}{!connected && " (not connected)"}
          </Typography>
        }
      />
    </Tooltip>
  );
}
