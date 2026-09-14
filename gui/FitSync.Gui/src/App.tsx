import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  useLocation,
} from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "./contexts/ThemeContext";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import LoginPage from "./pages/auth/LoginPage";
import RegisterPage from "./pages/auth/RegisterPage";
import VerifyAccountPage from "./pages/auth/VerifyAccountPage";
import ResendVerificationPage from "./pages/auth/ResendVerificationPage";
import RequestPasswordResetPage from "./pages/auth/RequestPasswordResetPage";
import ConfirmPasswordResetPage from "./pages/auth/ConfirmPasswordResetPage";
import ProtectedLayout, {
  LoadingScreen,
} from "./components/layout/ProtectedLayout";
import CalendarPage from "./pages/calendar/CalendarPage";
import WorkoutsPage from "./pages/workouts/WorkoutsPage";
import SyncPage from "./pages/sync/SyncPage";
import OAuthConsentPage from "./pages/oauth/OAuthConsentPage";
import SettingsPage from "./pages/settings/SettingsPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function PublicRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();
  if (isLoading) return <LoadingScreen />;
  if (!isAuthenticated) return <>{children}</>;
  const nextUrl = new URLSearchParams(location.search).get("next");
  if (
    nextUrl &&
    (nextUrl.startsWith("/api/oauth/") ||
      (nextUrl.startsWith("/oauth/") && !nextUrl.startsWith("/oauth/consent")))
  ) {
    window.location.href = nextUrl;
    return <LoadingScreen />;
  }
  return <Navigate to={nextUrl ?? "/"} />;
}

function AppRoutes() {
  return (
    <Routes>
      <Route element={<ProtectedLayout />}>
        <Route path="/" element={<CalendarPage />} />
        <Route path="/workouts" element={<WorkoutsPage />} />
        <Route path="/sync" element={<SyncPage />} />
        <Route path="/settings" element={<SettingsPage />} />
      </Route>

      <Route path="/dashboard" element={<Navigate to="/" replace />} />
      <Route path="/schedule" element={<Navigate to="/" replace />} />

      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginPage />
          </PublicRoute>
        }
      />
      <Route
        path="/register"
        element={
          <PublicRoute>
            <RegisterPage />
          </PublicRoute>
        }
      />
      <Route path="/verify" element={<VerifyAccountPage />} />
      <Route
        path="/resend-verification"
        element={
          <PublicRoute>
            <ResendVerificationPage />
          </PublicRoute>
        }
      />
      <Route
        path="/forgot-password"
        element={
          <PublicRoute>
            <RequestPasswordResetPage />
          </PublicRoute>
        }
      />
      <Route path="/reset-password" element={<ConfirmPasswordResetPage />} />
      <Route path="/oauth/consent" element={<OAuthConsentPage />} />
    </Routes>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <BrowserRouter>
            <AppRoutes />
          </BrowserRouter>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
