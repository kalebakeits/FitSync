import {
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Typography,
  Box,
  Divider,
} from "@mui/material";
import { Close } from "@mui/icons-material";

interface SyncHelpModalProps {
  open: boolean;
  onClose: () => void;
}

export default function SyncHelpModal({ open, onClose }: SyncHelpModalProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Typography variant="h5">How FitSync Works</Typography>
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
          {/* Setup Requirements */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              Getting Started
            </Typography>
            <Typography variant="body2" paragraph>
              To sync your activities between services:
            </Typography>
            <Box component="ol" sx={{ pl: 2, m: 0 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                Configure Garmin + Zwift credentials and connect Wahoo
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                Ensure both credentials are enabled (update them if they're
                disabled)
              </Typography>
              <Typography component="li" variant="body2">
                Credentials are automatically disabled after multiple
                consecutive sync failures
              </Typography>
            </Box>
          </Box>

          <Divider />

          {/* How It Works */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              The Process
            </Typography>
            <Typography variant="body2" paragraph>
              FitSync checks your Zwift account for new activities and also
              listens for Wahoo workout webhooks. When found, activities are
              automatically uploaded to Garmin Connect and appear as virtual
              rides recorded on a Garmin EDGE 820.
            </Typography>
            <Typography variant="body2" paragraph>
              Why does this matter? Garmin only calculates training readiness,
              body battery, and other advanced metrics for activities recorded
              on Garmin devices. By presenting your Zwift activities as Garmin
              device recordings, they count toward your training load and
              recovery metrics in Garmin Connect.
            </Typography>
          </Box>

          <Divider />

          {/* Privacy */}
          <Box>
            <Typography
              variant="h6"
              gutterBottom
              sx={{ fontWeight: 600, color: "primary.main" }}
            >
              Your Data & Privacy
            </Typography>

            <Typography variant="body2" paragraph sx={{ fontWeight: 500 }}>
              Activity Files:
            </Typography>
            <Typography variant="body2" paragraph>
              Files are stored temporarily during processing (from download to
              upload completion). Once processed, files are deleted. Only
              metadata (like the activity's unique identifier) is retained for 3
              months to prevent duplicate uploads.
            </Typography>

            <Typography variant="body2" paragraph sx={{ fontWeight: 500 }}>
              Credentials:
            </Typography>
            <Typography variant="body2" paragraph>
              Your usernames and passwords are encrypted and stored securely.
              When you delete credentials, they're removed immediately.
            </Typography>

            <Typography variant="body2" paragraph sx={{ fontWeight: 500 }}>
              Data Sharing:
            </Typography>
            <Typography variant="body2" paragraph>
              Your data is only sent to the services you configure as
              destinations. I have no interest in your activities and don't use
              your data for anything beyond syncing between your chosen
              services.
            </Typography>

            <Typography variant="body2" paragraph sx={{ fontWeight: 500 }}>
              Account Deletion:
            </Typography>
            <Typography variant="body2">
              To delete your account, email{" "}
              <Typography
                component="a"
                href="mailto:kalebahelp@gmail.com"
                sx={{ color: "primary.main" }}
              >
                kalebahelp@gmail.com
              </Typography>
            </Typography>
          </Box>

          <Divider />

          {/* About This Service */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              About This Service
            </Typography>
            <Typography variant="body2" paragraph>
              FitSync is a hobby project running on my home server. While I'm
              committed to keeping it reliable, occasional downtime may happen.
              I'll always work to restore service as quickly as possible.
            </Typography>
            <Typography variant="body2" paragraph>
              Interested in self-hosting? I'm happy to help you set up your own
              instance - just reach out!
            </Typography>
            <Typography variant="body2">
              For issues or questions, contact{" "}
              <Typography
                component="a"
                href="mailto:kalebahelp@gmail.com"
                sx={{ color: "primary.main" }}
              >
                kalebahelp@gmail.com
              </Typography>
            </Typography>
          </Box>

          <Divider />

          {/* Upcoming Features */}
          <Box>
            <Typography
              variant="h6"
              gutterBottom
              sx={{ fontWeight: 600, color: "success.main" }}
            >
              Coming Soon
            </Typography>
            <Box component="ul" sx={{ pl: 2, m: 0 }}>
              <Typography component="li" variant="body2" sx={{ mb: 0.5 }}>
                Bryton as a source
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 0.5 }}>
                Source-specific sync rules
              </Typography>
              <Typography component="li" variant="body2">
                Multiple destinations with customizable rules for different
                activity types
              </Typography>
            </Box>
          </Box>
        </Box>
      </DialogContent>
    </Dialog>
  );
}
