using LobbyServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace DuplexClient
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
            
        }

        public void LoadLobbies()
        {

            LobbiesList.ItemsSource = _client.Lobbies;

            Status.Text = $"Loaded {_client.Lobbies.Count} lobby(ies).";

        }

        private void LobbiesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //bind the event in the client to a method that reloads the lobbies list
            _client.OnLobbyCreated = () =>
            {
                Dispatcher.Invoke(() => LoadLobbies());
            };
            //fetch the lobby and load the lobbies
            _client.FetchLobbies();
        }


        private void newLobbyBtn_Click(object sender, RoutedEventArgs e)
        {
            string newLobbyName = newLobbyField.Text;
            if (_client.serverChannel.CreateLobby(newLobbyName, _client.Username))
            {
                newLobbyField.Text = "Created successfully";
                LoadNewLobby(newLobbyName);
                _client.CurrentLobbyName = newLobbyName;
            }
            else
            {
                newLobbyField.Text = "could not create lobby";
            }
        }

        private void LoadNewLobby(string lobbyName)
        {
            LobbyWindow lobbyWindow = new LobbyWindow(_client, lobbyName, this);
            lobbyWindow.Show();

            this.Hide();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLobby = (LobbiesList.SelectedItem ?? String.Empty).ToString();

            if (string.IsNullOrEmpty(selectedLobby))
            {
                MessageBox.Show("Please select a lobby to join.");
                return;
            }

            _client.serverChannel.JoinLobby(selectedLobby, _client.Username);
            _client.CurrentLobbyName = selectedLobby;
            LoadNewLobby(selectedLobby);

        }

        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            _client.Logout();
            _client.Disconnect();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        void Lobbies_Closing(object sender, CancelEventArgs e)
        {
            // Ensure the client disconnects when the window is closed
            if (_client.IsConnected())
            {
                _client.Logout();
                _client.Disconnect();
            }
        }

    }
}
