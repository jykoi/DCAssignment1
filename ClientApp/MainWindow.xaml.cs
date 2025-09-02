using ServerDLL;
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
    public partial class MainWindow : Window
    {

        private ClientServices _client;
        public MainWindow()
        {
            InitializeComponent();
            progBar.Visibility = Visibility.Hidden;
            

        }

        private async void loginBtn_Click(object sender, RoutedEventArgs e)
        {

            //start the connection process asynchronously using an instance of ClientServices

            DisableGui();
            _client = new ClientServices(usernameField.Text.Trim());
            Task connect = new Task(_client.Connect);
            connect.Start();
            await connect;

            //check that the username is valid & unique
            if (!_client.serverChannel.AddUser(_client.Username))
            {
                //if not, end the connection
                _client.Disconnect();
                usernameField.Text = "pick a valid and unique username";
                EnableGui();
            }
            else
            {
                //if valid, continue to the lobbies window
                usernameField.Text = "Username accepted";
                EnableGui();


                Lobbies lobbiesWin = new Lobbies(_client);
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
