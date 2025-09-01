using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClientApp
{
    
    public partial class Lobbies : Window
    {
        private readonly ClientServices _client;
        private Task<string[]> _lobbyFetching;

        // Must pass ClientServices instance to preserve the existing connection
        public Lobbies(ClientServices clientServices)
        {
            InitializeComponent();
            _client = clientServices;

            // load when window is ready
            Loaded += (_, __) => LoadLobbies();
            Closing += Lobbies_Closing;
        }

        private async void LoadLobbies()
        {
            while (_client.IsConnected())
            {
                try
                {
                    // fetch lobby names
                    _lobbyFetching = Task.Run(GetLobbyNames);
                    string[] names = await _lobbyFetching;

                    // update UI
                    LobbiesList.ItemsSource = names.ToList();
                    Status.Text = $"Loaded {names.Length} lobby(ies).";
                }
                catch (Exception ex)
                {
                    Status.Text = "Failed in loading lobbies: " + ex.Message;
                }

                await Task.Delay(5000);
            }
        }

        private string[] GetLobbyNames()
        {
            
            return _client.serverChannel.GetLobbyNames() ?? Array.Empty<string>();
        }

        private void newLobbyBtn_Click(object sender, RoutedEventArgs e)
        {
            var newLobbyName = (newLobbyField.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newLobbyName))
            {
                newLobbyField.Text = "enter a lobby name";
                return;
            }

            bool created = _client.serverChannel.CreateLobby(newLobbyName, _client.Username);
            if (created)
            {
                newLobbyField.Text = "Created successfully";
                LoadNewLobby(newLobbyName);
            }
            else
            {
                newLobbyField.Text = "could not create lobby";
            }
        }

        private void LoadNewLobby(string lobbyName)
        {
            var win = new LobbyWindow(_client, lobbyName, this);
            win.Show();
            this.Hide();
        }


        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLobby = (LobbiesList.SelectedItem ?? string.Empty).ToString();

            if (string.IsNullOrWhiteSpace(selectedLobby))
            {
                MessageBox.Show("Please select a lobby to join.");
                return;
            }

            _client.serverChannel.JoinLobby(selectedLobby, _client.Username);         
            LoadNewLobby(selectedLobby);
        }

        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            _client.Logout();
            _client.Disconnect();
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Hide();
        }

        private void Lobbies_Closing(object sender, CancelEventArgs e)
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
