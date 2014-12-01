using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using TechTalk.SpecLog.Application.Common;
using TechTalk.SpecLog.Application.Common.Dialogs;
using TechTalk.SpecLog.Application.Common.PluginsInfrastructure;
using TechTalk.SpecLog.Common;
using TechTalk.SpecLog.Entities;
using TechTalk.SpecLog.Logging;

namespace SpecLog.JenkinsPlugin.Client
{
    [PluginAttribute(PluginName)]
    public class JenkinsTestStatsPlugin : IClientPlugin, ICommandableStatsPlugin, IConfigurationSavedCallback
    {
        public const string PluginName = "SpecLog.JenkinsPlugin";

        private readonly ILogger logger;
        private readonly ITimeService timeService;
        private readonly IDialogService dialogService;
        public JenkinsTestStatsPlugin(ILogger logger, ITimeService timeService, IDialogService dialogService)
        {
            this.logger = logger;
            this.timeService = timeService;
            this.dialogService = dialogService;
        }

        public string Name { get { return PluginName; } }

        public string DisplayName { get { return "Jenkins Test Statistics"; } }

        public string Description { get { return "Client side plugin to store and display Jenkins test statistics for linked Gherkin files"; } }

        public string LearnMoreLink { get { return "http://github.com/quexy/SpecLog.JenkinsTestStats/wiki/"; } }

        public string LearnMoreLinkText { get { return "Learn more..."; } }

        public bool IsConfigurable(RepositoryMode repositoryMode) { return true; }

        public bool IsGherkinLinkProvider(RepositoryMode repositoryMode) { return false; }

        public bool IsWorkItemSynchronizer(RepositoryMode repositorMode) { return false; }

        public bool IsGherkinStatsProvider(RepositoryMode repositoryMode) { return true; }

        public IDialogViewModel GetConfigDialog(RepositoryMode repositoryMode, bool isEnabled, string configuration)
        {
            previouslyEnabled = isEnabled;
            return new JenkinsPluginConfigurationDialogViewModel(dialogService, configuration, isEnabled);
        }

        public IGherkinLinkProviderViewModel GetGherkinLinkViewModel(RepositoryMode repositoryMode)
        {
            throw new NotSupportedException();
        }

        public IGherkinStatsProvider CreateStatsProvider(RepositoryMode repositoryMode, string configuration, IGherkinStatsRepository statsRepository)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(configuration)))
            {
                var serializer = new XmlSerializer(typeof(JenkinsStatsPluginConfiguration));
                var config = (JenkinsStatsPluginConfiguration)serializer.Deserialize(stream);
                return new JenkinsTestStatsProvider(logger, timeService, statsRepository, config);
            }
        }

        public IEnumerable<PluginCommand> GetSupportedCommands(RepositoryMode repositoryMode)
        {
            return new[] { new PluginCommand { CommandVerb = "RefreshNow", DisplayText = "Refresh all Statistics" } };
        }

        public void PerformCommand(string commandVerb, IGherkinStatsProvider statsProvider)
        {
            var jenkinsTestStatsProvider = statsProvider as JenkinsTestStatsProvider;
            if (jenkinsTestStatsProvider == null) throw new InvalidOperationException("Invalid stats provider received");

            if (commandVerb != "RefreshNow") throw new InvalidOperationException("Unrecognised command verb: " + commandVerb);

            new Thread(() => jenkinsTestStatsProvider.Updater.UpdateSince(DateTime.MinValue)) { IsBackground = true }.Start();
        }

        bool previouslyEnabled = false;
        public void OnConfigurationSaved(RepositoryMode repositoryMode, PluginConfigurationDialogResult configuration, IDialogService dialogService, ICommandExecutionService commandExecutionService)
        {
            if (!previouslyEnabled && configuration.IsEnabled)
                commandExecutionService.IssueCommand("RefreshNow");
        }
    }
}
