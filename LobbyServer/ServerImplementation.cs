using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerSession)]
    internal class ServerImplementation : ServerInterface
    {

        public ServerImplementation()
        {

        }

        public bool AddUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || UserManager.usernames.Contains(username))
            {
                return false;
            }
            UserManager.usernames.Add(username);
            foreach (var user in UserManager.usernames)
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Current User Count: " + UserManager.usernames.Count);
            return true;
        }

        // checks if the lobby name is unique & valid and adds it to the list of lobbies
        public bool CreateLobby(string lobbyName, string ownerName)
        {
            
            if (string.IsNullOrWhiteSpace(lobbyName) || LobbyManager.ContainsLobbyName(lobbyName))
            {
                return false;
            }

            Lobby lobby = new Lobby(lobbyName);
            lobby.AddPlayer(ownerName);
            LobbyManager.AddLobby(lobby);

            foreach (var lobbyItem in LobbyManager.lobbies)
            {
                Console.WriteLine(lobbyItem.Name + ":" + lobbyItem.Players[0]);
            }

            return true;
        }
    }
}
