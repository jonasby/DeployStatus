﻿using Microsoft.AspNet.SignalR;

namespace DeployStatus.SignalR
{
    public class DeployStatusHub : Hub<IDeployStatusClient>
    {
        public DeploySystemStatus GetDeploySystemStatus()
        {
            return DeployStatusServiceContainer.State.Value.GetDeploySystemStatus();
        }
    }
}