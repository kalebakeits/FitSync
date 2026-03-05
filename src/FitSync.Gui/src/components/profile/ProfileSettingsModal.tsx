import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Tabs,
  Tab,
  Box,
  Divider,
  Button,
} from "@mui/material";
import { Close, DeleteForever } from "@mui/icons-material";
import UpdateUsernameForm from "./UpdateUsernameForm";
import UpdateEmailForm from "./UpdateEmailForm";
import UpdatePasswordForm from "./UpdatePasswordForm";
import DeleteAccountModal from "../account/DeleteAccountModal";
import IntegrationsTab from "../integrations/IntegrationsTab";
import { useDeleteApiAccount } from "../../api/generated/account/account";
import { useAuth } from "../../contexts/AuthContext";

interface Props {
  open: boolean;
  onClose: () => void;
  initialTab?: number;
}

function TabPanel({ children, value, index }: { children?: React.ReactNode; value: number; index: number }) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

export default function ProfileSettingsModal({ open, onClose, initialTab = 0 }: Props) {
  const [tab, setTab] = useState(initialTab);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const navigate = useNavigate();
  const { logout } = useAuth();

  const deleteMutation = useDeleteApiAccount({
    mutation: { onSuccess: () => { logout(); navigate("/login"); } },
  });

  return (
    <>
      <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
        <DialogTitle>
          Settings
          <IconButton onClick={onClose} sx={{ position: "absolute", right: 8, top: 8 }}>
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <Tabs value={tab} onChange={(_, v) => setTab(v)} variant="scrollable" scrollButtons="auto">
            <Tab label="Account" />
            <Tab label="Password" />
            <Tab label="Integrations" />
          </Tabs>

          <TabPanel value={tab} index={0}>
            <UpdateUsernameForm />
            <Divider sx={{ my: 4 }} />
            <UpdateEmailForm />
            <Divider sx={{ my: 4 }} />
            <Button
              variant="outlined"
              color="error"
              startIcon={<DeleteForever />}
              onClick={() => setDeleteOpen(true)}
              fullWidth
            >
              Delete Account
            </Button>
          </TabPanel>

          <TabPanel value={tab} index={1}>
            <UpdatePasswordForm />
          </TabPanel>

          <TabPanel value={tab} index={2}>
            <IntegrationsTab />
          </TabPanel>
        </DialogContent>
      </Dialog>

      <DeleteAccountModal
        open={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        onConfirm={() => deleteMutation.mutate()}
        isDeleting={deleteMutation.isPending}
        error={deleteMutation.isError ? "Failed to delete account. Please try again." : undefined}
      />
    </>
  );
}
