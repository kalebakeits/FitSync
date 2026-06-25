import { TextField, InputAdornment } from "@mui/material";
import InfoTooltip from "../InfoTooltip";
import { secondsToPaceDisplay, paceDisplayToSeconds } from "../../utils/paceFormat";

interface PaceFieldProps {
  label: string;
  tooltip: string;
  valueSeconds: string;
  onChangeSeconds: (v: string) => void;
}

export default function PaceField({
  label,
  tooltip,
  valueSeconds,
  onChangeSeconds,
}: PaceFieldProps) {
  const display = valueSeconds ? secondsToPaceDisplay(parseInt(valueSeconds)) : "";

  return (
    <TextField
      label={label}
      value={display}
      onChange={(e) => {
        const parsed = paceDisplayToSeconds(e.target.value);
        onChangeSeconds(parsed > 0 ? parsed.toString() : "");
      }}
      placeholder="m:ss"
      size="small"
      fullWidth
      slotProps={{
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <InfoTooltip title={tooltip} />
            </InputAdornment>
          ),
        },
      }}
    />
  );
}
