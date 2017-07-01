namespace TestRunReportService
{
    using System.Configuration;

    class Program
    {
        static void Main(string[] args)
        {
            
            var definitionNames = ConfigurationManager.AppSettings["BuildDefinitions"].Split(';');

            foreach (var dn in definitionNames)
            {
                var lastBuild = BuildManager.GetLastBuildByDefinitionName(dn);

                if (lastBuild == null)
                {
                    continue;
                }
                
                EmailNotificationManager.SendEmail(string.Format("Test Run Completed, Build definition: {0}", dn), lastBuild);
            }
        }
    }
}
