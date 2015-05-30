using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using TechTalk.SpecLog.Common;
using TechTalk.SpecLog.Entities;
using TechTalk.SpecLog.Server.Services.PluginInfrastructure;

namespace SpecLog.JenkinsPlugin.Server
{
    [PluginAttribute(PluginName)]
    public class JenkinsTestStatsPlugin : ServerPlugin
    {
        public const string PluginName = "SpecLog.JenkinsPlugin";

        public string Name { get { return PluginName; } }

        public override IEnumerable<IPeriodicActivity> ActiveSynchronizers
        {
            get { return new[] { synchronizer }; }
        }

        private readonly IJenkinsStatsPluginContainerSetup containerSetup;
        public JenkinsTestStatsPlugin(IJenkinsStatsPluginContainerSetup containerSetup)
        {
            this.containerSetup = containerSetup;
        }

        private IUnityContainer container;
        private ISynchronizer synchronizer;
        public override void OnStart()
        {
            var configuration = base.GetConfiguration<JenkinsStatsPluginConfiguration>();
            var secureConfig = base.GetSecuredConfiguration();
            if (!string.IsNullOrEmpty(configuration.Username))
                configuration.Password = secureConfig["JenkinsPassword"];
            
            container = containerSetup.SetupContainer(configuration);
            synchronizer = container.Resolve<JenkinsStatsPollingUpdater>();
            synchronizer.Start();

            Log(TraceEventType.Information, "The plugin '{0}' started successfully", PluginName);
        }

        public override void OnStop()
        {
            if (synchronizer != null)
                synchronizer.Stop();
            synchronizer = null;

            if (container != null)
                container.Dispose();
            container = null;

            Log(TraceEventType.Information, "The plugin '{0}' stopped successfully", PluginName);
        }

        public override void PerformCommand(string commandVerb, string issuingUser)
        {
            using (BeginEndDisposable.Create(synchronizer.Stop, synchronizer.Start))
            {
                if (commandVerb == "RefreshNow") synchronizer.TriggerAction();
                Log(TraceEventType.Information, "Perform command '{0}' finished", commandVerb);
            }
        }

        public override void AfterApplyCommand(RepositoryInfo repository, TechTalk.SpecLog.Commands.Command command) { /* do nothing */ }

        public override void AfterUndoCommand(RepositoryInfo repository, TechTalk.SpecLog.Commands.Command command) { /* do nothing */ }

        public override void BeforeApplyCommand(RepositoryInfo repository, TechTalk.SpecLog.Commands.Command command) { /* do nothing */ }

        public override void BeforeUndoCommand(RepositoryInfo repository, TechTalk.SpecLog.Commands.Command command) { /* do nothing */ }
    }
}
