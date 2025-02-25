using OpenTelemetry.Resources;

namespace Api;

public class KubernetesResourceDetector : IResourceDetector
{
    private const string KubernetesServiceHostEnvVar = "KUBERNETES_SERVICE_HOST";
    private const string AttributeServiceInstance = "service.instance.id";

    public Resource Detect()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(KubernetesServiceHostEnvVar)))
            return Resource.Empty; // Not running in Kubernetes

        //Set service.name to hostname
        return new Resource([
            new KeyValuePair<string, object>(AttributeServiceInstance, Environment.MachineName)
        ]);
    }
}