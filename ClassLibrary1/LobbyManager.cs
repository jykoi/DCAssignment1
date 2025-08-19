using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public static class LobbyManager
    {
        private static readonly List<Lobby> _lobbies = new List<Lobby>();
        private static readonly object LobbiesLock = new object();

        public static List<Lobby> Lobbies
        {
            get
            {
                return new List<Lobby>(_lobbies);
            }
        }

        public static void AddLobby(Lobby lobby)
        {
            if (lobby == null) return;

            var name = (lobby.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            lock (LobbiesLock)
            {
                bool exists = _lobbies.Any(l => string.Equals((l.Name ?? string.Empty).Trim(), name, StringComparison.Ordinal));

                if (!exists)
                {
                    _lobbies.Add(lobby);
                }
                // redundant line?
                //_lobbies.Add(lobby);
            }
        }

        //readonly snapshot of lobbyNames for client display 
        public static string[] GetLobbyNamesSnapshot()
        {
            
            return _lobbies
                .Select(l => (l.Name ?? string.Empty).Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToArray();
            
        }

        //public static List<Lobby> GetLobbiesSnapshot()
        //{
            
        //    return new List<Lobby>(_lobbies);
            
        //}

        public static string[] GetLobbyNames()
        {
                       
            return _lobbies
                .Select(l => (l.Name ?? string.Empty).Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToArray();
            
        }

        //TO BE FINISHED? Helper for future steps ie. join/create validation
        public static bool LobbyExists(string lobbyName)
        {
            lobbyName = (lobbyName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(lobbyName)) return false;

            lock (LobbiesLock)
            {
                return _lobbies.Any(l =>
                    string.Equals((l.Name ?? string.Empty).Trim(), lobbyName, StringComparison.Ordinal));
            }
        }    
        
    }
}
