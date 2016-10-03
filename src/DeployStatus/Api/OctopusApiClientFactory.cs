using DeployStatus.SignalR;

namespace DeployStatus.Api
{
    public class OctopusApiClientFactory
    {
        public ApiClients.OctopusClient Get()
        {
            return DeployStatusServiceContainer.Apis.Value.Octopus;
        }
    }
}