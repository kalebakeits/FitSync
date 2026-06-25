import { useEffect, useRef } from "react";
import { useSearchParams } from "react-router-dom";
import { Button, Typography, Box, Alert } from "@mui/material";
import AuthLayout from "../../components/auth/AuthLayout";

export default function OAuthConsentPage() {
  const [searchParams] = useSearchParams();
  const allowFormRef = useRef<HTMLFormElement>(null);
  const denyFormRef = useRef<HTMLFormElement>(null);

  const clientId = searchParams.get("client_id") ?? "";
  const clientName = searchParams.get("client_name") ?? clientId;
  const redirectUri = searchParams.get("redirect_uri") ?? "";
  const state = searchParams.get("state") ?? "";

  useEffect(() => {
    document.title = "FitSync - Authorize";
  }, []);

  if (!clientId || !redirectUri) {
    return (
      <AuthLayout title="FitSync" subtitle="Authorize Access">
        <Alert severity="error">Invalid OAuth request — missing required parameters.</Alert>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="FitSync" subtitle="Authorize Access">
      <Typography variant="body1" sx={{ mb: 2, textAlign: "center" }}>
        <strong>{clientName}</strong> is requesting access to your FitSync account.
      </Typography>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 4, textAlign: "center" }}>
        This will allow {clientName} to read your activities and connected services via MCP.
      </Typography>

      <form ref={allowFormRef} method="POST" action="/api/oauth/approve" style={{ display: "none" }}>
        <input type="hidden" name="client_id" value={clientId} />
        <input type="hidden" name="redirect_uri" value={redirectUri} />
        {state && <input type="hidden" name="state" value={state} />}
      </form>

      <form ref={denyFormRef} method="POST" action="/api/oauth/deny" style={{ display: "none" }}>
        <input type="hidden" name="redirect_uri" value={redirectUri} />
        {state && <input type="hidden" name="state" value={state} />}
      </form>

      <Box sx={{ display: "flex", gap: 2 }}>
        <Button
          fullWidth
          variant="outlined"
          color="inherit"
          onClick={() => denyFormRef.current?.submit()}
        >
          Deny
        </Button>
        <Button
          fullWidth
          variant="contained"
          onClick={() => allowFormRef.current?.submit()}
        >
          Allow
        </Button>
      </Box>
    </AuthLayout>
  );
}
