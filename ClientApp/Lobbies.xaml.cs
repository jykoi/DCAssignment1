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
        private readonly ClientServices _client;

        //Must pass ClientServices instance to preserve the existing connection
        public Lobbies(ClientServices clientServices)
        {
            InitializeComponent();
            _client = clientServices;

            //load when window is ready
            Loaded += (_, __) => LoadLobbies();
        }

        private void LoadLobbies()
        {
            try
            {
                var names = _client.serverChannel.ListLobbies() ?? Array.Empty<string>();
                LobbiesList.ItemsSource = names;
                Status.Text = $"Loaded {names.Length} lobby(ies).";
            }
            catch (Exception ex)
            {
                Status.Text = "Failed in loading lobbies: " + ex.Message;
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadLobbies();
        }

        private void newLobbyBtn_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby;
            string newLobbyName = newLobbyField.Text;
            if (_client.serverChannel.CreateLobby(newLobbyName, _client.Username, out lobby))
            {
                newLobbyField.Text = "Created successfully";
            }
            else
            {
                newLobbyField.Text = "could not create lobby";
            }
        }
    }
}
