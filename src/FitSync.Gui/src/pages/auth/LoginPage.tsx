import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { TextField, Button, Typography, Alert, Box } from "@mui/material";
import { useAuth } from "../../contexts/AuthContext";
import { usePostApiAuthLogin } from "../../api/generated/auth/auth";
import AuthLayout from "../../components/auth/AuthLayout";
import { loginSchema, type LoginFormData } from "../../schemas/auth";

interface AuthResponse {
  userId: string;
  username: string;
}

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [showVerificationLink, setShowVerificationLink] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: "onTouched",
  });

  useEffect(() => {
    document.title = "FitSync - Login";
  }, []);

  const loginMutation = usePostApiAuthLogin({
    mutation: {
      onSuccess: (data) => {
        const authData = data as unknown as AuthResponse;
        login(authData.userId, authData.username);
        navigate("/dashboard");
      },
      onError: (error: any) => {
        const status = error?.response?.status;
        // Show verification link for 403 (account not verified)
        setShowVerificationLink(status === 403);
      },
    },
  });

  const onSubmit = (data: LoginFormData) => {
    loginMutation.mutate({ data });
  };

  const getErrorMessage = (): string | null => {
    if (!loginMutation.isError) return null;

    const error = loginMutation.error as any;
    const status = error?.response?.status;

    // Map status codes to user-friendly messages
    if (status === 404) {
      return "Invalid username/email or password.";
    }
    if (status === 403) {
      return "Please verify your account. Check your email for the verification link.";
    }
    if (status >= 500) {
      return "Something went wrong on our end. Please try again later.";
    }

    return "Unable to log in. Please try again.";
  };

  const errorMessage = getErrorMessage();

  return (
    <AuthLayout title="FitSync" subtitle="Login">
      {errorMessage && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {errorMessage}
          {showVerificationLink && (
            <Box sx={{ mt: 1 }}>
              <Link
                to="/resend-verification"
                style={{ color: "inherit", textDecoration: "underline" }}
              >
                Resend verification email
              </Link>
            </Box>
          )}
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <TextField
          margin="normal"
          required
          fullWidth
          id="identifier"
          label="Username or Email"
          autoComplete="username"
          autoFocus
          error={!!errors.identifier}
          helperText={errors.identifier?.message}
          {...register("identifier")}
        />

        <TextField
          margin="normal"
          required
          fullWidth
          id="password"
          label="Password"
          type="password"
          autoComplete="current-password"
          error={!!errors.password}
          helperText={errors.password?.message}
          {...register("password")}
        />

        <Box sx={{ textAlign: "right", mb: 1 }}>
          <Typography variant="body2">
            <Link to="/forgot-password" style={{ color: "inherit" }}>
              Forgot password?
            </Link>
          </Typography>
        </Box>

        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 2, mb: 2 }}
          disabled={loginMutation.isPending || !isValid}
        >
          {loginMutation.isPending ? "Logging in..." : "Login"}
        </Button>

        <Box sx={{ textAlign: "center" }}>
          <Typography variant="body2">
            Don't have an account?{" "}
            <Link to="/register" style={{ color: "inherit" }}>
              Register
            </Link>
          </Typography>
        </Box>
      </Box>
    </AuthLayout>
  );
}
