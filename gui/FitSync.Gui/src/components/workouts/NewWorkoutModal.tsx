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
import {
  usePostApiWorkouts,
  getGetApiWorkoutsQueryKey,
} from "../../api/generated/workouts/workouts";
import PROMPT from "../../../../../assets/workout-generation-prompt.txt?raw";
import type { WorkoutSchema } from "../../api/generated/fitSyncApi.schemas";
import WorkoutSchemaPreview from "./WorkoutSchemaPreview";

interface Props {
  open: boolean;
  onClose: () => void;
}

function isWorkoutSchema(value: unknown): value is WorkoutSchema {
  return typeof value === "object" && value !== null;
}

export default function NewWorkoutModal({ open, onClose }: Props) {
  const [json, setJson] = useState("");
  const [copied, setCopied] = useState(false);
  const queryClient = useQueryClient();

  const finish = () => {
    setJson("");
    mutation.reset();
    onClose();
  };

  const mutation = usePostApiWorkouts({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiWorkoutsQueryKey(),
        });
        finish();
      },
    },
  });

  const handleCopy = () => {
    navigator.clipboard.writeText(PROMPT);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const parsed: { ok: boolean; value: unknown } = (() => {
    if (!json.trim()) return { ok: false, value: null };
    try {
      return { ok: true, value: JSON.parse(json) };
    } catch {
      return { ok: false, value: null };
    }
  })();

  const isValidJson = parsed.ok;

  const handleSubmit = () => {
    if (!isWorkoutSchema(parsed.value)) return;
    mutation.mutate({ data: parsed.value });
  };

  return (
    <Dialog open={open} onClose={finish} maxWidth="md" fullWidth>
      <DialogTitle>New Workout</DialogTitle>
      <DialogContent>
        <Box sx={{ mb: 2 }}>
          <Tooltip
            title={
              copied ? "Copied!" : "Copy the schema prompt to paste into an LLM"
            }
          >
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
          onChange={(e) => {
            setJson(e.target.value);
            mutation.reset();
          }}
          placeholder="Paste the JSON from your LLM here..."
          error={!!json.trim() && !isValidJson}
          helperText={
            !!json.trim() && !isValidJson ? "Invalid JSON" : undefined
          }
        />

        {isValidJson && <WorkoutSchemaPreview schema={parsed.value} />}

        {mutation.isError && (
          <Alert severity="error" sx={{ mt: 2 }}>
            Failed to create workout.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={finish}>Cancel</Button>
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
