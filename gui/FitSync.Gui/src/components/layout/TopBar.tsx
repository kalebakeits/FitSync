import { useNavigate } from "react-router-dom";
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Button,
  useTheme,
} from "@mui/material";
import { Brightness4, Brightness7, Logout, Menu } from "@mui/icons-material";
import { useAuth } from "../../contexts/AuthContext";
import { useAppTheme } from "../../contexts/ThemeContext";

interface TopBarProps {
  isMobile: boolean;
  onMenuOpen: () => void;
}

export default function TopBar({ isMobile, onMenuOpen }: TopBarProps) {
  const { logout } = useAuth();
  const { mode, toggleTheme } = useAppTheme();
  const navigate = useNavigate();
  const theme = useTheme();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  if (isMobile) {
    return (
      <AppBar position="fixed" sx={{ zIndex: theme.zIndex.drawer + 1 }}>
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            onClick={onMenuOpen}
            sx={{ mr: 1 }}
          >
            <Menu />
          </IconButton>
          <Typography variant="h6" fontWeight="bold" sx={{ flexGrow: 1 }}>
            FitSync
          </Typography>
          <IconButton color="inherit" onClick={toggleTheme}>
            {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
          <Button color="inherit" onClick={handleLogout}>
            <Logout />
          </Button>
        </Toolbar>
      </AppBar>
    );
  }

  return (
    <AppBar position="static" elevation={1} sx={{ zIndex: 1 }}>
      <Toolbar sx={{ justifyContent: "flex-end" }}>
        <IconButton color="inherit" onClick={toggleTheme}>
          {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
        </IconButton>
        <Button color="inherit" startIcon={<Logout />} onClick={handleLogout} />
      </Toolbar>
    </AppBar>
  );
}
