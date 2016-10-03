using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using DeployStatus.ApiClients;
using log4net;

namespace DeployStatus.Api
{
    public class DeployController: ApiController
    {
        //\b([a-f0-9]{40})\b|(develop)| // could be dangerous/pointless deploying develop if it's historical, or a commit where it could be the oldest of several rather than the newest
        private static readonly Regex gitBranchNameRegex = new Regex(@"((hotfix|support|feature)\/[\w\d\-_\.]+)");
        private readonly ILog log;
        private readonly TrelloClient trelloClient;
        private readonly ApiClients.OctopusClient octopusClient;
        private readonly TeamCityClient teamCityClient;

        public DeployController() : this(
            new TrelloClientFactory(), 
            new OctopusApiClientFactory(),
            new TeamCityClientFactory())
        {
        }

        public DeployController(
            TrelloClientFactory trelloClientFactory, 
            OctopusApiClientFactory octopusClientFactory, 
            TeamCityClientFactory teamCityClientFactory)
        {
            teamCityClient = teamCityClientFactory.Get();
            trelloClient = trelloClientFactory.Get();
            octopusClient = octopusClientFactory.Get();
            log = LogManager.GetLogger(GetType());
            log.Info("instantiated");
        }
        
        public async Task<object> Get(string trello)
        {
            log.Info("Get request for deployment of " + trello);

            var card = await trelloClient.GetCardByShortId(GetShortId(trello));
            var description = card.Desc;
            
            // is this a trello we can deploy?
            var isValid = octopusClient.GetEnvironments().Any(x => card.Labels.Contains(x.Name));

            if (!isValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No label on the card matches an Octopus environment name");

            var gitBranchName = GetGitBranchName(description);

            if (string.IsNullOrWhiteSpace(gitBranchName))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No branch name could be found on the card description");

            var deployDetails = teamCityClient.EnqueueADeploymentOfBranch(gitBranchName);

            return new
            {
                success = true,
                branchName = "feature/xyz",
                environmentName = "CAP",
                buildNumber = "1.2.12321.0",
                buildLink= "http://sfs-autobuild/test",
                octopusLink = "http://sfs-autodeploy/test"
            };
        }

        public static string GetGitBranchName(string description)
        {
            var matches = gitBranchNameRegex.Matches(description);
            foreach (Match match in matches)
            {
                foreach (Group group in match.Groups)
                    if (!string.IsNullOrWhiteSpace(group.Value))
                        return group.Value;
            }
            return null;
        }

        public static string GetShortId(string url)
        {
            var absolutePath = new Uri(url).AbsolutePath.Split('/').Skip(2);
            return absolutePath.First();
        }
    }
}