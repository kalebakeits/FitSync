import { useState } from "react";
import { TextField, Button, Alert, Typography, Box } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useGetApiAuthCurrentUser } from "../../api/generated/auth/auth";
import { usePutApiProfileEmail } from "../../api/generated/profile/profile";
import {
  updateEmailSchema,
  type UpdateEmailFormData,
} from "../../schemas/auth";

export default function UpdateEmailForm() {
  const [success, setSuccess] = useState(false);

  const { data: currentUser, refetch } = useGetApiAuthCurrentUser();

  const form = useForm<UpdateEmailFormData>({
    resolver: zodResolver(updateEmailSchema),
    mode: "onTouched",
  });

  const mutation = usePutApiProfileEmail({
    mutation: {
      onSuccess: () => {
        setSuccess(true);
        form.reset();
        refetch();
        setTimeout(() => setSuccess(false), 5000);
      },
    },
  });

  const onSubmit = (data: UpdateEmailFormData) => {
    mutation.mutate({ data });
  };

  const getError = (): string | null => {
    if (!mutation.isError) return null;

    const error = mutation.error as any;
    const status = error?.response?.status;

    // Map status codes to user-friendly messages
    if (status === 409) return "An account with this email already exists.";
    if (status === 404) return "User not found. Please log in again.";
    if (status >= 500)
      return "Something went wrong on our end. Please try again later.";

    return "Failed to update email. Please try again.";
  };

  const isEmailVerified = (currentUser as any)?.isVerified !== false;

  return (
    <Box component="form" onSubmit={form.handleSubmit(onSubmit)} noValidate>
      <Typography variant="subtitle2" gutterBottom>
        Update Email
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
        Current email: {(currentUser as any)?.email || "Loading..."}
      </Typography>

      {!isEmailVerified && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Your email is not verified. Please check your inbox.
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Email updated! Please verify your new email address.
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
        id="email"
        label="New Email Address"
        type="email"
        autoComplete="email"
        error={!!form.formState.errors.email}
        helperText={form.formState.errors.email?.message}
        {...form.register("email")}
      />

      <Button
        type="submit"
        variant="contained"
        sx={{ mt: 2 }}
        disabled={mutation.isPending || !form.formState.isValid}
      >
        {mutation.isPending ? "Updating..." : "Update Email"}
      </Button>
    </Box>
  );
}
