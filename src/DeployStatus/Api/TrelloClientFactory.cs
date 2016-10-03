using DeployStatus.ApiClients;
using DeployStatus.SignalR;

namespace DeployStatus.Api
{
    public class TrelloClientFactory
    {
        public TrelloClient Get()
        {
            return DeployStatusServiceContainer.Apis.Value.Trello;
        }
    }
}