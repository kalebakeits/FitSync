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
import type {
  AvailableServiceResponse,
  CreateCredentialRequest,
} from "../../api/generated/fitSyncApi.schemas";

interface CredentialModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateCredentialRequest) => void;
  availableServices: AvailableServiceResponse[];
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

  const selectedService = availableServices.find(
    (s) => s.serviceType === serviceType
  );
  const isOAuth = selectedService?.authType === "oauth";

  const servicesToShow = editingCredential
    ? [
        {
          serviceType: editingCredential.serviceType,
          authType: "credentials",
          connectUrl: null,
        } as AvailableServiceResponse,
      ]
    : availableServices;

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
                <MenuItem key={service.serviceType} value={service.serviceType}>
                  {service.serviceType}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {serviceType && isOAuth ? (
            <Button
              variant="contained"
              fullWidth
              href={`${import.meta.env.VITE_API_URL ?? ""}${selectedService?.connectUrl ?? "#"}`}
            >
              Connect
            </Button>
          ) : (
            serviceType && (
              <>
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
            )
          )}
        </DialogContent>

        <DialogActions>
          <Button onClick={handleClose} disabled={isSubmitting}>
            Cancel
          </Button>
          {!isOAuth && serviceType && (
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
