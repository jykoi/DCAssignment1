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
        private static readonly List<Lobby> lobbies = new List<Lobby>();
        private static readonly object LobbiesLock = new object();

        public static void AddLobby(Lobby lobby)
        {
            if (lobby == null) return;

            var name = (lobby.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            lock (LobbiesLock)
            {
                bool exists = lobbies.Any(l => string.Equals((l.Name ?? string.Empty).Trim(), name, StringComparison.Ordinal));

                if (!exists)
                {
                    lobbies.Add(lobby);
                }
                lobbies.Add(lobby);
            }
        }

        //readonly snapshot of lobbyNames for client display 
        public static string[] GetLobbyNamesSnapshot()
        {
            lock (LobbiesLock)
            {
                return lobbies
                    .Select(l => (l.Name ?? string.Empty).Trim())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToArray();
            }
        }

        //TO BE FINISHED? Helper for future steps ie. join/create validation
        public static bool LobbyExists(string lobbyName)
        {
            lobbyName = (lobbyName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(lobbyName)) return false;

            lock (LobbiesLock)
            {
                return lobbies.Any(l =>
                    string.Equals((l.Name ?? string.Empty).Trim(), lobbyName, StringComparison.Ordinal));
            }
        }    
        
    }
}
