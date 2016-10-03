using DeployStatus.Api;
using NUnit.Framework;

namespace DeployStatus.UnitTests
{
    public class DeployControllerTests
    {
        [Test]
        public void CanParseAShortIdFromAurl()
        {
            var shortId = DeployController.GetShortId("https://trello.com/c/mRNTsyVo/3203-2-mt4link-backfill-on-start");
            Assert.That(shortId, Is.EqualTo("mRNTsyVo"));
        }

        [Test]
        public void CanParseSimpleBranchFromDescription()
        {
            var descr = @"`feature/we-relax-in-hammocks`

Note: Reason = Client. Can detect by group foup matches ""coverage.* "", but ""Cover+)"" is suitable

----

 When we receive hedge trades fwe know wha
This is needed for reporting in st trade, tight integration and stp environments.

###Dev

* Add a gateway hedge trade comment parser, Parse ""Cover For #nr#"" and send the nr oo s
* make sure understands it and links the client and hedge trades together";

            var branchName = DeployController.GetGitBranchName(descr);

            Assert.That(branchName, Is.EqualTo("feature/we-relax-in-hammocks"));
        }

        //[TestCase("035b078004b77318c44510ba3de333c1bdb634ee")]
        //[TestCase("develop")]
        [TestCase("feature/bob-cake")]
        [TestCase("feature/bob_cake_bad_underscores_naughty_bob")]
        [TestCase("support/swan-2.21.4-cap-index")]
        public void MoreBranchNameTests(string actualBranch)
        {
            var sampleDescription = $"Blah lbah test `branch missing` `{actualBranch}`";

            var branchName = DeployController.GetGitBranchName(sampleDescription);

            Assert.That(branchName, Is.EqualTo(actualBranch));
        }
    }
}