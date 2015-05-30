﻿using System;
using System.Xml.Serialization;
using TechTalk.SpecLog.Common;

namespace SpecLog.JenkinsPlugin
{
    [Serializable]
    public class JenkinsStatsPluginConfiguration
    {
        public JenkinsStatsPluginConfiguration()
        {
            UpdateIntervalMinutes = 5;
        }

        public string JenkinsRoot { get; set; }

        public string ProjectName { get; set; }

        public string Username { get; set; }

        public int UpdateIntervalMinutes { get; set; }

        [XmlIgnore]
        public TimeSpan UpdateInterval
        {
            get { return TimeSpan.FromMinutes(UpdateIntervalMinutes); }
        }
    }
}
