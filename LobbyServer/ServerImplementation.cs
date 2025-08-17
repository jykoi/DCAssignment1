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

        public bool CheckUsername(string username)
        {
            bool result = AddUser(username);
            foreach (var user in UserManager.usernames)
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Current User Count: " + UserManager.usernames.Count);
            return result;
        }

        public bool AddUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || UserManager.usernames.Contains(username))
            {
                return false;
            }
            UserManager.usernames.Add(username);
            return true;
        }

        public void createLobby(string lobbyName, string ownerName)
        {
            Lobby lobby = new Lobby(lobbyName);
            lobby.AddPlayer(ownerName);
            LobbyManager.AddLobby(lobby);
        }
    }
}
