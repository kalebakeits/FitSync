import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { TextField, Button, Typography, Alert, Box } from "@mui/material";
import { usePostApiAuthRequestPasswordReset } from "../../api/generated/auth/auth";
import AuthLayout from "../../components/auth/AuthLayout";
import {
  requestPasswordResetSchema,
  type RequestPasswordResetFormData,
} from "../../schemas/auth";

export default function RequestPasswordResetPage() {
  const [emailSent, setEmailSent] = useState(false);
  const [sentEmail, setSentEmail] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<RequestPasswordResetFormData>({
    resolver: zodResolver(requestPasswordResetSchema),
    mode: "onTouched",
  });

  useEffect(() => {
    document.title = "FitSync - Forgot Password";
  }, []);

  const resetRequestMutation = usePostApiAuthRequestPasswordReset({
    mutation: {
      onSuccess: () => {
        setEmailSent(true);
      },
    },
  });

  const onSubmit = (data: RequestPasswordResetFormData) => {
    setSentEmail(data.email);
    resetRequestMutation.mutate({ data });
  };

  const getErrorMessage = (): string | null => {
    if (!resetRequestMutation.isError) return null;

    const error = resetRequestMutation.error as any;
    const status = error?.response?.status;
    const apiMessage = error?.response?.data?.message;

    // If API provides a specific message, use it
    if (apiMessage) return apiMessage;

    // Handle specific status codes
    if (status === 400) {
      return "Invalid email address. Please check and try again.";
    }
    if (status === 404) {
      return "No account found with this email address.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Unable to send password reset email. Please try again.";
  };

  const errorMessage = getErrorMessage();

  return (
    <AuthLayout title="FitSync" subtitle="Forgot Password">
      {emailSent ? (
        <>
          <Alert severity="success" sx={{ mb: 2 }}>
            Password reset instructions have been sent to {sentEmail}. Please
            check your inbox.
          </Alert>
          <Box sx={{ textAlign: "center", mt: 2 }}>
            <Link to="/login" style={{ textDecoration: "none" }}>
              <Button variant="contained" fullWidth>
                Go to Login
              </Button>
            </Link>
          </Box>
        </>
      ) : (
        <>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Enter your email address and we'll send you instructions to reset
            your password.
          </Typography>

          {errorMessage && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {errorMessage}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              type="email"
              autoComplete="email"
              autoFocus
              error={!!errors.email}
              helperText={errors.email?.message}
              {...register("email")}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={resetRequestMutation.isPending || !isValid}
            >
              {resetRequestMutation.isPending
                ? "Sending..."
                : "Send Reset Instructions"}
            </Button>

            <Box sx={{ textAlign: "center" }}>
              <Typography variant="body2">
                Remember your password?{" "}
                <Link to="/login" style={{ color: "inherit" }}>
                  Back to Login
                </Link>
              </Typography>
            </Box>
          </Box>
        </>
      )}
    </AuthLayout>
  );
}
