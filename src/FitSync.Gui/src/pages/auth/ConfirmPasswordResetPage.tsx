import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { TextField, Button, Typography, Alert, Box } from "@mui/material";
import { usePostApiAuthConfirmPasswordReset } from "../../api/generated/auth/auth";
import AuthLayout from "../../components/auth/AuthLayout";
import {
  confirmPasswordResetSchema,
  type ConfirmPasswordResetFormData,
} from "../../schemas/auth";

export default function ConfirmPasswordResetPage() {
  const [searchParams] = useSearchParams();
  const [resetSuccess, setResetSuccess] = useState(false);
  const token = searchParams.get("token");

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<ConfirmPasswordResetFormData>({
    resolver: zodResolver(confirmPasswordResetSchema),
    mode: "onTouched",
    defaultValues: {
      token: token || "",
    },
  });

  useEffect(() => {
    document.title = "FitSync - Reset Password";
  }, []);

  const resetMutation = usePostApiAuthConfirmPasswordReset({
    mutation: {
      onSuccess: () => {
        setResetSuccess(true);
      },
    },
  });

  const onSubmit = (data: ConfirmPasswordResetFormData) => {
    resetMutation.mutate({ data });
  };

  const getErrorMessage = (): string | null => {
    if (!resetMutation.isError) return null;

    const error = resetMutation.error as any;
    const status = error?.response?.status;
    const apiMessage = error?.response?.data?.message;

    // If API provides a specific message, use it
    if (apiMessage) return apiMessage;

    // Handle specific status codes
    if (status === 400) {
      return "Invalid or expired reset link. Please request a new password reset.";
    }
    if (status === 404) {
      return "Reset link not found. Please request a new password reset.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Unable to reset password. Please try again.";
  };

  const errorMessage = getErrorMessage();

  if (!token) {
    return (
      <AuthLayout title="FitSync" subtitle="Reset Password">
        <Alert severity="error">
          Invalid password reset link. Please request a new one.
        </Alert>
        <Box sx={{ textAlign: "center", mt: 2 }}>
          <Link to="/forgot-password" style={{ textDecoration: "none" }}>
            <Button variant="contained" fullWidth>
              Request Password Reset
            </Button>
          </Link>
        </Box>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="FitSync" subtitle="Reset Password">
      {resetSuccess ? (
        <>
          <Alert severity="success" sx={{ mb: 2 }}>
            Your password has been reset successfully! You can now log in with
            your new password.
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
            Enter your new password below.
          </Typography>

          {errorMessage && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {errorMessage}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
            <input type="hidden" {...register("token")} />

            <TextField
              margin="normal"
              required
              fullWidth
              id="newPassword"
              label="New Password"
              type="password"
              autoComplete="new-password"
              autoFocus
              error={!!errors.newPassword}
              helperText={errors.newPassword?.message || "Minimum 8 characters"}
              {...register("newPassword")}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={resetMutation.isPending || !isValid}
            >
              {resetMutation.isPending ? "Resetting..." : "Reset Password"}
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
