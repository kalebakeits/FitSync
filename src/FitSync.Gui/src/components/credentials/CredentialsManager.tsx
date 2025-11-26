import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  getGetApiCredentialsQueryKey,
  useDeleteApiCredentialsServiceType,
  useGetApiCredentials,
  useGetApiCredentialsAvailable,
  usePostApiCredentials,
} from "../../api/generated/credentials/credentials";
import type { CreateCredentialRequest } from "../../api/generated/fitSyncApi.schemas";
import DashboardColumn from "../dashboard/DashboardColumn";
import CredentialModal from "./CredentialModal";
import CredentialsContent from "./CredentialsContent";
import CredentialsHeader from "./CredentialsHeader";

interface Credential {
  serviceType: string;
  username: string;
  createdAt: string;
  updatedAt: string;
  enabled: boolean;
}

export default function CredentialsManager() {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [editingCredential, setEditingCredential] = useState<{
    serviceType: string;
    username: string;
  } | null>(null);

  const { data: credentials, isLoading } = useGetApiCredentials({
    query: { refetchInterval: 60000 },
  });
  const { data: availableServicesData } = useGetApiCredentialsAvailable({
    query: { enabled: modalOpen && !editingCredential },
  });

  const addMutation = usePostApiCredentials({
    mutation: {
      onSuccess: () => {
        setModalOpen(false);
        setEditingCredential(null);
        queryClient.invalidateQueries({
          queryKey: getGetApiCredentialsQueryKey(),
        });
      },
    },
  });

  const deleteMutation = useDeleteApiCredentialsServiceType({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiCredentialsQueryKey(),
        });
      },
    },
  });

  const handleOpenModal = (credential?: Credential) => {
    if (credential) {
      setEditingCredential({
        serviceType: credential.serviceType,
        username: credential.username,
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

  const handleDelete = (serviceType: string) => {
    if (
      confirm(
        `Are you sure you want to delete your ${serviceType} credentials?`,
      )
    ) {
      deleteMutation.mutate({ serviceType });
    }
  };

  const credentialsList = (credentials as unknown as Credential[]) || [];

  return (
    <>
      <CredentialModal
        open={modalOpen}
        onClose={handleCloseModal}
        onSubmit={handleSubmit}
        availableServices={(availableServicesData as unknown as string[]) || []}
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
            credentials={credentialsList}
            isLoading={isLoading}
            isDeleting={deleteMutation.isPending}
            onEdit={handleOpenModal}
            onDelete={handleDelete}
          />
        }
      />
    </>
  );
}
