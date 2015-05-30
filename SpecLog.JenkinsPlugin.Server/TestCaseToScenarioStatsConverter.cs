using System;
using System.ComponentModel;
using System.Linq;
using TechTalk.SpecLog.Commands.Synchronization.GherkinStats;

namespace SpecLog.JenkinsPlugin.Server
{
    static class TestCaseToScenarioStatsConverter
    {
        static readonly ByteConverter converter = new ByteConverter();
        public static IGherkinScenarioStatistics Convert(this TestCase testCase)
        {
            var result = new GherkinScenarioStatisticsData();
            result.ScenarioId = new Guid(GetGuidBytes(testCase));
            result.ScenarioTitle = testCase.name;
            result.FeatureTitle = testCase.classOnly;
            result.LastRunDate = testCase.executed;
            result.LastResult = AsTestResult(testCase.status);
            result.HistoricalResult = AsHistoricResult(testCase.status);
            return result;
        }

        private static byte[] GetGuidBytes(TestCase testCase)
        {
            var classPrefix = testCase.className == null || !testCase.className.Contains(".")
                ? "Gherkin" : testCase.className.Substring(0, testCase.className.LastIndexOf('.'));
            return Enumerable.Empty<byte>()
                .Concat(BitConverter.GetBytes("Jenkins".GetHashCode()))
                .Concat(BitConverter.GetBytes(classPrefix.GetHashCode()))
                .Concat(BitConverter.GetBytes(testCase.classOnly.GetHashCode()))
                .Concat(BitConverter.GetBytes(testCase.name.GetHashCode()))
                .ToArray();
        }

        private static TestResult AsTestResult(TestStatus testStatus)
        {
            switch (testStatus)
            {
                case TestStatus.FAILED:
                    return TestResult.Failed;
                case TestStatus.FIXED:
                    return TestResult.Passed;
                case TestStatus.PASSED:
                    return TestResult.Passed;
                case TestStatus.REGRESSION:
                    return TestResult.Failed;
                case TestStatus.SKIPPED:
                    return TestResult.Ignored;
                default:
                    return TestResult.Unknown;
            }
        }

        private static HistoricalTestResult AsHistoricResult(TestStatus testStatus)
        {
            switch (testStatus)
            {
                case TestStatus.FAILED:
                    return HistoricalTestResult.StableFailure;
                case TestStatus.FIXED:
                    return HistoricalTestResult.Recovering;
                case TestStatus.PASSED:
                    return HistoricalTestResult.StableSuccess;
                case TestStatus.REGRESSION:
                    return HistoricalTestResult.Regression;
                case TestStatus.SKIPPED:
                    return HistoricalTestResult.Ignored;
                default:
                    return HistoricalTestResult.Unknown;
            }
        }
    }
}
