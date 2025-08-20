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
    /// Interaction logic for LobbyRoom.xaml
    /// </summary>
    public partial class LobbyRoom : Window
    {
        private Lobby _thisLobby;
        private ClientServices _client;
        public LobbyRoom(ClientServices client, Lobby lobby)
        {
            InitializeComponent();
            _client = client;
            _thisLobby = lobby;
        }
    }
}
