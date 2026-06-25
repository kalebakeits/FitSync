import { useNavigate, useLocation, useSearchParams } from "react-router-dom";
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  IconButton,
  Divider,
  Button,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import {
  GridView,
  DirectionsRun,
  CalendarMonth,
  Brightness4,
  Brightness7,
  Logout,
  Settings,
  Menu,
  ArrowBack,
} from "@mui/icons-material";
import { useAuth } from "../../contexts/AuthContext";
import { useAppTheme } from "../../contexts/ThemeContext";
import { useState } from "react";
import { SETTINGS_TABS } from "../../pages/settings/SettingsPage";

const DRAWER_WIDTH = 220;

const navItems = [
  { label: "Dashboard", path: "/dashboard", icon: <GridView /> },
  { label: "Workouts", path: "/workouts", icon: <DirectionsRun /> },
  { label: "Schedule", path: "/schedule", icon: <CalendarMonth /> },
];

interface AppLayoutProps {
  children: React.ReactNode;
}

export default function AppLayout({ children }: AppLayoutProps) {
  const { logout } = useAuth();
  const { mode, toggleTheme } = useAppTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams, setSearchParams] = useSearchParams();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));
  const [mobileOpen, setMobileOpen] = useState(false);

  const isSettings = location.pathname === "/settings";
  const settingsTab = searchParams.get("tab") ?? "account";

  const showSettingsInDrawer = isMobile && isSettings;

  const drawer = (
    <Box sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
      <Toolbar>
        <Typography variant="h6" fontWeight="bold" letterSpacing={0.5}>
          FitSync
        </Typography>
      </Toolbar>
      <Divider />
      <List sx={{ flexGrow: 1 }}>
        {navItems.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              selected={location.pathname === item.path}
              onClick={() => {
                navigate(item.path);
                setMobileOpen(false);
              }}
            >
              <ListItemIcon sx={{ minWidth: 36 }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
      {showSettingsInDrawer && (
        <>
          <Divider />
          <List dense disablePadding sx={{ px: 1 }}>
            {SETTINGS_TABS.map((t) => (
              <ListItem key={t.key} disablePadding>
                <ListItemButton
                  selected={settingsTab === t.key}
                  onClick={() => {
                    setSearchParams({ tab: t.key }, { replace: true });
                    setMobileOpen(false);
                  }}
                  sx={{ borderRadius: 1, mb: 0.5 }}
                >
                  <ListItemText primary={t.label} />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </>
      )}
      <Divider />
      <Box sx={{ p: 1 }}>
        {showSettingsInDrawer ? (
          <ListItemButton
            onClick={() => {
              navigate("/dashboard");
              setMobileOpen(false);
            }}
          >
            <ListItemIcon sx={{ minWidth: 36 }}>
              <ArrowBack fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Back" />
          </ListItemButton>
        ) : (
          <ListItemButton
            selected={isSettings}
            onClick={() => {
              navigate("/settings");
              setMobileOpen(false);
            }}
          >
            <ListItemIcon sx={{ minWidth: 36 }}>
              <Settings fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Settings" />
          </ListItemButton>
        )}
      </Box>
    </Box>
  );

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      {isMobile && (
        <AppBar position="fixed" sx={{ zIndex: theme.zIndex.drawer + 1 }}>
          <Toolbar>
            <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(true)} sx={{ mr: 1 }}>
              <Menu />
            </IconButton>
            <Typography variant="h6" fontWeight="bold" sx={{ flexGrow: 1 }}>
              FitSync
            </Typography>
            <IconButton color="inherit" onClick={toggleTheme}>
              {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
            </IconButton>
            <Button color="inherit" onClick={() => { logout(); navigate("/login"); }}>
              <Logout />
            </Button>
          </Toolbar>
        </AppBar>
      )}

      <Box component="nav" sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}>
        <Drawer
          variant={isMobile ? "temporary" : "permanent"}
          open={isMobile ? mobileOpen : true}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{
            "& .MuiDrawer-paper": { width: DRAWER_WIDTH, boxSizing: "border-box" },
          }}
        >
          {drawer}
        </Drawer>
      </Box>

      <Box sx={{ flexGrow: 1, display: "flex", flexDirection: "column" }}>
        {!isMobile && (
          <AppBar position="static" elevation={1} sx={{ zIndex: 1 }}>
            <Toolbar sx={{ justifyContent: "flex-end" }}>
              <IconButton color="inherit" onClick={toggleTheme}>
                {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
              </IconButton>
              <Button color="inherit" startIcon={<Logout />} onClick={() => { logout(); navigate("/login"); }} />
            </Toolbar>
          </AppBar>
        )}
        <Box sx={{ flexGrow: 1, display: "flex", mt: isMobile ? 8 : 0, minHeight: 0 }}>
          {children}
        </Box>
      </Box>
    </Box>
  );
}
