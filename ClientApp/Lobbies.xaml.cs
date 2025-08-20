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
        private Task<List<Lobby>> _lobbyFetching;

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

            while (true)
            {
                try
                {
                    _lobbyFetching = new Task<List<Lobby>>(() => GetLobbies());
                    _lobbyFetching.Start();
                    List<Lobby> lobbies = await _lobbyFetching;
                    LobbiesList.ItemsSource = lobbies.Select(l => l.Name).ToList();

                    Status.Text = $"Loaded {lobbies.Count} lobby(ies).";
                    

                }
                catch (Exception ex)
                {
                    Status.Text = "Failed in loading lobbies: " + ex.Message;
                }
                await Task.Delay(5000);
            }
        }

        private List<Lobby> GetLobbies()
        {
            List<Lobby> lobbies = _client.serverChannel.ListLobbies();
            return lobbies;
        }

        private void newLobbyBtn_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby;
            string newLobbyName = newLobbyField.Text;
            if (_client.serverChannel.CreateLobby(newLobbyName, _client.Username, out lobby))
            {
                newLobbyField.Text = "Created successfully";
                LoadNewLobby(lobby);
            }
            else
            {
                newLobbyField.Text = "could not create lobby";
            }
        }

        private void LoadNewLobby(Lobby lobby)
        {
            LobbyRoom lobbyRoom = new LobbyRoom(_client, lobby);
            lobbyRoom.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string selectedLobby = (LobbiesList.SelectedItem ?? String.Empty).ToString();

            if (string.IsNullOrEmpty(selectedLobby))
            {
                MessageBox.Show("Please select a lobby to join.");
                return;
            }

            _client.serverChannel.JoinLobby(selectedLobby, _client.Username);
            LoadNewLobby(_client.serverChannel.GetLobbyByName(selectedLobby));

        }
    }
}
