using System;

namespace SpecLog.JenkinsPlugin.Client
{
    class Build
    {
        private static readonly DateTime epoch = new DateTime(1970, 01, 01, 00, 00, 00, DateTimeKind.Utc);

        public int number { get; set; }
        public Uri url { get; set; }
        public DateTime buildTime { get; set; }
        public long timestamp
        {
            get { return (long)(buildTime - epoch).TotalMilliseconds; }
            set { buildTime = epoch.AddMilliseconds(value); }
        }
    }
}
