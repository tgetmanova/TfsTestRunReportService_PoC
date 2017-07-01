

namespace TestRunReportService
{
    using System;
    using System.Configuration;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;

    class TfsManager
    {

        internal static string TeamProject = ConfigurationManager.AppSettings["TeamProject"];

        internal static TfsTeamProjectCollection Tfs
        {
            get
            {
                var tfsUri = new Uri(ConfigurationManager.AppSettings["tfsUri"]);

                return new TfsTeamProjectCollection(tfsUri);
            }
        }

        internal static ITestManagementService TestManagementService
        {
            get
            {
                return Tfs.GetService<ITestManagementService>();
            }
        }
    }
}
