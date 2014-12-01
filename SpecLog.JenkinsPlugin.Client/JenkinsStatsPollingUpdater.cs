using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using TechTalk.SpecLog.Common;
using TechTalk.SpecLog.Entities;
using TechTalk.SpecLog.Logging;
//for UpdateStatisticsHack()
using System.Reflection;
using TechTalk.SpecLog.DataAccess.Boundaries;
using TechTalk.SpecLog.DataAccess.Repositories;
using StatsEntity = TechTalk.SpecLog.Entities.GherkinScenarioStatistics;
using StatsDict = System.Collections.Generic.Dictionary<System.Guid, TechTalk.SpecLog.Entities.GherkinScenarioStatistics>;

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
            return UpdateSince(statsRepository.LastStatsDate);
        }

        public bool UpdateSince(DateTime? lastDate)
        {
            try
            {
                var testStats = GetTestResults(pluginConfiguration, lastDate)
                    .Select(TestCaseToScenarioStatsConverter.Convert).ToArray();
                //HACK: error in 'statsRepository.UpdateStatistics(testStats)' implementation
                UpdateStatisticsHack(statsRepository, testStats);
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(TraceEventType.Error, "Could not update test run statistics: {0}", ex);
                return false;
            }
        }

        private static void UpdateStatisticsHack(IGherkinStatsRepository statsRepo, IGherkinScenarioStatistics[] testStats)
        {
            if (testStats.Length == 0) return;

            var fieldFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;
            var methodFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            var mainBoundary = (IBoundary)typeof(GherkinStatsRepository).GetField("mainBoundary", fieldFlags).GetValue(statsRepo);
            var boundaryFactory = (IBoundaryFactory)typeof(GherkinStatsRepository).GetField("boundaryFactory", fieldFlags).GetValue(statsRepo);
            var statsCache = (StatsDict)typeof(GherkinStatsRepository).GetField("statsCollection", fieldFlags).GetValue(statsRepo);
            var repoStore = (IRepositoryStorage)typeof(GherkinStatsRepository).GetField("repositoryStorage", fieldFlags).GetValue(statsRepo);

            var statsMethod = typeof(GherkinStatsRepository).GetMethod("GetAffectedRequierements", methodFlags);
            Func<object, Guid[]> statsIds = group => (Guid[])statsMethod.Invoke(statsRepo, new object[] { group });
            var changedIds = testStats.GroupBy(s => s.FeatureTitle).SelectMany(statsIds).Distinct().ToArray();

            using (var boundary = boundaryFactory.CreateShortRunning(mainBoundary))
            {
                foreach (var stat in testStats)
                {
                    StatsEntity statsEntity = null;
                    if (!statsCache.TryGetValue(stat.ScenarioId, out statsEntity))
                    {
                        statsEntity = repoStore.Create<StatsEntity>(boundary);
                        boundary.Complete();
                    }

                    statsEntity = boundary.AttachObject(statsEntity);
                    statsEntity.PluginName = JenkinsTestStatsPlugin.PluginName;
                    statsEntity.ScenarioId = stat.ScenarioId;
                    statsEntity.ScenarioTitle = stat.ScenarioTitle;
                    statsEntity.FeatureTitle = stat.FeatureTitle;
                    statsEntity.LastRunDate = stat.LastRunDate;
                    statsEntity.LastResult = stat.LastResult;
                    statsEntity.HistoricalResult = stat.HistoricalResult;
                    boundary.Complete();

                    statsCache[stat.ScenarioId] = mainBoundary.AttachObject(statsEntity);
                }
            }

            typeof(GherkinStatsRepository).GetMethod("OnStatisticsUpdated", methodFlags)
                .Invoke(statsRepo, new object[] { EventArgs.Empty });

            typeof(GherkinStatsRepository).GetMethod("OnRequirementStatisticsChanged", methodFlags)
                .Invoke(statsRepo, new object[] { changedIds });
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
