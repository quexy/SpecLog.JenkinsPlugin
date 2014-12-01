using System;
using TechTalk.SpecLog.Application.Common.Dialogs;

namespace SpecLog.JenkinsPlugin.Client
{
    class ChangeUserDialogResult : IDialogResult
    {
        public ChangeUserDialogResult(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
