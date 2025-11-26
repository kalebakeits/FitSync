import { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Alert,
  Box,
} from "@mui/material";
import type { CreateCredentialRequest } from "../../api/generated/fitSyncApi.schemas";

interface CredentialModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateCredentialRequest) => void;
  availableServices: string[];
  isSubmitting: boolean;
  error?: string;
  editingCredential?: {
    serviceType: string;
    username: string;
  } | null;
}

export default function CredentialModal({
  open,
  onClose,
  onSubmit,
  availableServices,
  isSubmitting,
  error,
  editingCredential,
}: CredentialModalProps) {
  const [serviceType, setServiceType] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  useEffect(() => {
    if (editingCredential) {
      setServiceType(editingCredential.serviceType);
      setUsername(editingCredential.username);
      setPassword("");
    } else {
      setServiceType("");
      setUsername("");
      setPassword("");
    }
  }, [editingCredential, open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({ serviceType, username, password });
  };

  const handleClose = () => {
    setServiceType("");
    setUsername("");
    setPassword("");
    onClose();
  };

  const servicesToShow = editingCredential
    ? [editingCredential.serviceType]
    : availableServices;

  const showNoServicesMessage =
    !editingCredential && availableServices.length === 0;

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {editingCredential ? "Update Credentials" : "Add New Credentials"}
      </DialogTitle>

      <Box component="form" onSubmit={handleSubmit}>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {showNoServicesMessage ? (
            <Alert severity="info">
              No available services. You have already added all supported
              services.
            </Alert>
          ) : (
            <>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Service</InputLabel>
                <Select
                  value={serviceType}
                  label="Service"
                  onChange={(e) => setServiceType(e.target.value)}
                  required
                  disabled={!!editingCredential}
                >
                  {servicesToShow.map((service) => (
                    <MenuItem key={service} value={service}>
                      {service}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                fullWidth
                label="Username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="Password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                sx={{ mb: 2 }}
                helperText={
                  editingCredential
                    ? "Enter your password to update credentials"
                    : ""
                }
              />
            </>
          )}
        </DialogContent>

        <DialogActions>
          <Button onClick={handleClose} disabled={isSubmitting}>
            Cancel
          </Button>
          {!showNoServicesMessage && (
            <Button
              type="submit"
              variant="contained"
              disabled={isSubmitting || !serviceType || !username || !password}
            >
              {isSubmitting
                ? "Saving..."
                : editingCredential
                  ? "Update"
                  : "Add"}
            </Button>
          )}
        </DialogActions>
      </Box>
    </Dialog>
  );
}
