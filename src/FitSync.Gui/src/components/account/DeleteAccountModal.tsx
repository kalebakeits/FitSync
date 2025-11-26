import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  TextField,
  Box,
  Alert,
} from "@mui/material";
import { Warning } from "@mui/icons-material";

interface DeleteAccountModalProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isDeleting: boolean;
  error?: string;
}

export default function DeleteAccountModal({
  open,
  onClose,
  onConfirm,
  isDeleting,
  error,
}: DeleteAccountModalProps) {
  const [confirmText, setConfirmText] = useState("");
  const CONFIRM_PHRASE = "DELETE MY ACCOUNT";

  const handleConfirm = () => {
    if (confirmText === CONFIRM_PHRASE) {
      onConfirm();
    }
  };

  const handleClose = () => {
    if (!isDeleting) {
      setConfirmText("");
      onClose();
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ display: "flex", alignItems: "center", gap: 1 }}>
        <Warning color="error" />
        <Typography variant="h6">Delete Account</Typography>
      </DialogTitle>
      <DialogContent dividers>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          {error && <Alert severity="error">{error}</Alert>}
          <Alert severity="error">
            This action is permanent and cannot be undone!
          </Alert>

          <Typography variant="body2">
            Deleting your account will immediately:
          </Typography>

          <Box component="ul" sx={{ pl: 2, m: 0 }}>
            <Typography component="li" variant="body2" sx={{ mb: 1 }}>
              Delete all your credentials (encrypted passwords)
            </Typography>
            <Typography component="li" variant="body2" sx={{ mb: 1 }}>
              Delete your user profile and email
            </Typography>
            <Typography component="li" variant="body2" sx={{ mb: 1 }}>
              Stop all activity syncing
            </Typography>
            <Typography component="li" variant="body2" sx={{ mb: 1 }}>
              Delete all activity records and metadata
            </Typography>
            <Typography component="li" variant="body2">
              Log you out of all sessions
            </Typography>
          </Box>

          <Typography variant="body2" sx={{ fontWeight: 500, mt: 1 }}>
            To confirm deletion, type:{" "}
            <Typography
              component="span"
              sx={{ fontFamily: "monospace", color: "error.main" }}
            >
              {CONFIRM_PHRASE}
            </Typography>
          </Typography>

          <TextField
            fullWidth
            value={confirmText}
            onChange={(e) => setConfirmText(e.target.value)}
            placeholder={CONFIRM_PHRASE}
            disabled={isDeleting}
            error={confirmText.length > 0 && confirmText !== CONFIRM_PHRASE}
            helperText={
              confirmText.length > 0 && confirmText !== CONFIRM_PHRASE
                ? "Text does not match"
                : ""
            }
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isDeleting}>
          Cancel
        </Button>
        <Button
          onClick={handleConfirm}
          color="error"
          variant="contained"
          disabled={confirmText !== CONFIRM_PHRASE || isDeleting}
        >
          {isDeleting ? "Deleting..." : "Delete Account"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
