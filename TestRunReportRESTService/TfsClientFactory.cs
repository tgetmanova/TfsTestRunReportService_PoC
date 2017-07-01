

namespace TestRunReportRESTService
{
    using System;
    using System.Configuration;
    using System.Threading;

    using Microsoft.TeamFoundation.Core.WebApi;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.TeamFoundation.TestManagement.WebApi;

    internal class TfsClientFactory
    {
        internal static string TeamProject = ConfigurationManager.AppSettings["TeamProject"];

        internal TfsTeamProjectCollection Tfs
        {
            get
            {
                return new TfsTeamProjectCollection(new Uri(ConfigurationManager.AppSettings["tfsUri"]));
            }
        }

        internal ProjectHttpClient GetProjectClient()
        {
            return Tfs.GetClient<ProjectHttpClient>();
        }

        internal BuildHttpClient GetBuildClient()
        {
            return Tfs.GetClient<BuildHttpClient>();
        }

        internal TestManagementHttpClient GetTestManagementClient()
        {
            return Tfs.GetClient<TestManagementHttpClient>();
        }
    }
}
