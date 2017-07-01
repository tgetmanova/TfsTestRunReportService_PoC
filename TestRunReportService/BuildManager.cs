
namespace TestRunReportService
{
    using System.Linq;

    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;

    class BuildManager
    {
        internal static IBuildDefinition GetLastBuildByDefinitionName(string definitionName)
        {
            var tfsBuildServer = TfsManager.Tfs.GetService<IBuildServer>();

            var allBuildDefinitions = tfsBuildServer.QueryBuildDefinitions(TfsManager.TeamProject);
            return allBuildDefinitions.FirstOrDefault(bd => bd.Name == definitionName);
        }

        internal static ITestRun[] GetTestRuns(IBuildDefinition buildDefinition)
        {
            return TfsManager.TestManagementService.GetTeamProject(TfsManager.TeamProject).TestRuns.ByBuild(buildDefinition.LastBuildUri).ToArray();
        }
    }
}
