import { Stack, Typography } from "@mui/material";

interface SummaryRowProps {
  label: string;
  value: string;
}

export default function SummaryRow({ label, value }: SummaryRowProps) {
  return (
    <Stack
      direction="row"
      justifyContent="space-between"
      spacing={2}
      sx={{ py: 0.5 }}
    >
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" fontWeight="medium">
        {value}
      </Typography>
    </Stack>
  );
}
