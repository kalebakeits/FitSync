namespace FitSync.Shared.Features.GlobalVariables;

public static class InstanceIdentity
{
    /// <summary>
    /// A configured instance id names a deployment, not a replica: one deployment runs several
    /// processes from one configuration (k8s replicas, AppHost <c>WithReplicas</c>), and every one
    /// of them would otherwise advertise the same heartbeat identity and claim work under the same
    /// worker lock. The machine name separates containers, where it is the pod name; the process id
    /// separates replicas of a local run, which all share one machine name.
    /// </summary>
    public static string Derive(string configuredInstanceId)
    {
        return $"{configuredInstanceId}-{Environment.MachineName}-{Environment.ProcessId}";
    }
}
