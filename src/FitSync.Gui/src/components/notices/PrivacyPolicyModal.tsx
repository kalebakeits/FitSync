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

interface PrivacyPolicyModalProps {
  open: boolean;
  onClose: () => void;
}

export default function PrivacyPolicyModal({
  open,
  onClose,
}: PrivacyPolicyModalProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Typography variant="h5">Privacy Policy</Typography>
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
              FitSync ("we", "our", or "us") is committed to protecting your
              privacy. This Privacy Policy explains how we collect, use,
              disclose, and safeguard your information when you use our service.
            </Typography>
            <Typography variant="body2">
              By using FitSync, you agree to the collection and use of
              information in accordance with this policy.
            </Typography>
          </Box>

          <Divider />

          {/* Data Controller */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              1. Data Controller
            </Typography>
            <Typography variant="body2" paragraph>
              The data controller responsible for your personal data is:
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

          <Divider />

          {/* Information We Collect */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              2. Information We Collect
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              2.1 Personal Information
            </Typography>
            <Typography variant="body2" component="div">
              <Box component="ul" sx={{ pl: 2, mt: 1 }}>
                <li>
                  Username and email address (provided during registration)
                </li>
                <li>
                  Third-party service credentials (Garmin, Zwift usernames and
                  passwords)
                </li>
                <li>Session data for authentication</li>
              </Box>
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              2.2 Activity Data
            </Typography>
            <Typography variant="body2" component="div">
              <Box component="ul" sx={{ pl: 2, mt: 1 }}>
                <li>
                  Fitness activity files (.fit files) from your Zwift account
                </li>
                <li>
                  Activity metadata (file names, unique identifiers, dates,
                  status)
                </li>
                <li>
                  Processing information (upload status, retry counts, errors)
                </li>
              </Box>
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              2.3 Technical Data
            </Typography>
            <Typography variant="body2" component="div">
              <Box component="ul" sx={{ pl: 2, mt: 1 }}>
                <li>IP address (via Cloudflare proxy)</li>
                <li>Browser type and version</li>
                <li>Session cookies for authentication</li>
              </Box>
            </Typography>
          </Box>

          <Divider />

          {/* How We Use Your Information */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              3. How We Use Your Information
            </Typography>
            <Typography variant="body2" paragraph>
              We use your personal information solely for the following
              purposes:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                To provide and maintain our service (syncing activities between
                Zwift and Garmin)
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                To authenticate you and manage your account
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                To access your Zwift activities using your credentials
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                To upload modified activities to your Garmin account
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                To prevent duplicate uploads (using activity metadata)
              </Typography>
              <Typography component="li" variant="body2">
                To troubleshoot technical issues
              </Typography>
            </Box>
          </Box>

          <Divider />

          {/* Data Sharing */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              4. Data Sharing and Third Parties
            </Typography>
            <Typography variant="body2" paragraph sx={{ fontWeight: 500 }}>
              Your data is only shared with the services you explicitly
              configure as destinations.
            </Typography>
            <Typography variant="body2" paragraph>
              Specifically:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Zwift:</strong> We use your Zwift credentials to fetch
                your activity files
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Garmin Connect:</strong> We use your Garmin credentials
                to upload modified activities
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Cloudflare:</strong> Our service is proxied through
                Cloudflare, which may collect technical data (see Cookie Policy)
              </Typography>
            </Box>
            <Typography variant="body2" sx={{ fontWeight: 500, mt: 2 }}>
              We do NOT sell, rent, or share your personal data with any other
              third parties. We have no interest in your fitness activities
              beyond facilitating the sync between your chosen services.
            </Typography>
          </Box>

          <Divider />

          {/* Data Retention */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              5. Data Retention
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              5.1 Activity Files
            </Typography>
            <Typography variant="body2" paragraph>
              Activity files (.fit files) are stored temporarily during
              processing only. Once an activity is successfully uploaded or
              marked as failed, the file is immediately deleted from our
              servers.
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              5.2 Activity Metadata
            </Typography>
            <Typography variant="body2" paragraph>
              To prevent duplicate uploads, we retain minimal metadata (activity
              ID, file name, or unique identifier) for a maximum of 3 months.
              After 3 months, this metadata is automatically deleted.
            </Typography>

            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 2 }}>
              5.3 Credentials and Account Data
            </Typography>
            <Typography variant="body2">
              Your credentials and account information are retained until you
              delete your account. Deletion is immediate and permanent.
            </Typography>
          </Box>

          <Divider />

          {/* Data Security */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              6. Data Security
            </Typography>
            <Typography variant="body2" paragraph>
              We implement appropriate technical and organizational security
              measures to protect your data:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                All passwords and credentials are encrypted using
                industry-standard encryption before storage
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                Secure database storage with access controls
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                HTTPS encryption for all data transmission
              </Typography>
              <Typography component="li" variant="body2">
                Session-based authentication with secure cookies
              </Typography>
            </Box>
            <Typography variant="body2" sx={{ mt: 2 }}>
              However, no method of transmission over the internet is 100%
              secure. While we strive to protect your personal data, we cannot
              guarantee absolute security.
            </Typography>
          </Box>

          <Divider />

          {/* Your Rights (GDPR) */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              7. Your Rights (GDPR)
            </Typography>
            <Typography variant="body2" paragraph>
              Under the General Data Protection Regulation (GDPR), you have the
              following rights:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Right to Access:</strong> Request a copy of your
                personal data
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Right to Rectification:</strong> Request correction of
                inaccurate data
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Right to Erasure:</strong> Request deletion of your data
                (use "Delete Account" feature)
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Right to Restrict Processing:</strong> Request
                limitation of how we process your data
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Right to Data Portability:</strong> Request your data in
                a machine-readable format
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                <strong>Right to Object:</strong> Object to processing of your
                data
              </Typography>
              <Typography component="li" variant="body2">
                <strong>Right to Withdraw Consent:</strong> Withdraw consent at
                any time (by deleting your account)
              </Typography>
            </Box>
            <Typography variant="body2" sx={{ mt: 2 }}>
              To exercise any of these rights, contact us at{" "}
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

          {/* Account Deletion */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              8. Account Deletion
            </Typography>
            <Typography variant="body2" paragraph>
              You can delete your account at any time using the "Delete Account"
              button in Account Settings. When you delete your account:
            </Typography>
            <Box component="ul" sx={{ pl: 2 }}>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                All credentials are immediately and permanently deleted
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                Your profile and email are permanently deleted
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                All activity records and metadata are permanently deleted
              </Typography>
              <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                All sessions are terminated
              </Typography>
              <Typography component="li" variant="body2">
                This action cannot be undone
              </Typography>
            </Box>
          </Box>

          <Divider />

          {/* International Transfers */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              9. International Data Transfers
            </Typography>
            <Typography variant="body2">
              FitSync is hosted on servers that may be located outside your
              country of residence. By using our service, you consent to the
              transfer of your data to these locations. We ensure appropriate
              safeguards are in place to protect your data in accordance with
              this Privacy Policy and applicable data protection laws.
            </Typography>
          </Box>

          <Divider />

          {/* Children's Privacy */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              10. Children's Privacy
            </Typography>
            <Typography variant="body2">
              FitSync is not intended for use by individuals under the age of
              16. We do not knowingly collect personal data from children under
              16. If you become aware that a child has provided us with personal
              data, please contact us immediately.
            </Typography>
          </Box>

          <Divider />

          {/* Changes to Privacy Policy */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              11. Changes to This Privacy Policy
            </Typography>
            <Typography variant="body2">
              We may update this Privacy Policy from time to time. We will
              notify you of any changes by updating the "Last updated" date at
              the top of this policy. You are advised to review this Privacy
              Policy periodically for any changes.
            </Typography>
          </Box>

          <Divider />

          {/* Contact */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              12. Contact Us
            </Typography>
            <Typography variant="body2" paragraph>
              If you have any questions about this Privacy Policy or wish to
              exercise your rights, please contact us:
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
            <Typography variant="body2" sx={{ mt: 2 }}>
              We will respond to your request within 30 days as required by
              GDPR.
            </Typography>
          </Box>
        </Box>
      </DialogContent>
    </Dialog>
  );
}
