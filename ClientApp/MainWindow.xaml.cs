using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ClientServices clientServices;
        public MainWindow()
        {
            InitializeComponent();
            progBar.Visibility = Visibility.Hidden;
            

        }

        private async void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableGui();
            clientServices = new ClientServices(usernameField.Text);
            Task connect = new Task(clientServices.Connect);
            connect.Start();
            await connect;
            if (!clientServices.serverChannel.CheckUsername(clientServices.Username))
            {
                clientServices.Disconnect();
                usernameField.Text = "pick valid and unique username";
                EnableGui();
            }
            else
            {
                usernameField.Text = "Username accepted";
                EnableGui();

                Lobbies lobbiesWin = new Lobbies(clientServices);
                lobbiesWin.Show();
                this.Close();

            }
        }

        private void DisableGui()
        {
            usernameField.IsEnabled = false;
            loginBtn.IsEnabled = false;
            progBar.Visibility = Visibility.Visible;
            progBar.IsIndeterminate = true;
        }

        private void EnableGui()
        {
            usernameField.IsEnabled = true;
            loginBtn.IsEnabled = true;
            progBar.Visibility = Visibility.Hidden;
            progBar.IsIndeterminate = false;
        }
    }
}
