using System;
using System.Collections.Generic;

namespace SpecLog.JenkinsPlugin.Client
{
    class TestSuite
    {
        public string name { get; set; }
        public List<TestCase> cases { get; set; }
    }
}
