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
import { useDeleteApiAccount } from "../../api/generated/account/account";
import { useAuth } from "../../contexts/AuthContext";

interface Props {
  open: boolean;
  onClose: () => void;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index } = props;
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

export default function ProfileSettingsModal({ open, onClose }: Props) {
  const [tabValue, setTabValue] = useState(0);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const navigate = useNavigate();
  const { logout } = useAuth();

  const deleteMutation = useDeleteApiAccount({
    mutation: {
      onSuccess: () => {
        logout();
        navigate("/login");
      },
    },
  });

  const handleDeleteAccount = () => {
    deleteMutation.mutate();
  };

  return (
    <>
      <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
        <DialogTitle>
          Account Settings
          <IconButton
            onClick={onClose}
            sx={{ position: "absolute", right: 8, top: 8 }}
          >
            <Close />
          </IconButton>
        </DialogTitle>

        <DialogContent>
          <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
            <Tab label="Account" />
            <Tab label="Password" />
          </Tabs>

          <TabPanel value={tabValue} index={0}>
            <UpdateUsernameForm />
            <Divider sx={{ my: 4 }} />
            <UpdateEmailForm />
            <Divider sx={{ my: 4 }} />
            <Box>
              <Button
                variant="outlined"
                color="error"
                startIcon={<DeleteForever />}
                onClick={() => setDeleteModalOpen(true)}
                fullWidth
              >
                Delete Account
              </Button>
            </Box>
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            <UpdatePasswordForm />
          </TabPanel>
        </DialogContent>
      </Dialog>

      <DeleteAccountModal
        open={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        onConfirm={handleDeleteAccount}
        isDeleting={deleteMutation.isPending}
        error={
          deleteMutation.isError
            ? "Failed to delete account. Please try again."
            : undefined
        }
      />
    </>
  );
}
