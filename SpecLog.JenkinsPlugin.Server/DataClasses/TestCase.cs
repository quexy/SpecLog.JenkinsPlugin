using System;

namespace SpecLog.JenkinsPlugin.Server
{
    class TestCase
    {
        public string className { get; set; }
        public string classOnly { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public TestStatus status { get; set; }
        public DateTime executed { get; set; }
    }
}
