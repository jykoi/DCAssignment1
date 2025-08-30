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
    
    public partial class LobbyRoom : Window
    {
        private string _thisLobbyName;
        private ClientServices _client;
        private Lobbies _lobbiesWindowInstance;
        public LobbyRoom(ClientServices client, string lobbyName, Lobbies lobbiesWindow)
        {
            InitializeComponent();
            _client = client;
            _thisLobbyName = lobbyName;
            _lobbiesWindowInstance = lobbiesWindow;
        }

        void LobbyRoom_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _lobbiesWindowInstance.Show();
            _client.serverChannel.LeaveLobby(_thisLobbyName, _client.Username);
        }
    }
}
