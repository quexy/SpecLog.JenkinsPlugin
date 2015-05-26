using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using TechTalk.SpecLog.Common;
using TechTalk.SpecLog.Entities;
using TechTalk.SpecLog.Logging;

namespace SpecLog.JenkinsPlugin.Client
{
    class JenkinsStatsPollingUpdater : PollingSynchronizer
    {
        private readonly ILogger logger;
        private readonly IGherkinStatsRepository statsRepository;
        private readonly IJenkinsStatsPluginConfiguration pluginConfiguration;
        public JenkinsStatsPollingUpdater
        (
            ILogger logger, ITimeService timeService,
            IGherkinStatsRepository statsRepository,
            IJenkinsStatsPluginConfiguration configuration
        )
            : base(timeService, configuration)
        {
            this.logger = logger;
            this.statsRepository = statsRepository;
            this.pluginConfiguration = configuration;
        }

        public override bool TriggerUpdate()
        {
            return UpdateSince(statsRepository.LastRetrievedAt);
        }

        public bool UpdateSince(DateTime? lastDate)
        {
            try
            {
                var queryTime = timeService.CurrentTime;
                var testStats = GetTestResults(pluginConfiguration, lastDate)
                    .Select(TestCaseToScenarioStatsConverter.Convert).ToArray();
                statsRepository.UpdateStatistics(testStats, queryTime);
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(TraceEventType.Error, "Could not update test run statistics: {0}", ex);
                return false;
            }
        }

        public static IEnumerable<TestCase> GetTestResults(IJenkinsStatsPluginConfiguration config, DateTime? since)
        {
            var password = CryptoService.Decrypt(config.Password);
            var auth = string.IsNullOrEmpty(config.Username) ? null : new NetworkCredential(config.Username, password);
            var projectRoot = new Uri(config.JenkinsRoot.TrimEnd('/') + "/job/" + config.ProjectName.Trim('/') + "/");
            return GetObject<BuildList>(projectRoot, "api/json", auth).builds.OrderByDescending(b => b.number)
                .Select(b => GetObject<Build>(b.url, "api/json", auth)).TakeWhile(b => since == null || b.buildTime > since)
                .Select(b => new { Build = b, TestReport = GetObject<TestReport>(b.url, "testReport/api/json?depth=2", auth) })
                .SelectMany(e => e.TestReport.suites.SelectMany(s => s.cases).Update(t => t.executed = e.Build.buildTime))
                .Update(t => t.classOnly = t.className.Substring(t.className.LastIndexOf('.') + 1))
                .OrderBy(t => t.executed).ThenBy(t => t.className).ThenBy(t => t.name).ToArray();
        }

        private static TObject GetObject<TObject>(Uri root, string path, ICredentials auth)
        {
            var client = new System.Net.WebClient();
            client.BaseAddress = root.ToString();
            if (auth != null) client.Credentials = auth;
            var data = client.DownloadString(path);
            return JsonConvert.DeserializeObject<TObject>(data);
        }
    }

    static class Extensions
    {
        public static IEnumerable<T> Update<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
                yield return item;
            }
        }
    }
}
