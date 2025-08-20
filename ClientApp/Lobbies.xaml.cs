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
                    _lobbyFetching = new Task<List<Lobby>>(() => PollLobbies());
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

        private List<Lobby> PollLobbies()
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
            }
            else
            {
                newLobbyField.Text = "could not create lobby";
            }
        }
    }
}
