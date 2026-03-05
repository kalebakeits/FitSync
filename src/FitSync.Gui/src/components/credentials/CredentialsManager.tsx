import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  getGetApiCredentialsAvailableQueryKey,
  getGetApiCredentialsQueryKey,
  useGetApiCredentialsAvailable,
  usePostApiCredentials,
} from "../../api/generated/credentials/credentials";
import {
  getGetApiConnectionsQueryKey,
  useDeleteApiConnectionsServiceType,
  useGetApiConnections,
} from "../../api/generated/connections/connections";
import type {
  ConnectionResponse,
  CreateCredentialRequest,
} from "../../api/generated/fitSyncApi.schemas";
import DashboardColumn from "../dashboard/DashboardColumn";
import CredentialModal from "./CredentialModal";
import CredentialsContent from "./CredentialsContent";
import CredentialsHeader from "./CredentialsHeader";

export default function CredentialsManager() {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [editingCredential, setEditingCredential] = useState<{
    serviceType: string;
    username: string;
  } | null>(null);

  const { data: connections = [], isLoading } = useGetApiConnections({
    query: { refetchInterval: 60000 },
  });

  const { data: availableServices = [] } = useGetApiCredentialsAvailable({
    query: { enabled: !editingCredential },
  });

  const addMutation = usePostApiCredentials({
    mutation: {
      onSuccess: () => {
        setModalOpen(false);
        setEditingCredential(null);
        queryClient.invalidateQueries({ queryKey: getGetApiConnectionsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiCredentialsQueryKey() });
      },
    },
  });

  const disconnectMutation = useDeleteApiConnectionsServiceType({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiConnectionsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiCredentialsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiCredentialsAvailableQueryKey() });
      },
    },
  });

  const handleOpenModal = (connection?: ConnectionResponse) => {
    if (connection) {
      setEditingCredential({
        serviceType: connection.serviceType ?? "",
        username: connection.displayName ?? "",
      });
    } else {
      setEditingCredential(null);
    }
    setModalOpen(true);
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setEditingCredential(null);
  };

  const handleSubmit = (data: CreateCredentialRequest) => {
    addMutation.mutate({ data });
  };

  const handleDisconnect = (serviceType: string) => {
    if (confirm(`Are you sure you want to remove your ${serviceType} connection?`)) {
      disconnectMutation.mutate({ serviceType });
    }
  };


  return (
    <>
      <CredentialModal
        open={modalOpen}
        onClose={handleCloseModal}
        onSubmit={handleSubmit}
        availableServices={availableServices}
        isSubmitting={addMutation.isPending}
        error={
          addMutation.isError
            ? (addMutation.error as any)?.response?.data?.message ||
              "Failed to save credentials"
            : undefined
        }
        editingCredential={editingCredential}
      />

      <DashboardColumn
        header={<CredentialsHeader onAddClick={() => handleOpenModal()} />}
        body={
          <CredentialsContent
            connections={connections}
            isLoading={isLoading}
            isProcessing={disconnectMutation.isPending}
            onEdit={handleOpenModal}
            onDisconnect={handleDisconnect}
          />
        }
      />
    </>
  );
}
