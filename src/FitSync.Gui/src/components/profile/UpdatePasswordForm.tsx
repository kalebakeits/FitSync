import { useState } from "react";
import { TextField, Button, Alert, Typography, Box } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { usePutApiProfilePassword } from "../../api/generated/profile/profile";
import {
  updatePasswordSchema,
  type UpdatePasswordFormData,
} from "../../schemas/auth";

export default function UpdatePasswordForm() {
  const [success, setSuccess] = useState(false);

  const form = useForm<UpdatePasswordFormData>({
    resolver: zodResolver(updatePasswordSchema),
    mode: "onTouched",
  });

  const mutation = usePutApiProfilePassword({
    mutation: {
      onSuccess: () => {
        setSuccess(true);
        form.reset();
        setTimeout(() => setSuccess(false), 5000);
      },
    },
  });

  const onSubmit = (data: UpdatePasswordFormData) => {
    mutation.mutate({ data });
  };

  const getError = (): string | null => {
    if (!mutation.isError) return null;

    const error = mutation.error as any;
    const status = error?.response?.status;

    // Map status codes to user-friendly messages
    if (status === 400) return "Current password is incorrect.";
    if (status === 404) return "User not found. Please log in again.";
    if (status >= 500)
      return "Something went wrong on our end. Please try again later.";

    return "Failed to update password. Please try again.";
  };

  return (
    <Box component="form" onSubmit={form.handleSubmit(onSubmit)} noValidate>
      <Typography variant="subtitle2" gutterBottom>
        Change Password
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Password updated successfully!
        </Alert>
      )}
      {getError() && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {getError()}
        </Alert>
      )}

      <TextField
        margin="normal"
        required
        fullWidth
        id="currentPassword"
        label="Current Password"
        type="password"
        autoComplete="current-password"
        error={!!form.formState.errors.currentPassword}
        helperText={form.formState.errors.currentPassword?.message}
        {...form.register("currentPassword")}
      />

      <TextField
        margin="normal"
        required
        fullWidth
        id="newPassword"
        label="New Password"
        type="password"
        autoComplete="new-password"
        error={!!form.formState.errors.newPassword}
        helperText={
          form.formState.errors.newPassword?.message || "Minimum 8 characters"
        }
        {...form.register("newPassword")}
      />

      <TextField
        margin="normal"
        required
        fullWidth
        id="confirmPassword"
        label="Confirm New Password"
        type="password"
        error={!!form.formState.errors.confirmPassword}
        helperText={form.formState.errors.confirmPassword?.message}
        {...form.register("confirmPassword")}
      />

      <Button
        type="submit"
        variant="contained"
        sx={{ mt: 2 }}
        disabled={mutation.isPending || !form.formState.isValid}
      >
        {mutation.isPending ? "Updating..." : "Update Password"}
      </Button>
    </Box>
  );
}
