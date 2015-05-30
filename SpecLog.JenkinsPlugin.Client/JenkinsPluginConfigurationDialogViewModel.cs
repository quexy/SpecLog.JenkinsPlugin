using System;
using System.Windows;
using System.Windows.Input;
using TechTalk.SpecLog.Application.Common;
using TechTalk.SpecLog.Application.Common.Dialogs;
using TechTalk.SpecLog.Application.Common.PluginsInfrastructure;

namespace SpecLog.JenkinsPlugin.Client
{
    class JenkinsPluginConfigurationDialogViewModel : PluginConfigurationDialogViewModel<JenkinsStatsPluginConfiguration>
    {
        private readonly IDialogService dialogService;
        public JenkinsPluginConfigurationDialogViewModel(IDialogService dialogService, string configuration, bool isEnabled)
            : base(configuration, isEnabled, JenkinsTestStatsPlugin.LearnMoreText, JenkinsTestStatsPlugin.LearnMoreUrl)
        {
            this.dialogService = dialogService;
            ChangeUserCommand = new DelegateCommand(ChangeUser);
            ClearUserCommand = new DelegateCommand(ClearUser);
        }

        public override string Caption { get { return "Jenkins Plugin Configuration"; } }

        public ICommand ChangeUserCommand { get; private set; }

        public ICommand ClearUserCommand { get; private set; }

        public Visibility ClearUserVisibility
        {
            get
            {
                return string.IsNullOrEmpty(configuration.Username)
                    ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string HostAddress
        {
            get { return configuration.JenkinsRoot; }
            set { configuration.JenkinsRoot = FixHostAddress(value); }
        }

        public string ProjectName
        {
            get { return configuration.ProjectName; }
            set { configuration.ProjectName = Trim(value); }
        }

        public int UpdateInterval
        {
            get { return configuration.UpdateIntervalMinutes; }
            set { configuration.UpdateIntervalMinutes = value; }
        }

        public string DisplayedUser
        {
            get
            {
                return string.IsNullOrEmpty(configuration.Username)
                    ? "<Anonymous>" : configuration.Username;
            }
        }

        void ChangeUser()
        {
            var result = dialogService.ShowDialog(new ChangeUserDialogViewModel(configuration.Username));

            var changeResult = result as ChangeUserDialogResult;
            if (changeResult == null) return;

            configuration.Username = changeResult.Username;
            sensitiveConfig["JenkinsPassword"] = changeResult.Password;
            base.NotifyPropertyChanged("DisplayedUser");
        }

        void ClearUser()
        {
            configuration.Username = null;
            sensitiveConfig.Remove("JenkinsPassword"); ;
            base.NotifyPropertyChanged("DisplayedUser");
        }

        public override bool CanSave()
        {
            if (!IsEnabled) return true;
            return base.CanSave()
                && !string.IsNullOrEmpty(HostAddress)
                && !string.IsNullOrEmpty(ProjectName)
                && UpdateInterval > 0
            ;
        }

        private static string FixHostAddress(string jenkinsRoot)
        {
            if (string.IsNullOrWhiteSpace(jenkinsRoot))
                return string.Empty;

            jenkinsRoot = jenkinsRoot.Trim();
            if (!jenkinsRoot.Contains("://"))
                jenkinsRoot = "http://" + jenkinsRoot;

            return jenkinsRoot;
        }
    }
}
