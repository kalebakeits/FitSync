import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Box, Divider, Typography } from "@mui/material";
import {
  getGetApiConnectionsMappingsQueryKey,
  getGetApiConnectionsQueryKey,
  getGetApiConnectionsStatusQueryKey,
  useDeleteApiConnectionsServiceType,
  useGetApiConnectionsMappings,
  useGetApiConnections,
  usePutApiConnectionsMappings,
} from "../../api/generated/connections/connections";
import {
  getGetApiCredentialsQueryKey,
  useGetApiCredentialsAll,
  useGetApiCredentialsAvailable,
  usePostApiCredentials,
} from "../../api/generated/credentials/credentials";
import type { CreateCredentialRequest } from "../../api/generated/fitSyncApi.schemas";
import CredentialModal from "../credentials/CredentialModal";
import ConnectedServicesSection from "./ConnectedServicesSection";
import FetcherDestinationCard from "./FetcherDestinationCard";
import ConfirmModal from "../ConfirmModal";

export default function IntegrationsTab() {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<{ serviceType: string; username: string } | null>(null);
  const [disconnectTarget, setDisconnectTarget] = useState<string | null>(null);

  const { data: connections = [] } = useGetApiConnections({ query: { refetchInterval: 10000 } });
  const { data: mappings = [] } = useGetApiConnectionsMappings();
  const { data: allServices = [] } = useGetApiCredentialsAll();
  const { data: availableServices = [] } = useGetApiCredentialsAvailable({ query: { enabled: !editing } });

  const fetchers = connections.filter((c) => allServices.find((s) => s.serviceType === c.serviceType)?.isFetcher);
  const uploaders = allServices.filter((s) => s.isUploader);

  const invalidateStatus = () => queryClient.invalidateQueries({ queryKey: getGetApiConnectionsStatusQueryKey() });

  const addMutation = usePostApiCredentials({
    mutation: {
      onSuccess: () => {
        setModalOpen(false);
        setEditing(null);
        queryClient.invalidateQueries({ queryKey: getGetApiConnectionsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiCredentialsQueryKey() });
        invalidateStatus();
      },
    },
  });

  const disconnectMutation = useDeleteApiConnectionsServiceType({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiConnectionsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiCredentialsQueryKey() });
        invalidateStatus();
      },
    },
  });

  const mappingMutation = usePutApiConnectionsMappings({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiConnectionsMappingsQueryKey() });
        invalidateStatus();
      },
    },
  });

  const getMappedDests = (src: string) =>
    mappings.find((m) => m.sourceServiceType === src)?.destinationServiceTypes ?? [];

  const handleToggle = (fetcherType: string, dest: string, enabled: boolean) => {
    const current = getMappedDests(fetcherType);
    const next = enabled ? [...current, dest] : current.filter((d) => d !== dest);
    mappingMutation.mutate({ data: { sourceServiceType: fetcherType, destinationServiceTypes: next } });
  };

  const handleDisconnectConfirm = () => {
    if (disconnectTarget) {
      disconnectMutation.mutate({ serviceType: disconnectTarget });
    }
    setDisconnectTarget(null);
  };

  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
      <ConnectedServicesSection
        connections={connections}
        isProcessing={disconnectMutation.isPending}
        onAdd={() => { setEditing(null); setModalOpen(true); }}
        onEdit={(c) => { setEditing({ serviceType: c.serviceType ?? "", username: c.displayName ?? "" }); setModalOpen(true); }}
        onDisconnect={(st) => setDisconnectTarget(st)}
      />
      <Divider />
      <Box>
        <Typography variant="h6" gutterBottom>Destination Routing</Typography>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          {fetchers.map((f) => (
            <FetcherDestinationCard
              key={f.serviceType}
              fetcherServiceType={f.serviceType ?? ""}
              uploaders={uploaders}
              connections={connections}
              enabledDestinations={getMappedDests(f.serviceType ?? "")}
              isPending={mappingMutation.isPending}
              onToggle={(dest, enabled) => handleToggle(f.serviceType ?? "", dest, enabled)}
            />
          ))}
          {fetchers.length === 0 && (
            <Typography variant="body2" color="text.secondary">
              Connect a source (Zwift or Wahoo) to configure routing.
            </Typography>
          )}
        </Box>
      </Box>

      <CredentialModal
        open={modalOpen}
        onClose={() => { setModalOpen(false); setEditing(null); }}
        onSubmit={(data: CreateCredentialRequest) => addMutation.mutate({ data })}
        availableServices={availableServices}
        isSubmitting={addMutation.isPending}
        error={addMutation.isError ? "Failed to save credentials" : undefined}
        editingCredential={editing}
      />

      <ConfirmModal
        open={Boolean(disconnectTarget)}
        title="Disconnect service"
        message={`Remove ${disconnectTarget} connection? This cannot be undone.`}
        confirmLabel="Disconnect"
        severity="warning"
        onConfirm={handleDisconnectConfirm}
        onClose={() => setDisconnectTarget(null)}
      />
    </Box>
  );
}
