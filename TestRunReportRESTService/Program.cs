
using System;
using System.Configuration;
using System.Linq;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace TestRunReportRESTService
{
    class Program
    {
        static void Main(string[] args)
        {
            var definitionNames = ConfigurationManager.AppSettings["BuildDefinitions"].Split(';');
            var sender = ConfigurationManager.AppSettings["Sender"];
            var recipient = ConfigurationManager.AppSettings["Recipient"];

            var tfsClientFactory = new TfsClientFactory();

            TeamProjectReference project;

            using (var projectClient = tfsClientFactory.GetProjectClient())
            {
                project = projectClient.GetProjects().Result.First(p => p.Name == TfsClientFactory.TeamProject);
            }

            foreach (var dn in definitionNames)
            {
                Build lastBuild;
                DefinitionReference buildDefinition;

                using (var buildClient = tfsClientFactory.GetBuildClient())
                {
                    buildDefinition = buildClient.GetDefinitionsAsync(TfsClientFactory.TeamProject, dn)
                        .Result
                        .SingleOrDefault();

                    if (buildDefinition == null)
                    {
                        continue;
                    }

                    lastBuild =
                        buildClient.GetBuildsAsync(TfsClientFactory.TeamProject, new[] { buildDefinition.Id })
                            .Result
                            .OrderBy(b => b.FinishTime)
                            .Last();
                }

                using (var testClient = tfsClientFactory.GetTestManagementClient())
                {
                    var testRuns =
                        testClient.GetTestRunsAsync(project.Id.ToString(), lastBuild.Uri.AbsoluteUri).Result;

                    foreach (var tr in testRuns)
                    {
                        var testResults = testClient.GetTestResultsAsync(TfsClientFactory.TeamProject, tr.Id).Result;

                        var message = MessageFormatter.ComposeMessage(recipient, sender, tr, lastBuild,
                            testResults);
                        NotificationManager.SendEmail(message,
                            string.Format("Test Run \"{1}\" Completed, Build definition: \"{0}\"", buildDefinition.Name, tr.Name));
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
