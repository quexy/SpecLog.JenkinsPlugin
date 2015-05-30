using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using TechTalk.SpecLog.Common;

namespace SpecLog.JenkinsPlugin.Server
{
    public interface IJenkinsStatsPluginContainerSetup : IPluginContainerSetup
    {
        IUnityContainer SetupContainer(JenkinsStatsPluginConfiguration configuration);
    }

    public class JenkinsStatsPluginContainerSetup : IJenkinsStatsPluginContainerSetup
    {
        private IUnityContainer container;
        public void Setup(IUnityContainer container)
        {
            this.container = container;
            container.RegisterInstance<IJenkinsStatsPluginContainerSetup>(this, new ContainerControlledLifetimeManager());
        }

        public IUnityContainer SetupContainer(JenkinsStatsPluginConfiguration configuration)
        {
            var subContainer = container.CreateChildContainer();
            subContainer.RegisterInstance<IJenkinsStatsPluginConfiguration>(configuration, new ContainerControlledLifetimeManager());
            return subContainer;
        }
    }
}
