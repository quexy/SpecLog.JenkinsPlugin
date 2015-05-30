using System;
using System.Collections.Generic;
using TechTalk.SpecLog.Application.Common;
using TechTalk.SpecLog.Application.Common.Dialogs;
using TechTalk.SpecLog.Application.Common.PluginsInfrastructure;
using TechTalk.SpecLog.Common;
using TechTalk.SpecLog.Entities;
using TechTalk.SpecLog.Logging;

namespace SpecLog.JenkinsPlugin.Client
{
    [PluginAttribute(PluginName)]
    public class JenkinsTestStatsPlugin : IClientPlugin, IConfigurationSavedCallback
    {
        public const string PluginName = "SpecLog.JenkinsPlugin";
        public const string LearnMoreText = "Learn more...";
        public const string LearnMoreUrl = "https://github.com/quexy/SpecLog.JenkinsPlugin/wiki";

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

        public string Description { get { return "Server side plugin to store Jenkins test statistics for linked Gherkin files"; } }

        public string LearnMoreLink { get { return LearnMoreUrl; } }

        public string LearnMoreLinkText { get { return LearnMoreText; } }

        public string WorkItemProviderName { get { throw new NotSupportedException(); } }

        public bool IsConfigurable(RepositoryMode repositoryMode)
        {
            return repositoryMode == RepositoryMode.ClientServer;
        }

        public bool IsGherkinLinkProvider(RepositoryMode repositoryMode) { return false; }

        public bool IsWorkItemSynchronizer(RepositoryMode repositorMode) { return false; }

        public bool IsGherkinStatsProvider(RepositoryMode repositoryMode) { return false; }

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
            throw new NotSupportedException();
        }

        public IEnumerable<PluginCommand> GetSupportedCommands(RepositoryMode repositoryMode)
        {
            return new[] { new PluginCommand { CommandVerb = "RefreshNow", DisplayText = "Refresh all Statistics" } };
        }

        bool previouslyEnabled = false;
        public void OnConfigurationSaved(RepositoryMode repositoryMode, PluginConfigurationDialogResult configuration, IDialogService dialogService, ICommandExecutionService commandExecutionService)
        {
            if (!previouslyEnabled && configuration.IsEnabled)
                commandExecutionService.IssueCommand("RefreshNow");
        }
    }
}
