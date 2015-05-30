using System;
using System.Xml.Serialization;
using TechTalk.SpecLog.Common;

namespace SpecLog.JenkinsPlugin
{
    public interface IJenkinsStatsPluginConfiguration : IPollingSynchronizerConfiguration
    {
        string JenkinsRoot { get; }
        string ProjectName { get; }

        string Username { get; }
        string Password { get; }
    }

    [Serializable]
    public class JenkinsStatsPluginConfiguration : IJenkinsStatsPluginConfiguration
    {
        public JenkinsStatsPluginConfiguration()
        {
            UpdateIntervalMinutes = 5;
        }

        public string JenkinsRoot { get; set; }

        public string ProjectName { get; set; }

        public string Username { get; set; }

        [XmlIgnore]
        public string Password { get; set; }

        public int UpdateIntervalMinutes { get; set; }

        [XmlIgnore]
        public TimeSpan UpdateInterval
        {
            get { return TimeSpan.FromMinutes(UpdateIntervalMinutes); }
        }
    }
}
