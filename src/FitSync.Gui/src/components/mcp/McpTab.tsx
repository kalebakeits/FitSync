import { useState } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  InputAdornment,
  List,
  ListItem,
  ListItemText,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { Add, ContentCopy, Delete } from "@mui/icons-material";
import {
  useGetApiTokens,
  usePostApiTokens,
  useDeleteApiTokensId,
} from "../../api/generated/tokens/tokens";

export default function McpTab() {
  const [createOpen, setCreateOpen] = useState(false);
  const [newTokenName, setNewTokenName] = useState("");
  const [createdToken, setCreatedToken] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const { data: tokens = [], refetch } = useGetApiTokens();

  const createMutation = usePostApiTokens({
    mutation: {
      onSuccess: (data) => {
        setCreatedToken(data.token ?? null);
        setNewTokenName("");
        refetch();
      },
    },
  });

  const deleteMutation = useDeleteApiTokensId({
    mutation: { onSuccess: () => refetch() },
  });

  const handleCreate = () => {
    if (!newTokenName.trim()) return;
    createMutation.mutate({ data: { name: newTokenName.trim() } });
  };

  const handleCopy = (text: string) => {
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleClose = () => {
    setCreateOpen(false);
    setCreatedToken(null);
    setNewTokenName("");
  };

  const mcpUrl = `${window.location.origin}/mcp`;

  return (
    <Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Connect AI assistants to FitSync via MCP. Paste the MCP URL into your AI tool — it will handle authentication automatically. If your tool requires a token, create one below.
      </Typography>

      <Box sx={{ mb: 3, p: 2, bgcolor: "action.hover", borderRadius: 1 }}>
        <Typography variant="caption" color="text.secondary">
          MCP server URL
        </Typography>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, mt: 0.5 }}>
          <Typography variant="body2" fontFamily="monospace" sx={{ flexGrow: 1, wordBreak: "break-all" }}>
            {mcpUrl}
          </Typography>
          <Tooltip title="Copy">
            <IconButton size="small" onClick={() => handleCopy(mcpUrl)}>
              <ContentCopy fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
        <Typography variant="subtitle2">Connected apps</Typography>
        <Button size="small" startIcon={<Add />} onClick={() => setCreateOpen(true)}>
          New token
        </Button>
      </Box>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
        Each token represents a connected app. Delete a token to revoke its access.
      </Typography>

      {tokens.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No connected apps yet.
        </Typography>
      ) : (
        <List dense disablePadding>
          {tokens.map((token) => (
            <ListItem
              key={token.id}
              disablePadding
              secondaryAction={
                <IconButton
                  edge="end"
                  size="small"
                  onClick={() => deleteMutation.mutate({ id: token.id! })}
                >
                  <Delete fontSize="small" />
                </IconButton>
              }
            >
              <ListItemText
                primary={token.name}
                secondary={`Created ${new Date(token.createdAt!).toLocaleDateString()}${token.lastUsedAt ? ` · Last used ${new Date(token.lastUsedAt).toLocaleDateString()}` : ""}`}
              />
            </ListItem>
          ))}
        </List>
      )}

      <Dialog open={createOpen} onClose={handleClose} maxWidth="xs" fullWidth>
        <DialogTitle>{createdToken ? "Token created" : "New token"}</DialogTitle>
        <DialogContent>
          {createdToken ? (
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Copy this token now — it won't be shown again.
              </Typography>
              <TextField
                fullWidth
                size="small"
                value={createdToken}
                InputProps={{
                  readOnly: true,
                  endAdornment: (
                    <InputAdornment position="end">
                      <Tooltip title={copied ? "Copied!" : "Copy"}>
                        <IconButton size="small" onClick={() => handleCopy(createdToken)}>
                          <ContentCopy fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </InputAdornment>
                  ),
                }}
                slotProps={{ htmlInput: { style: { fontFamily: "monospace", fontSize: 12 } } }}
              />
            </Box>
          ) : (
            <TextField
              autoFocus
              fullWidth
              size="small"
              label="Name"
              placeholder="e.g. Claude Desktop"
              value={newTokenName}
              onChange={(e) => setNewTokenName(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleCreate()}
              sx={{ mt: 1 }}
            />
          )}
        </DialogContent>
        <DialogActions>
          {createdToken ? (
            <Button onClick={handleClose}>Done</Button>
          ) : (
            <>
              <Button onClick={handleClose}>Cancel</Button>
              <Button
                variant="contained"
                onClick={handleCreate}
                disabled={!newTokenName.trim() || createMutation.isPending}
              >
                Create
              </Button>
            </>
          )}
        </DialogActions>
      </Dialog>
    </Box>
  );
}
