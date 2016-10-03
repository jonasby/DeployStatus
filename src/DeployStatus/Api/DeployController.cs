using System;
using System.Web.Http;
using DeployStatus.ApiClients;
using log4net;

namespace DeployStatus.Api
{
    public class DeployController: ApiController
    {
        private readonly ILog log;
        private readonly TrelloClient trelloClient;

        public DeployController() : this(new TrelloClientFactory())
        {
        }

        public DeployController(TrelloClientFactory trelloClientFactory)
        {
            this.trelloClient = trelloClientFactory.Get();
            log = LogManager.GetLogger(GetType());
            log.Info("instantiated");
        }
        
        public object Get(string trello)
        {
            log.Info("Get request for deployment of " + trello);

            log.Info("Attempting TeamCity enqueue");

            var trelloCard = trelloClient.GetCardByShortId(GetShortId(trello));


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

        private string GetShortId(string url)
        {
            var absolutePath = new Uri(url).AbsolutePath;
            return absolutePath;
        }
    }
}