using System.Web.Http;
using log4net;

namespace DeployStatus.Api
{
    public class DeployController: ApiController
    {
        private readonly ILog log;

        public DeployController()
        {
            log = LogManager.GetLogger(GetType());
            log.Info("instantiated");
        }
        
        public object Get(string trello)
        {
            log.Info("Get request." + trello);

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
    }
}