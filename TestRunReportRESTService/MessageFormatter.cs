using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.UI.WebControls;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace TestRunReportRESTService
{
    class MessageFormatter
    {
        internal static MailMessage ComposeMessage(string to, string from, TestRun testRun, Build build, List<TestCaseResult> testResults)
        {
            var body = new StringBuilder();
            body.Append(ConfigurationManager.AppSettings["HtmlTemplateBuild"]);
            body.Append(ConfigurationManager.AppSettings["HtmlTemplateAttachment"]);

            var passed = testResults.Where(tr => tr.Outcome == TestOutcome.Passed.ToString()).ToArray();
            var error = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Error.ToString()).ToArray());
            var warning = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Warning.ToString()).ToArray());
            var aborted = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Aborted.ToString()).ToArray());
            var blocked = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Blocked.ToString()).ToArray());
            var inconclusive = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Inconclusive.ToString()).ToArray());
            var timeout = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Timeout.ToString()).ToArray());
            var notExecuted = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.NotExecuted.ToString()).ToArray());
            var notApplicable = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.NotApplicable.ToString()).ToArray());
            var paused = GetTestResultsPlaceholders(testResults.Where(tr => tr.Outcome == TestOutcome.Paused.ToString()).ToArray());

            if (passed.Any())
            {
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePassedTestsTitle"]);
                body.Append(ConfigurationManager.AppSettings["HtmlTemplatePassedTests"]);
            }

            var failedTests = testResults.Where(tr => tr.Outcome == TestOutcome.Failed.ToString()).ToArray();
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
                {"{buildDefinition}", build.Definition.Name},
                {"{lastBuildName}", build.Id.ToString()},
                {"{lastBuildUri}", build.Url},
                {"{testRunTitle}", string.Format("<b>{0}</b>", testRun.Name)},
                {"{RequestedBy}", build.RequestedBy.DisplayName},
                {"{BuildConfiguration}", string.Empty}, // TODO Research
                {"{DateStarted}", build.StartTime.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)},
                {"{DateCompleted}", build.FinishTime.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)},
                {"{totalTests}", testRun.TotalTests.ToString()},
                {"{passedTests}", passed.Length.ToString()},
                {
                    "{preconditionFailedTests}",
                    GetTestResultsPlaceholders(preconditionsFailed)
                },
                {"{failedTests}", GetTestResultsPlaceholders(assertFailed)},
                {
                    "{unexpectedFailedTests}",
                    GetTestResultsPlaceholders(othersFailed)
                },
                {"{errorTests}", error},
                {"{warningTests}", warning},
                {"{abortedTests}", aborted},
                {"{blockTests}", blocked},
                {"{inconclusiveTests}", inconclusive},
                {"{notApplicableTests}", notApplicable},
                {"{notExecutedTests}", notExecuted},
                {"{timeoutTests}", timeout},
                {"{pausedTests}", paused},
                {"{attachUri}", testRun.WebAccessUrl}
            };

            var md = new MailDefinition { From = from, IsBodyHtml = true };
            return md.CreateMailMessage(to, replacements, body.ToString(), new System.Web.UI.Control());
        }

        private static string GetTestResultsPlaceholders(TestCaseResult[] tests)
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
