using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public static class LobbyManager
    {
        private static List<Lobby> _lobbies = new List<Lobby>();
        public static List<Lobby> lobbies
        {
            get { return _lobbies; }
        }
        public static void AddLobby(Lobby lobby)
        {
            _lobbies.Add(lobby);
        }
        
        public static bool ContainsLobbyName(string lobbyName)
        {
            foreach (var lobby in _lobbies)
            {
                if (lobby.Name.Equals(lobbyName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}
