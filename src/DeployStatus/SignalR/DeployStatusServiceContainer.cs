using System;
using DeployStatus.ApiClients;
using DeployStatus.Configuration;
using Microsoft.AspNet.SignalR;

namespace DeployStatus.SignalR
{
    public static class DeployStatusServiceContainer
    {
        public static readonly Lazy<DeployStatusState> State = new Lazy<DeployStatusState>(StateFactory);

        public static readonly Lazy<DeployStatusInfoClient> Apis = new Lazy<DeployStatusInfoClient>(ApiFactory);

        private static DeployStatusInfoClient ApiFactory()
        {
            var deployConfiguration = DeployStatusSettingsSection.Settings.AsDeployConfiguration();
            var deployStatusInfoClient = new DeployStatusInfoClient(deployConfiguration);
            return deployStatusInfoClient;
        }

        private static DeployStatusState StateFactory()
        {
            var deployConfiguration = DeployStatusSettingsSection.Settings.AsDeployConfiguration();
            var deployStatusInfoClient = Apis.Value;

            return new DeployStatusState(
                GlobalHost.ConnectionManager.GetHubContext<DeployStatusHub, IDeployStatusClient>(),
                deployStatusInfoClient,
                deployConfiguration.Name);
        }
    }
}