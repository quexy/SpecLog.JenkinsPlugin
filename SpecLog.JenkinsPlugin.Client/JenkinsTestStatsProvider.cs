using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecLog.Application.Common.PluginsInfrastructure;
using TechTalk.SpecLog.Commands.Synchronization.GherkinStats;
using TechTalk.SpecLog.Common;
using TechTalk.SpecLog.Entities;
using TechTalk.SpecLog.Logging;

namespace SpecLog.JenkinsPlugin.Client
{
    class JenkinsTestStatsProvider : IGherkinStatsProvider
    {
        private readonly IGherkinStatsRepository statsRepository;
        private readonly JenkinsStatsPollingUpdater statsUpdater;
        public JenkinsTestStatsProvider(ILogger logger, ITimeService timeService, IGherkinStatsRepository statsRepository, IJenkinsStatsPluginConfiguration configuration)
        {
            this.statsRepository = statsRepository;
            statsRepository.StatisticsUpdated += OnStatisticsRepositoryUpdated;
            statsRepository.RequirementStatisticsChanged += OnRequirementStatisticsChanged;
            this.statsUpdater = new JenkinsStatsPollingUpdater(logger, timeService, statsRepository, configuration);
            this.statsUpdater.Start();
        }

        public JenkinsStatsPollingUpdater Updater { get { return statsUpdater; } }

        public AggregateTestStatus GetFeatureCollectionStatus(IEnumerable<string> featureTitles)
        {
            return featureTitles.Select(GetFeatureStatus).Aggregate(AggregateTestStatus.NoInformation, AggregateTestStatusMethods.CombineStatus);
        }

        public AggregateTestStatus GetFeatureStatus(string featureTitle)
        {
            return statsRepository.GetScenarioHistory(featureTitle.AsFeatureId(), string.Empty, true)
                .Where(s => s.LastResult != TestResult.Unknown)
                .Select(s => AggregateTestStatusMethods.ConvertStatus(s.LastResult))
                .Aggregate(AggregateTestStatus.NoInformation, AggregateTestStatusMethods.CombineStatus);
        }

        public HistoricalTestResult GetScenarioStatus(string featureTitle, string scenarioTitle, bool scenarioOutline)
        {
            return (HistoricalTestResult)statsRepository
                .GetScenarioHistory(featureTitle.AsFeatureId(), scenarioTitle.AsScenarioId(), scenarioOutline)
                .Select(s => (int)s.HistoricalResult).Aggregate(0, (a, b) => Math.Max(a, b));
        }

        public event EventHandler GherkinStatisticsChanged = delegate { };
        private void OnStatisticsRepositoryUpdated(object sender, EventArgs args)
        {
            GherkinStatisticsChanged(this, args);
        }

        public event EventHandler<TechTalk.SpecLog.Common.EventArgs<Guid[]>> RequirementStatisticsChanged = delegate { };
        private void OnRequirementStatisticsChanged(object sender, TechTalk.SpecLog.Common.EventArgs<Guid[]> args)
        {
            RequirementStatisticsChanged(this, args);
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                statsRepository.StatisticsUpdated -= OnStatisticsRepositoryUpdated;
                statsRepository.RequirementStatisticsChanged -= OnRequirementStatisticsChanged;
                statsUpdater.Stop();
            }
            GC.SuppressFinalize(this);
        }
    }
}
