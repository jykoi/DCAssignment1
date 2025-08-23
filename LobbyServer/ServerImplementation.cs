using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LobbyServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerSession)]
    internal class ServerImplementation : ServerInterface
    {

        public ServerImplementation()
        {

        }

        private static readonly object UsersLock = new object();

        public bool AddUser(string username)
        {
            //normalising the username once eg User and User are the SAME.
            username = (username ?? string.Empty).Trim();

            lock (UsersLock)
            {
                bool success = UserManager.AddUser(username);
                foreach (var user in UserManager.Usernames)
                {
                    Console.WriteLine(user);
                }
                Console.WriteLine("Current User Count: " + UserManager.Usernames.Count);
                return success;
            }

        }

        public bool CreateLobby(string lobbyName, string ownerName, out Lobby lobby)
        {
            lobby = null;
            if (string.IsNullOrWhiteSpace(lobbyName) || LobbyManager.LobbyExists(lobbyName))
            {
                return false;
            }

            lobby = new Lobby(lobbyName);
            LobbyManager.AddLobby(lobby);
            JoinLobby(lobbyName, ownerName);


            foreach (var lobbyItem in LobbyManager.Lobbies)
            {
                //for testing...
                Console.WriteLine(lobbyItem.Name + ":" + lobbyItem.GetPlayersSnapshot()[0]);
            }

            return true;
        }

        public void Logout(string username)
        {
            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username)) return;

            lock (UsersLock)
            {
                if (UserManager.Usernames.Remove(username))
                {
                    Console.WriteLine($"User '{username}' logged out.");
                }
                else
                {
                    Console.WriteLine($"Logout requested for '{username}' but no such user exists.");
                }
                Console.WriteLine("Current User Count: " + UserManager.Usernames.Count);

            }

        }

        public List<Lobby> ListLobbies()
        {
            return LobbyManager.Lobbies;
        }

        public string[] GetLobbyNames()
        {
            return LobbyManager.GetLobbyNames();
        }

        public void JoinLobby(string lobbyName, string username)
        {
            List<Lobby> lobbies = LobbyManager.Lobbies;
            bool lobbyFound = false;
            for (int i = 0; i < lobbies.Count && !lobbyFound; i++)
            {
                if (lobbies[i].Name.Equals(lobbyName, StringComparison.Ordinal))
                {
                    if (lobbies[i].AddPlayer(username))
                    {
                        Console.WriteLine($"User '{username}' joined lobby '{lobbyName}'.");
                        foreach (var player in lobbies[i].GetPlayersSnapshot())
                        {
                            Console.WriteLine($"Player in lobby '{lobbyName}': {player}");
                        }
                    }
                    lobbyFound = true;
                }
            }
        }

        public Lobby GetLobbyByName(string lobbyName)
        {
            Lobby lobby = LobbyManager.Lobbies
                .FirstOrDefault(l => l.Name.Equals(lobbyName, StringComparison.Ordinal));
            return lobby;
        }

        public void LeaveLobby(string lobbyName, string username)
        {
            Lobby lobby = GetLobbyByName(lobbyName);
            if (lobby == null || string.IsNullOrWhiteSpace(username)) return;
            if (lobby.RemovePlayer(username))
            {
                Console.WriteLine($"User '{username}' left lobby '{lobby.Name}'.");
            }
            else
            {
                Console.WriteLine($"User '{username}' could not leave lobby '{lobby.Name}' - an error occured.");
            }
        }
    }
}
