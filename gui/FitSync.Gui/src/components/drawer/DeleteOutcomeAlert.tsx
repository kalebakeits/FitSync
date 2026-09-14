import { Alert, Box, Typography } from "@mui/material";
import type { DeleteOutcomeNotice } from "../../utils/deleteOutcome";

interface DeleteOutcomeAlertProps {
  notice: DeleteOutcomeNotice;
}

export default function DeleteOutcomeAlert({
  notice,
}: DeleteOutcomeAlertProps) {
  return (
    <Alert severity={notice.severity} sx={{ mb: 1.5 }}>
      <Typography variant="body2">{notice.headline}</Typography>
      <Box component="ul" sx={{ m: 0, pl: 3 }}>
        {notice.failedServiceTypes.map((serviceType) => (
          <li key={serviceType}>
            <Typography variant="body2">{serviceType}</Typography>
          </li>
        ))}
      </Box>
    </Alert>
  );
}
