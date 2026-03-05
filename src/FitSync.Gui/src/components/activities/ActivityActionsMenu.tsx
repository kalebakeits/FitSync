import { useState } from "react";
import { IconButton, Menu, MenuItem } from "@mui/material";
import { MoreVert } from "@mui/icons-material";
import type { ActivityResponse } from "../../api/generated/fitSyncApi.schemas";
import ConfirmModal from "../ConfirmModal";
import PushToDestinationModal from "./PushToDestinationModal";

const FAILED_STATUSES = new Set([4, 5, 6]);

interface ActivityActionsMenuProps {
  activity: ActivityResponse;
  onRetry: () => void;
  onDelete: () => void;
}

export default function ActivityActionsMenu({
  activity,
  onRetry,
  onDelete,
}: ActivityActionsMenuProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [pushOpen, setPushOpen] = useState(false);

  const hasFailed = activity.uploadStatuses?.some((u) =>
    FAILED_STATUSES.has(u.status),
  );

  const handleOpen = (e: React.MouseEvent<HTMLElement>) =>
    setAnchorEl(e.currentTarget);
  const handleClose = () => setAnchorEl(null);

  const handleRetry = () => {
    handleClose();
    onRetry();
  };

  const handleDeleteConfirm = () => {
    setDeleteOpen(false);
    onDelete();
  };

  return (
    <>
      <IconButton size="small" onClick={handleOpen} aria-label="activity actions">
        <MoreVert fontSize="small" />
      </IconButton>

      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleClose}>
        {hasFailed && (
          <MenuItem onClick={handleRetry}>Retry failed</MenuItem>
        )}
        <MenuItem
          onClick={() => {
            handleClose();
            setPushOpen(true);
          }}
        >
          Push to destination
        </MenuItem>
        <MenuItem
          onClick={() => {
            handleClose();
            setDeleteOpen(true);
          }}
          sx={{ color: "error.main" }}
        >
          Delete
        </MenuItem>
      </Menu>

      <ConfirmModal
        open={deleteOpen}
        title="Delete activity"
        message="This activity will be hidden immediately and permanently purged within 90 days. It cannot be re-fetched after deletion."
        confirmLabel="Delete"
        severity="error"
        onConfirm={handleDeleteConfirm}
        onClose={() => setDeleteOpen(false)}
      />

      <PushToDestinationModal
        open={pushOpen}
        activity={activity}
        onClose={() => setPushOpen(false)}
      />
    </>
  );
}
