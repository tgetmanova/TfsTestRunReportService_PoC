using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;


namespace TestRunReportService
{
    using System.Configuration;
    using System.Net.Mail;
    using System.Collections.Specialized;
    using System.Globalization;

    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;

    class MessageManager
    {
        internal static MailMessage GetFormattedMessage(string to, string from, IBuildDefinition buildDefinition, ITestRun testRun)
        {
            var body = new StringBuilder();
            body.Append(ConfigurationManager.AppSettings["HtmlTemplateBuild"]);

            var passed = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Passed).ToArray());
            var error = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Error).ToArray());
            var warning = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Warning).ToArray());
            var aborted = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Aborted).ToArray());
            var blocked = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Blocked).ToArray());
            var inconclusive = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Inconclusive).ToArray());
            var timeout = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Timeout).ToArray());
            var notExecuted = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.NotExecuted).ToArray());
            var notApplicable = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.NotApplicable).ToArray());
            var paused = GetTestResultsPlaceholders(testRun.QueryResultsByOutcome(TestOutcome.Paused).ToArray());

            if (!string.IsNullOrEmpty(passed))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePassedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePassedTests"]);
            }

            var failedTests = testRun.QueryResultsByOutcome(TestOutcome.Failed);
            var preconditionsFailed =
                failedTests.Where(t => t.ErrorMessage.Contains("Assert.Precondition")).ToArray();
            var assertFailed =
                failedTests.Where(t => t.ErrorMessage.Contains("Assert")).Except(preconditionsFailed).ToArray();
            var othersFailed = failedTests.Except(assertFailed).Except(preconditionsFailed).ToArray();

            if (preconditionsFailed.Any())
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePreconditionFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePreconditionFailedTests"]);
            }

            if (assertFailed.Any())
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateFailedTests"]);
            }

            if (othersFailed.Any())
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateUnexpectedFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateUnexpectedFailedTests"]);
            }

            if (!string.IsNullOrEmpty(error))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateErrorFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateErrorFailedTests"]);
            }

            if (!string.IsNullOrEmpty(warning))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateWarningFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateWarningFailedTests"]);
            }

            if (!string.IsNullOrEmpty(aborted))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateAbortedFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateAbortedFailedTests"]);
            }

            if (!string.IsNullOrEmpty(blocked))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateBlockFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateBlockFailedTests"]);
            }

            if (!string.IsNullOrEmpty(inconclusive))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateInconclusiveFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateInconclusiveFailedTests"]);
            }

            if (!string.IsNullOrEmpty(timeout))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateTimeoutFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateTimeoutFailedTests"]);
            }

            if (!string.IsNullOrEmpty(notApplicable))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateNotApplicableFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateNotApplicableFailedTests"]);
            }

            if (!string.IsNullOrEmpty(notExecuted))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateNotExecutedFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateNotExecutedFailedTests"]);
            }

            if (!string.IsNullOrEmpty(paused))
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePausedFailedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePausedFailedTests"]);
            }

            var replacements = new ListDictionary
                                       {
                                           { "{buildDefinition}", buildDefinition.Name },
                                           { "{lastBuildUri}", buildDefinition.LastBuildUri.ToString() },
                                           { "{testRunTitle}", string.Format("<b>{0}</b>", testRun.Title) },
                                           { "{TestRunId}", testRun.Id.ToString() },
                                           { "{TestPlanId}", testRun.TestPlanId.ToString() },
                                           { "{BuildPlatform}", testRun.BuildPlatform },
                                           { "{BuildConfigurationId}", testRun.BuildConfigurationId.ToString() },
                                           { "{DateStarted}", testRun.DateStarted.ToString(CultureInfo.InvariantCulture) },
                                           { "{DateCompleted}", testRun.DateCompleted.ToString(CultureInfo.InvariantCulture) },
                                           { "{totalTests}", testRun.Statistics.TotalTests.ToString() },
                                           { "{passedTests}", passed },
                                           {
                                               "{preconditionFailedTests}",
                                               GetTestResultsPlaceholders(preconditionsFailed)
                                           },
                                           { "{failedTests}", GetTestResultsPlaceholders(assertFailed) },
                                           {
                                               "{unexpectedFailedTests}",
                                               GetTestResultsPlaceholders(othersFailed)
                                           },
                                           { "{errorTests}", error },
                                           { "{warningTests}", warning },
                                           { "{abortedTests}", aborted },
                                           { "{blockTests}", blocked },
                                           { "{inconclusiveTests}", inconclusive },
                                           { "{notApplicableTests}", notApplicable },
                                           { "{notExecutedTests}", notExecuted },
                                           { "{timeoutTests}", timeout },
                                           { "{pausedTests}", paused }
                                       };

            var attachments = testRun.Attachments;
            if (attachments.Any())
            {
                replacements.Add("{attachUri}", attachments.First().Uri.ToString());
                body.Append(ConfigurationManager.AppSettings["HtmlTemplateAttachment"]);
            }

            var md = new MailDefinition { From = from, IsBodyHtml = true };
            return md.CreateMailMessage(to, replacements, body.ToString(), new System.Web.UI.Control());
        }

        private static string GetTestResultsPlaceholders(ITestCaseResult[] tests)
        {
            var count = 0;
            var str = new StringBuilder();

            if (!tests.Any())
            {
                return string.Empty;
            }

            foreach (var t in tests)
            {
                count++;
                str.Append(string.Format("{2}) {0}: <b>{1}</b>. <br/>", t.Outcome, t.TestCaseTitle, count));
                if (!string.IsNullOrEmpty(t.ErrorMessage))
                {
                    str.Append(string.Format("{0} <br/>", t.ErrorMessage));
                }
            }

            return str.ToString();
        }
    }
}
