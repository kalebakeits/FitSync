import { useState } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Alert,
  Box,
  Tooltip,
} from "@mui/material";
import { ContentCopy, Check } from "@mui/icons-material";
import { useQueryClient } from "@tanstack/react-query";
import { usePostApiWorkouts, getGetApiWorkoutsQueryKey } from "../../api/generated/workouts/workouts";
import PROMPT from "../../../../../assets/workout-generation-prompt.txt?raw";

interface Props {
  open: boolean;
  onClose: () => void;
}

export default function NewWorkoutModal({ open, onClose }: Props) {
  const [json, setJson] = useState("");
  const [copied, setCopied] = useState(false);
  const queryClient = useQueryClient();

  const mutation = usePostApiWorkouts({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiWorkoutsQueryKey() });
        setJson("");
        onClose();
      },
    },
  });

  const handleCopy = () => {
    navigator.clipboard.writeText(PROMPT);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleSubmit = () => {
    let parsed: unknown;
    try {
      parsed = JSON.parse(json);
    } catch {
      return;
    }
    mutation.mutate({ data: parsed as Record<string, unknown> });
  };

  const isValidJson = (() => {
    if (!json.trim()) return false;
    try { JSON.parse(json); return true; } catch { return false; }
  })();

  const handleClose = () => {
    setJson("");
    mutation.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>New Workout</DialogTitle>
      <DialogContent>
        <Box sx={{ mb: 2 }}>
          <Tooltip title={copied ? "Copied!" : "Copy the schema prompt to paste into an LLM"}>
            <Button
              variant="outlined"
              startIcon={copied ? <Check /> : <ContentCopy />}
              onClick={handleCopy}
              color={copied ? "success" : "primary"}
            >
              {copied ? "Copied!" : "Copy Generation Prompt"}
            </Button>
          </Tooltip>
        </Box>

        <TextField
          label="Workout JSON"
          multiline
          rows={14}
          fullWidth
          value={json}
          onChange={(e) => { setJson(e.target.value); mutation.reset(); }}
          placeholder="Paste the JSON from your LLM here..."
          error={!!json.trim() && !isValidJson}
          helperText={!!json.trim() && !isValidJson ? "Invalid JSON" : undefined}
        />

        {mutation.isError && (
          <Alert severity="error" sx={{ mt: 2 }}>Failed to create workout.</Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={!isValidJson || mutation.isPending}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
}
