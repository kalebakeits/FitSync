import { useNavigate, useLocation, useSearchParams } from "react-router-dom";
import {
  Box,
  Drawer,
  Toolbar,
  Typography,
  Divider,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import {
  CalendarMonth,
  DirectionsRun,
  Settings,
  Sync,
  ArrowBack,
} from "@mui/icons-material";
import { SETTINGS_TABS } from "../../pages/settings/SettingsPage";

export const DRAWER_WIDTH = 220;

const navItems = [
  { label: "Calendar", path: "/", icon: <CalendarMonth /> },
  { label: "Workouts", path: "/workouts", icon: <DirectionsRun /> },
  { label: "Sync", path: "/sync", icon: <Sync /> },
  { label: "Settings", path: "/settings", icon: <Settings /> },
];

interface NavSidebarProps {
  isMobile: boolean;
  mobileOpen: boolean;
  onClose: () => void;
}

export default function NavSidebar({
  isMobile,
  mobileOpen,
  onClose,
}: NavSidebarProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams, setSearchParams] = useSearchParams();

  const isSettings = location.pathname === "/settings";
  const settingsTab = searchParams.get("tab") ?? "account";
  const showSettingsTabs = isMobile && isSettings;

  return (
    <Box
      component="nav"
      sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}
    >
      <Drawer
        variant={isMobile ? "temporary" : "permanent"}
        open={isMobile ? mobileOpen : true}
        onClose={onClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          "& .MuiDrawer-paper": {
            width: DRAWER_WIDTH,
            boxSizing: "border-box",
          },
        }}
      >
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
                    onClose();
                  }}
                >
                  <ListItemIcon sx={{ minWidth: 36 }}>{item.icon}</ListItemIcon>
                  <ListItemText primary={item.label} />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
          {showSettingsTabs && (
            <>
              <Divider />
              <List dense disablePadding sx={{ px: 1 }}>
                {SETTINGS_TABS.map((t) => (
                  <ListItem key={t.key} disablePadding>
                    <ListItemButton
                      selected={settingsTab === t.key}
                      onClick={() => {
                        setSearchParams({ tab: t.key }, { replace: true });
                        onClose();
                      }}
                      sx={{ borderRadius: 1, mb: 0.5 }}
                    >
                      <ListItemText primary={t.label} />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
              <Divider />
              <Box sx={{ p: 1 }}>
                <ListItemButton
                  onClick={() => {
                    navigate("/");
                    onClose();
                  }}
                >
                  <ListItemIcon sx={{ minWidth: 36 }}>
                    <ArrowBack fontSize="small" />
                  </ListItemIcon>
                  <ListItemText primary="Back" />
                </ListItemButton>
              </Box>
            </>
          )}
        </Box>
      </Drawer>
    </Box>
  );
}
