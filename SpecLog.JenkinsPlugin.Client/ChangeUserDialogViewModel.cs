using System;
using TechTalk.SpecLog.Application.Common;
using TechTalk.SpecLog.Application.Common.Dialogs;
using TechTalk.SpecLog.Common;

namespace SpecLog.JenkinsPlugin.Client
{
    class ChangeUserDialogViewModel : IDialogViewModel
    {
        public ChangeUserDialogViewModel(string username)
        {
            this.username = username;
            CancelCommand = new DelegateCommand(Cancel, () => true);
            AcceptCommand = new DelegateCommand(Accept, CanAccept);
        }

        public string Caption { get { return "Change user"; } }

        public event EventHandler<TechTalk.SpecLog.Common.EventArgs<bool?>> Close = delegate { };

        public DelegateCommand CancelCommand { get; private set; }

        public DelegateCommand AcceptCommand { get; private set; }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = (value ?? "").Trim();
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        private string controlPassword;
        public string ControlPassword
        {
            get { return controlPassword; }
            set
            {
                controlPassword = value;
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public void Cancel()
        {
            Close(this, new EventArgs<bool?>(false));
        }

        public void Accept()
        {
            Close(this, new EventArgs<bool?>(true));
        }

        public bool CanAccept()
        {
            return !string.IsNullOrEmpty(Username)
                && !string.IsNullOrEmpty(Password)
                && Password == ControlPassword
            ;
        }

        public IDialogResult GetDialogResultData()
        {
            return new ChangeUserDialogResult(username, password);
        }
    }
}
