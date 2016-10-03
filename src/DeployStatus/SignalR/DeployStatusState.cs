using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using DeployStatus.ApiClients;
using DeployStatus.Configuration;
using log4net;
using Microsoft.AspNet.SignalR;

namespace DeployStatus.SignalR
{
    /// <summary>
    /// Holds all the state used by the web UI. Used as a singleton
    /// </summary>
    public class DeployStatusState
    {
        private DeploySystemStatus deploySystemStatus = new DeploySystemStatus("Starting system...", DateTime.UtcNow, Enumerable.Empty<Environment>());

        private readonly IHubContext<IDeployStatusClient> context;
        private readonly Timer timer;
        private readonly DeployStatusInfoClient deployStatusInfoClient;
        private readonly ILog log;
        private readonly string name;

        public DeployStatusState(
            IHubContext<IDeployStatusClient> context,
            DeployStatusInfoClient deployStatusInfoClient, 
            string name)
        {
            this.context = context;
            timer = new Timer(UpdateDeploySystemStatus);
            log = LogManager.GetLogger(typeof (DeployStatusState));
            this.deployStatusInfoClient = deployStatusInfoClient;
            this.name = name;
        }
        
        public void Start()
        {
            timer.Change(TimeSpan.FromMinutes(0), Timeout.InfiniteTimeSpan);
            log.Info("Timer started");
        }

        private async void UpdateDeploySystemStatus(object state)
        {
            try
            {
                log.Info("Deploy system state polling started.");
                var started = DateTime.Now;
                var status = await deployStatusInfoClient.GetDeployStatus();
                log.InfoFormat("Deploy system state polling finished in {0}ms.", (DateTime.Now - started).TotalMilliseconds);

                log.Info("Pushing out update via SignalR.");
                var newEnvironments = GetEnvironments(status, deployStatusInfoClient.DeployUserResolver);
                var newDeploySystemStatus = new DeploySystemStatus(name, DateTime.UtcNow, newEnvironments);

                Interlocked.Exchange(ref deploySystemStatus, newDeploySystemStatus);

                context.Clients.All.DeploySystemStatusChanged(newDeploySystemStatus);
                log.Info("SignalR update pushed.");
            }
            catch (Exception ex)
            {
                log.Error($"Error occurred polling for deploy status: {ex}.", ex);
                Debug.Assert(true, ex.ToString());
            }

            timer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(-1));
        }

        public DeploySystemStatus GetDeploySystemStatus()
        {
             return deploySystemStatus;
        }

        private static IList<Environment> GetEnvironments(IEnumerable<DeployStatusInfo> status, IDeployUserResolver deployUserResolver)
        {            
            return
                status.Select(
                    x =>
                        new Environment(
                            x.Environment.Id, x.Environment.Name, x.Environment.ReleaseVersion,
                            x.Environment.StartTime.GetValueOrDefault(), 
                            x.Environment.State, x.BranchName, x.Environment.AbsoluteDeployLink, 
                            GetNormalizedName(deployUserResolver.GetDeployer(x)), 
                            x.Environment.Machines.Any(y => y.IsDisabled),
                            x.BranchRelatedTrellos.Select(GetTrelloCard).ToList(),
                            x.EnvironmentTaggedTrellos.Select(GetTrelloCard).ToList(),
                            x.BuildInfo.Select(GetBuildInfo))).ToList();
        }

        private static string GetNormalizedName(string deployerName)
        {
            var undottedName = deployerName.Replace('.', ' ');

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(undottedName);
        }

        private static Trello GetTrelloCard(TrelloCardInfo trelloCardInfo)
        {
            return new Trello(trelloCardInfo.Id, trelloCardInfo.Name, trelloCardInfo.Url);
        }

        private static TeamCityBuild GetBuildInfo(TeamCityBuildInfo buildInfo)
        {
            return new TeamCityBuild(buildInfo.Id, buildInfo.BuildTypeId, buildInfo.WebUrl, buildInfo.Status, buildInfo.TriggeredAt);
        }

        public void Stop()
        {
            log.Info("Stopping service...");
            timer.Dispose();
            log.Info("Service stopped.");
        }
    }
}