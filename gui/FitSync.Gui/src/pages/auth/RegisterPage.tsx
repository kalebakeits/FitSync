import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { TextField, Button, Typography, Alert, Box } from "@mui/material";
import { usePostApiAuthRegister } from "../../api/generated/auth/auth";
import AuthLayout from "../../components/auth/AuthLayout";
import { registerSchema, type RegisterFormData } from "../../schemas/auth";

export default function RegisterPage() {
  const [registrationSuccess, setRegistrationSuccess] = useState(false);
  const [registeredEmail, setRegisteredEmail] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: "onTouched",
  });

  useEffect(() => {
    document.title = "FitSync - Register";
  }, []);

  const registerMutation = usePostApiAuthRegister({
    mutation: {
      onSuccess: () => {
        setRegistrationSuccess(true);
      },
    },
  });

  const onSubmit = (data: RegisterFormData) => {
    setRegisteredEmail(data.email);
    registerMutation.mutate({
      data: {
        username: data.username,
        email: data.email,
        password: data.password,
      },
    });
  };

  const getErrorMessage = (): string | null => {
    if (!registerMutation.isError) return null;

    const error = registerMutation.error as any;
    const status = error?.response?.status;

    // Map status codes to user-friendly messages
    if (status === 409) {
      return "This username or email is already taken.";
    }
    if (status === 400) {
      return "Username can only contain letters, numbers, and underscores.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Unable to create account. Please try again.";
  };

  const errorMessage = getErrorMessage();

  return (
    <AuthLayout title="FitSync" subtitle="Create Account">
      {registrationSuccess ? (
        <>
          <Alert severity="success" sx={{ mb: 2 }}>
            Account created successfully! Please check your email (
            {registeredEmail}) to verify your account.
          </Alert>
          <Box sx={{ textAlign: "center", mt: 2 }}>
            <Typography variant="body2" sx={{ mb: 2 }}>
              Didn't receive the email?{" "}
              <Link to="/resend-verification" style={{ color: "inherit" }}>
                Resend verification email
              </Link>
            </Typography>
            <Link to="/login" style={{ color: "inherit" }}>
              <Button variant="contained" fullWidth>
                Go to Login
              </Button>
            </Link>
          </Box>
        </>
      ) : (
        <>
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
              id="username"
              label="Username"
              autoComplete="username"
              autoFocus
              error={!!errors.username}
              helperText={
                errors.username?.message ||
                "Letters, numbers, and underscores only"
              }
              {...register("username")}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              type="email"
              autoComplete="email"
              error={!!errors.email}
              helperText={errors.email?.message}
              {...register("email")}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              id="password"
              label="Password"
              type="password"
              autoComplete="new-password"
              error={!!errors.password}
              helperText={errors.password?.message || "Minimum 8 characters"}
              {...register("password")}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              id="confirmPassword"
              label="Confirm Password"
              type="password"
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
              {...register("confirmPassword")}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={registerMutation.isPending || !isValid}
            >
              {registerMutation.isPending ? "Creating account..." : "Register"}
            </Button>

            <Box sx={{ textAlign: "center" }}>
              <Typography variant="body2">
                Already have an account?{" "}
                <Link to="/login" style={{ color: "inherit" }}>
                  Login
                </Link>
              </Typography>
            </Box>
          </Box>
        </>
      )}
    </AuthLayout>
  );
}
