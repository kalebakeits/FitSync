import type { ReactNode } from "react";
import { Container, Box, Typography, Paper, IconButton } from "@mui/material";
import { Brightness4, Brightness7 } from "@mui/icons-material";
import { useAppTheme } from "../../contexts/ThemeContext";
import Footer from "../Footer";

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  children: ReactNode;
}

export default function AuthLayout({ subtitle, children }: AuthLayoutProps) {
  const { mode, toggleTheme } = useAppTheme();

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <Container
        maxWidth="sm"
        sx={{
          flex: 1,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <Box
          sx={{
            width: "100%",
            py: 4,
          }}
        >
          <Paper
            elevation={3}
            sx={{ p: 4, width: "100%", position: "relative" }}
          >
            <IconButton
              onClick={toggleTheme}
              sx={{ position: "absolute", top: 16, right: 16 }}
              aria-label="Toggle theme"
            >
              {mode === "dark" ? <Brightness7 /> : <Brightness4 />}
            </IconButton>

            <Typography variant="h4" component="h1" gutterBottom align="center">
              FitSync
            </Typography>
            <Typography
              variant="h6"
              gutterBottom
              align="center"
              color="text.secondary"
            >
              {subtitle}
            </Typography>

            {children}
          </Paper>
        </Box>
      </Container>
      <Footer />
    </Box>
  );
}
