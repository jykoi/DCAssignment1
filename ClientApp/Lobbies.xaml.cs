using LobbyServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Task<List<string>> _lobbyFetching;

        //Must pass ClientServices instance to preserve the existing connection
        public Lobbies(ClientServices clientServices)
        {
            InitializeComponent();
            _client = clientServices;

            //load when window is ready
            Loaded += (_, __) => LoadLobbies();
        }

        private async void LoadLobbies()
        {
            while (true && _client.IsConnected())
            {
                try
                {
                    _lobbyFetching = new Task<List<string>>(() => GetLobbies());
                    _lobbyFetching.Start();
                    List<string> lobbies = await _lobbyFetching;
                    LobbiesList.ItemsSource = lobbies;

                    Status.Text = $"Loaded {lobbies.Count} lobby(ies).";
                }
                catch (Exception ex)
                {
                    Status.Text = "Failed in loading lobbies: " + ex.Message;
                }
                await Task.Delay(5000);
            }
        }

        private List<string> GetLobbies()
        {
            List<string> lobbies = _client.serverChannel.GetLobbyNames().ToList();
            return lobbies;
        }

        private void newLobbyBtn_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby;
            string newLobbyName = newLobbyField.Text;
            if (_client.serverChannel.CreateLobby(newLobbyName, _client.Username, out lobby))
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
            LobbyRoom lobbyRoom = new LobbyRoom(_client, lobbyName, this);
            lobbyRoom.Show();

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
