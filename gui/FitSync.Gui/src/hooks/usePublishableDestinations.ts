import { useGetApiConnections } from "../api/generated/connections/connections";
import { useGetApiCredentialsAll } from "../api/generated/credentials/credentials";
import { getPublishingServiceTypes } from "../utils/publishingServices";
import type { ConnectionResponse } from "../api/generated/fitSyncApi.schemas";

export function usePublishableDestinations(): ConnectionResponse[] {
  const { data: connections = [] } = useGetApiConnections();
  const { data: allServices = [] } = useGetApiCredentialsAll();

  const publishableServiceTypes = getPublishingServiceTypes(allServices);

  return connections.filter(
    (connection) =>
      connection.connected &&
      connection.enabled &&
      publishableServiceTypes.includes(connection.serviceType ?? ""),
  );
}
