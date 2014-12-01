using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpecLog.JenkinsPlugin.Client
{
    /// <summary>
    /// Interaction logic for ChangeUserDialogView.xaml
    /// </summary>
    public partial class ChangeUserDialogView : UserControl
    {
        public ChangeUserDialogView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as ChangeUserDialogViewModel;
            if (dataContext == null) return;
            dataContext.Password = this.PasswordBox.Password;
        }

        private void ControlBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as ChangeUserDialogViewModel;
            if (dataContext == null) return;
            dataContext.ControlPassword = this.ControlBox.Password;
        }
    }
}
