import { useState } from "react";
import {
  Paper,
  Typography,
  Box,
  Tabs,
  Tab,
  TextField,
  Button,
  Alert,
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useGetApiAuthCurrentUser } from "../../api/generated/auth/auth";
import {
  usePutApiProfileUsername,
  usePutApiProfileEmail,
  usePutApiProfilePassword,
} from "../../api/generated/profile/profile";
import {
  updateUsernameSchema,
  updateEmailSchema,
  updatePasswordSchema,
  type UpdateUsernameFormData,
  type UpdateEmailFormData,
  type UpdatePasswordFormData,
} from "../../schemas/auth";
import { useAuth } from "../../contexts/AuthContext";

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`profile-tabpanel-${index}`}
      aria-labelledby={`profile-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

export default function ProfileSettings() {
  const [tabValue, setTabValue] = useState(0);
  const [usernameSuccess, setUsernameSuccess] = useState(false);
  const [emailSuccess, setEmailSuccess] = useState(false);
  const [passwordSuccess, setPasswordSuccess] = useState(false);
  const { user } = useAuth();

  const { data: currentUser, refetch: refetchUser } =
    useGetApiAuthCurrentUser();

  const usernameForm = useForm<UpdateUsernameFormData>({
    resolver: zodResolver(updateUsernameSchema),
    mode: "onTouched",
  });

  const emailForm = useForm<UpdateEmailFormData>({
    resolver: zodResolver(updateEmailSchema),
    mode: "onTouched",
  });

  const passwordForm = useForm<UpdatePasswordFormData>({
    resolver: zodResolver(updatePasswordSchema),
    mode: "onTouched",
  });

  const usernameMutation = usePutApiProfileUsername({
    mutation: {
      onSuccess: () => {
        setUsernameSuccess(true);
        refetchUser();
        setTimeout(() => setUsernameSuccess(false), 5000);
      },
    },
  });

  const emailMutation = usePutApiProfileEmail({
    mutation: {
      onSuccess: () => {
        setEmailSuccess(true);
        refetchUser();
        setTimeout(() => setEmailSuccess(false), 5000);
      },
    },
  });

  const passwordMutation = usePutApiProfilePassword({
    mutation: {
      onSuccess: () => {
        setPasswordSuccess(true);
        passwordForm.reset();
        setTimeout(() => setPasswordSuccess(false), 5000);
      },
    },
  });

  const onUpdateUsername = (data: UpdateUsernameFormData) => {
    usernameMutation.mutate({ data });
  };

  const onUpdateEmail = (data: UpdateEmailFormData) => {
    emailMutation.mutate({ data });
  };

  const onUpdatePassword = (data: UpdatePasswordFormData) => {
    passwordMutation.mutate({ data });
  };

  const getUsernameError = (): string | null => {
    if (!usernameMutation.isError) return null;

    const error = usernameMutation.error as any;
    const status = error?.response?.status;
    const apiMessage = error?.response?.data?.message;

    if (apiMessage) return apiMessage;

    if (status === 409) {
      return "Username already taken. Please choose a different one.";
    }
    if (status === 400) {
      return "Invalid username. Use only letters, numbers, and underscores.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Failed to update username. Please try again.";
  };

  const getEmailError = (): string | null => {
    if (!emailMutation.isError) return null;

    const error = emailMutation.error as any;
    const status = error?.response?.status;
    const apiMessage = error?.response?.data?.message;

    if (apiMessage) return apiMessage;

    if (status === 409) {
      return "Email already in use. Please use a different email.";
    }
    if (status === 400) {
      return "Invalid email address. Please check and try again.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Failed to update email. Please try again.";
  };

  const getPasswordError = (): string | null => {
    if (!passwordMutation.isError) return null;

    const error = passwordMutation.error as any;
    const status = error?.response?.status;
    const apiMessage = error?.response?.data?.message;

    if (apiMessage) return apiMessage;

    if (status === 401) {
      return "Current password is incorrect.";
    }
    if (status === 400) {
      return "Invalid password format. Must be at least 8 characters.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Failed to update password. Please try again.";
  };

  const isEmailVerified = (currentUser as any)?.isVerified !== false;

  return (
    <Paper sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Profile Settings
      </Typography>

      <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
        <Tabs
          value={tabValue}
          onChange={(_, newValue) => setTabValue(newValue)}
        >
          <Tab label="Account" />
          <Tab label="Password" />
        </Tabs>
      </Box>

      <TabPanel value={tabValue} index={0}>
        <Box
          component="form"
          onSubmit={usernameForm.handleSubmit(onUpdateUsername)}
          noValidate
        >
          <Typography variant="subtitle2" gutterBottom>
            Update Username
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Current username: {user?.username}
          </Typography>

          {usernameSuccess && (
            <Alert severity="success" sx={{ mb: 2 }}>
              Username updated successfully!
            </Alert>
          )}
          {getUsernameError() && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {getUsernameError()}
            </Alert>
          )}

          <TextField
            margin="normal"
            required
            fullWidth
            id="username"
            label="New Username"
            autoComplete="username"
            error={!!usernameForm.formState.errors.username}
            helperText={
              usernameForm.formState.errors.username?.message ||
              "Letters, numbers, and underscores only"
            }
            {...usernameForm.register("username")}
          />

          <Button
            type="submit"
            variant="contained"
            sx={{ mt: 2 }}
            disabled={
              usernameMutation.isPending || !usernameForm.formState.isValid
            }
          >
            {usernameMutation.isPending ? "Updating..." : "Update Username"}
          </Button>
        </Box>

        <Box
          component="form"
          onSubmit={emailForm.handleSubmit(onUpdateEmail)}
          noValidate
          sx={{ mt: 4 }}
        >
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

          {emailSuccess && (
            <Alert severity="success" sx={{ mb: 2 }}>
              Email updated! Please verify your new email address.
            </Alert>
          )}
          {getEmailError() && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {getEmailError()}
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
            error={!!emailForm.formState.errors.email}
            helperText={emailForm.formState.errors.email?.message}
            {...emailForm.register("email")}
          />

          <Button
            type="submit"
            variant="contained"
            sx={{ mt: 2 }}
            disabled={emailMutation.isPending || !emailForm.formState.isValid}
          >
            {emailMutation.isPending ? "Updating..." : "Update Email"}
          </Button>
        </Box>
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <Box
          component="form"
          onSubmit={passwordForm.handleSubmit(onUpdatePassword)}
          noValidate
        >
          <Typography variant="subtitle2" gutterBottom>
            Change Password
          </Typography>

          {passwordSuccess && (
            <Alert severity="success" sx={{ mb: 2 }}>
              Password updated successfully!
            </Alert>
          )}
          {getPasswordError() && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {getPasswordError()}
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
            error={!!passwordForm.formState.errors.currentPassword}
            helperText={passwordForm.formState.errors.currentPassword?.message}
            {...passwordForm.register("currentPassword")}
          />

          <TextField
            margin="normal"
            required
            fullWidth
            id="newPassword"
            label="New Password"
            type="password"
            autoComplete="new-password"
            error={!!passwordForm.formState.errors.newPassword}
            helperText={
              passwordForm.formState.errors.newPassword?.message ||
              "Minimum 8 characters"
            }
            {...passwordForm.register("newPassword")}
          />

          <TextField
            margin="normal"
            required
            fullWidth
            id="confirmPassword"
            label="Confirm New Password"
            type="password"
            error={!!passwordForm.formState.errors.confirmPassword}
            helperText={passwordForm.formState.errors.confirmPassword?.message}
            {...passwordForm.register("confirmPassword")}
          />

          <Button
            type="submit"
            variant="contained"
            sx={{ mt: 2 }}
            disabled={
              passwordMutation.isPending || !passwordForm.formState.isValid
            }
          >
            {passwordMutation.isPending ? "Updating..." : "Update Password"}
          </Button>
        </Box>
      </TabPanel>
    </Paper>
  );
}
