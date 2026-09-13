import {
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Typography,
  Box,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from "@mui/material";
import { Close } from "@mui/icons-material";

interface CookiePolicyModalProps {
  open: boolean;
  onClose: () => void;
}

export default function CookiePolicyModal({
  open,
  onClose,
}: CookiePolicyModalProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Typography variant="h5">Cookie Policy</Typography>
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
          {/* Last Updated */}
          <Typography variant="caption" color="text.secondary">
            Last updated:{" "}
            {new Date().toLocaleDateString("en-GB", {
              day: "2-digit",
              month: "long",
              year: "numeric",
            })}
          </Typography>

          {/* Introduction */}
          <Box>
            <Typography variant="body2" paragraph>
              This Cookie Policy explains how FitSync ("we", "our", or "us")
              uses cookies and similar technologies when you use our service.
            </Typography>
            <Typography variant="body2">
              By using FitSync, you consent to the use of cookies in accordance
              with this policy.
            </Typography>
          </Box>

          <Divider />

          {/* What Are Cookies */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              1. What Are Cookies?
            </Typography>
            <Typography variant="body2" paragraph>
              Cookies are small text files stored on your device (computer,
              tablet, or mobile) when you visit a website. They are widely used
              to make websites work more efficiently and provide information to
              website owners.
            </Typography>
            <Typography variant="body2">
              Cookies can be "session cookies" (deleted when you close your
              browser) or "persistent cookies" (remain on your device until
              deleted or expired).
            </Typography>
          </Box>

          <Divider />

          {/* Cookies We Use */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              2. Cookies We Use
            </Typography>

            <Typography
              variant="subtitle2"
              sx={{ fontWeight: 600, mt: 2, mb: 1 }}
            >
              2.1 Essential Cookies (Strictly Necessary)
            </Typography>
            <Typography variant="body2" paragraph>
              These cookies are essential for the website to function and cannot
              be disabled. Without these cookies, services you have requested
              cannot be provided.
            </Typography>

            <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Cookie Name</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Purpose</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Duration</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  <TableRow>
                    <TableCell sx={{ fontFamily: "monospace" }}>
                      session_token
                    </TableCell>
                    <TableCell>
                      Authenticates your session and keeps you logged in
                    </TableCell>
                    <TableCell>Session (deleted on logout)</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </TableContainer>

            <Typography
              variant="subtitle2"
              sx={{ fontWeight: 600, mt: 2, mb: 1 }}
            >
              2.2 Cloudflare Cookies
            </Typography>
            <Typography variant="body2" paragraph>
              Our service is delivered through Cloudflare's network. Cloudflare
              may set cookies for security purposes.
            </Typography>

            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Cookie Name</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Purpose</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Duration</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  <TableRow>
                    <TableCell sx={{ fontFamily: "monospace" }}>
                      __cf_bm
                    </TableCell>
                    <TableCell>Security purposes</TableCell>
                    <TableCell>30 minutes</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell sx={{ fontFamily: "monospace" }}>
                      cf_clearance
                    </TableCell>
                    <TableCell>Security purposes</TableCell>
                    <TableCell>1 year</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </TableContainer>

            <Typography variant="body2" sx={{ mt: 2, fontSize: "0.875rem" }}>
              For more information about Cloudflare's cookie usage, visit:{" "}
              <Typography
                component="a"
                href="https://www.cloudflare.com/cookie-policy/"
                target="_blank"
                rel="noopener noreferrer"
                sx={{ color: "primary.main" }}
              >
                Cloudflare Cookie Policy
              </Typography>
            </Typography>
          </Box>

          <Divider />

          {/* Third-Party Cookies */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              3. Third-Party Cookies
            </Typography>
            <Typography variant="body2">
              Currently, we do not use any analytics, advertising, or tracking
              cookies. We do not integrate with third-party services like Google
              Analytics, Facebook Pixel, or similar tracking technologies.
            </Typography>
          </Box>

          <Divider />

          {/* Managing Cookies */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              4. Managing Cookies
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              4.1 Browser Settings
            </Typography>
            <Typography variant="body2" paragraph>
              Most web browsers allow you to control cookies through their
              settings. You can:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                Delete all cookies currently stored on your device
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                Block all cookies from being set
              </Typography>
              <Typography component="li" variant="body2">
                Block third-party cookies only
              </Typography>
            </Box>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              4.2 Impact of Disabling Cookies
            </Typography>
            <Typography variant="body2" paragraph>
              Please note that if you disable or block our essential cookies
              (session_token), you will not be able to use FitSync as
              authentication will not work. The service requires session cookies
              to function.
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              4.3 Browser-Specific Instructions
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 0.5 }}>
                <strong>Chrome:</strong> Settings → Privacy and security →
                Cookies and other site data
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 0.5 }}>
                <strong>Firefox:</strong> Settings → Privacy & Security →
                Cookies and Site Data
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 0.5 }}>
                <strong>Safari:</strong> Preferences → Privacy → Manage Website
                Data
              </Typography>
              <Typography component="li" variant="body2">
                <strong>Edge:</strong> Settings → Cookies and site permissions →
                Manage and delete cookies
              </Typography>
            </Box>
          </Box>

          <Divider />

          {/* Do Not Track */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              5. Do Not Track (DNT)
            </Typography>
            <Typography variant="body2">
              Some browsers have a "Do Not Track" (DNT) feature that signals to
              websites that you do not want to have your online activity
              tracked. Since we do not use tracking or analytics cookies, DNT
              signals do not affect your use of FitSync. We respect your privacy
              regardless of DNT settings.
            </Typography>
          </Box>

          <Divider />

          {/* GDPR Compliance */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              6. GDPR and Cookie Consent
            </Typography>
            <Typography variant="body2" paragraph>
              Under GDPR and the ePrivacy Directive, we are required to obtain
              consent for non-essential cookies. However, FitSync only uses:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Essential cookies</strong> (session authentication) -
                these are strictly necessary for the service to function and do
                not require consent under GDPR
              </Typography>
              <Typography component="li" variant="body2">
                <strong>Cloudflare security cookies</strong> - these are
                necessary for security purposes
              </Typography>
            </Box>
            <Typography variant="body2" sx={{ mt: 2 }}>
              By registering for and using FitSync, you acknowledge that these
              essential cookies will be set. If we introduce non-essential
              cookies in the future, we will obtain your explicit consent before
              using them.
            </Typography>
          </Box>

          <Divider />

          {/* Changes to Cookie Policy */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              7. Changes to This Cookie Policy
            </Typography>
            <Typography variant="body2">
              We may update this Cookie Policy from time to time to reflect
              changes in technology or legal requirements. We will notify you of
              any significant changes by updating the "Last updated" date at the
              top of this policy.
            </Typography>
          </Box>

          <Divider />

          {/* Contact */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              8. Contact Us
            </Typography>
            <Typography variant="body2" paragraph>
              If you have any questions about our use of cookies, please contact
              us:
            </Typography>
            <Typography
              variant="body2"
              sx={{
                fontFamily: "monospace",
                bgcolor: "action.hover",
                p: 1,
                borderRadius: 1,
              }}
            >
              Email: kalebahelp@gmail.com
            </Typography>
          </Box>
        </Box>
      </DialogContent>
    </Dialog>
  );
}
