import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Typography,
} from "@mui/material";
import { usePostApiAuthVerify } from "../../api/generated/auth/auth";
import AuthLayout from "../../components/auth/AuthLayout";

export default function VerifyAccountPage() {
  const [searchParams] = useSearchParams();
  const [verificationAttempted, setVerificationAttempted] = useState(false);
  const token = searchParams.get("token");

  useEffect(() => {
    document.title = "FitSync - Verify Account";
  }, []);

  const verifyMutation = usePostApiAuthVerify({
    mutation: {
      onSuccess: () => {
        setVerificationAttempted(true);
      },
      onError: () => {
        setVerificationAttempted(true);
      },
    },
  });

  useEffect(() => {
    if (token && !verificationAttempted) {
      verifyMutation.mutate({ data: { token } });
    }
  }, [token]);

  if (!token) {
    return (
      <AuthLayout title="FitSync" subtitle="Verify Account">
        <Alert severity="error">
          Invalid verification link. Please check your email and try again.
        </Alert>
        <Box sx={{ textAlign: "center", mt: 2 }}>
          <Link to="/login" style={{ textDecoration: "none" }}>
            <Button variant="contained" fullWidth>
              Go to Login
            </Button>
          </Link>
        </Box>
      </AuthLayout>
    );
  }

  if (verifyMutation.isPending) {
    return (
      <AuthLayout title="FitSync" subtitle="Verify Account">
        <Box sx={{ textAlign: "center", py: 4 }}>
          <CircularProgress />
          <Typography variant="body1" sx={{ mt: 2 }}>
            Verifying your account...
          </Typography>
        </Box>
      </AuthLayout>
    );
  }

  if (verifyMutation.isSuccess) {
    return (
      <AuthLayout title="FitSync" subtitle="Verify Account">
        <Alert severity="success" sx={{ mb: 2 }}>
          Your account has been verified successfully! You can now log in.
        </Alert>
        <Box sx={{ textAlign: "center", mt: 2 }}>
          <Link to="/login" style={{ textDecoration: "none" }}>
            <Button variant="contained" fullWidth>
              Go to Login
            </Button>
          </Link>
        </Box>
      </AuthLayout>
    );
  }

  if (verifyMutation.isError) {
    const errorMessage =
      (verifyMutation.error as any)?.response?.data?.message ||
      "Verification failed. The link may be expired or invalid.";

    return (
      <AuthLayout title="FitSync" subtitle="Verify Account">
        <Alert severity="error" sx={{ mb: 2 }}>
          {errorMessage}
        </Alert>
        <Box sx={{ textAlign: "center", mt: 2 }}>
          <Typography variant="body2" sx={{ mb: 2 }}>
            <Link to="/resend-verification" style={{ color: "inherit" }}>
              Resend verification email
            </Link>
          </Typography>
          <Link to="/login" style={{ textDecoration: "none" }}>
            <Button variant="outlined" fullWidth>
              Go to Login
            </Button>
          </Link>
        </Box>
      </AuthLayout>
    );
  }

  return null;
}
