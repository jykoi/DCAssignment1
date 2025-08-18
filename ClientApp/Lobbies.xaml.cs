using LobbyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for Lobbies.xaml
    /// </summary>
    public partial class Lobbies : Window
    {
        private ClientServices clientServices;

        //Must pass ClientServices instance to preserve the existing connection
        public Lobbies(ClientServices clientServices)
        {
            InitializeComponent();
            this.clientServices = clientServices;
        }

        // create a new lobby
        private void newLobbyBtn_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby;
            string newLobbyName = newLobbyNameField.Text;
            if (clientServices.serverChannel.CreateLobby(newLobbyName, clientServices.Username, out lobby))
            {
                newLobbyNameField.Text = "Created successfully";
            }
            else
            {
                newLobbyNameField.Text = "could not create lobby";
            }
        }
    }
}
