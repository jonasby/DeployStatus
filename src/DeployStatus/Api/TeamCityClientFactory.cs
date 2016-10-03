using DeployStatus.ApiClients;
using DeployStatus.SignalR;

namespace DeployStatus.Api
{
    public class TeamCityClientFactory
    {
        public TeamCityClient Get()
        {
            return DeployStatusServiceContainer.Apis.Value.TeamCity;
        }
    }
}