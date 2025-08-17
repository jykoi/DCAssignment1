using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public static class LobbyManager
    {
        private static List<Lobby> lobbies = new List<Lobby>();
        public static void AddLobby(Lobby lobby)
        {
            lobbies.Add(lobby);
        }
        
        
    }
}
