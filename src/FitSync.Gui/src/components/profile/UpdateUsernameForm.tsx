import { useState } from "react";
import { TextField, Button, Alert, Typography, Box } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { usePutApiProfileUsername } from "../../api/generated/profile/profile";
import {
  updateUsernameSchema,
  type UpdateUsernameFormData,
} from "../../schemas/auth";
import { useAuth } from "../../contexts/AuthContext";

export default function UpdateUsernameForm() {
  const [success, setSuccess] = useState(false);
  const { user } = useAuth();

  const form = useForm<UpdateUsernameFormData>({
    resolver: zodResolver(updateUsernameSchema),
    mode: "onTouched",
  });

  const mutation = usePutApiProfileUsername({
    mutation: {
      onSuccess: () => {
        setSuccess(true);
        form.reset();
        setTimeout(() => setSuccess(false), 5000);
      },
    },
  });

  const onSubmit = (data: UpdateUsernameFormData) => {
    mutation.mutate({ data });
  };

  const getError = (): string | null => {
    if (!mutation.isError) return null;

    const error = mutation.error as any;
    const status = error?.response?.status;

    // Map status codes to user-friendly messages
    if (status === 409) return "This username is already taken.";
    if (status === 400)
      return "Username can only contain letters, numbers, and underscores.";
    if (status === 404) return "User not found. Please log in again.";
    if (status >= 500)
      return "Something went wrong on our end. Please try again later.";

    return "Failed to update username. Please try again.";
  };

  return (
    <Box component="form" onSubmit={form.handleSubmit(onSubmit)} noValidate>
      <Typography variant="subtitle2" gutterBottom>
        Update Username
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Current username: {user?.username}
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Username updated successfully!
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
        id="username"
        label="New Username"
        autoComplete="username"
        error={!!form.formState.errors.username}
        helperText={
          form.formState.errors.username?.message ||
          "Letters, numbers, and underscores only"
        }
        {...form.register("username")}
      />

      <Button
        type="submit"
        variant="contained"
        sx={{ mt: 2 }}
        disabled={mutation.isPending || !form.formState.isValid}
      >
        {mutation.isPending ? "Updating..." : "Update Username"}
      </Button>
    </Box>
  );
}
