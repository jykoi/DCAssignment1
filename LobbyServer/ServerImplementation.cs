using ServerDLL;
using InterfaceLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using LobbyServer;


namespace LobbyServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerSession)]
    internal class ServerImplementation : ServerInterface
    {
        private class Conversation
        {
            public readonly object Lock = new object();
            public readonly List<ChatMessage> Messages = new List<ChatMessage>();
            public int NextId = 1;
        }

        private static readonly ConcurrentDictionary<string, Conversation> _conversations
           = new ConcurrentDictionary<string, Conversation>();

        private static string ConvKey(string u1, string u2)
        {
            var a = string.CompareOrdinal(u1, u2) <= 0 ? u1 : u2;
            var b = string.CompareOrdinal(u1, u2) <= 0 ? u2 : u1;
            return $"{a}|{b}";
        }
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

        public bool CreateLobby(string lobbyName, string ownerName)
        {
            if (string.IsNullOrWhiteSpace(lobbyName) || LobbyManager.LobbyExists(lobbyName))
                return false;

            var lobby = new Lobby(lobbyName);
            LobbyManager.AddLobby(lobby);
            JoinLobby(lobbyName, ownerName);
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

        public bool PostLobbyMessage(string lobbyName, string fromUser, string text)
        {
            if (string.IsNullOrWhiteSpace(lobbyName) ||
                string.IsNullOrWhiteSpace(fromUser) ||
                string.IsNullOrWhiteSpace(text)) return false;

            var lobby = LobbyManager.GetLobbyByName(lobbyName); 
            if (lobby == null) return false;

            lobby.AddLobbyMessage(fromUser, text.Trim());
            return true;
        }

        public MessagesPage GetLobbyMessagesSince(string lobbyName, int afterId, int max = 100)
        {
            var lobby = LobbyManager.GetLobbyByName(lobbyName);
            if (lobby == null) return new MessagesPage { Items = new List<ChatMessage>(), LastId = afterId, HasMore = false };

            var items = lobby.GetMessagesSince(afterId, max);
            var last = items.Count == 0 ? afterId : items[items.Count - 1].Id;
            var hasMore = lobby.CurrentMaxId() > last;

            return new MessagesPage { Items = items, LastId = last, HasMore = hasMore };
        }

        // --------------- Private DM chat -----------------------------------

        public bool SendPrivateMessage(string fromUser, string toUser, string text)
        {
            if (string.IsNullOrWhiteSpace(fromUser) ||
                string.IsNullOrWhiteSpace(toUser) ||
                string.IsNullOrWhiteSpace(text)) return false;

            var key = ConvKey(fromUser, toUser);
            var conv = _conversations.GetOrAdd(key, _ => new Conversation());

            lock (conv.Lock)
            {
                conv.Messages.Add(new ChatMessage
                {
                    Id = conv.NextId++,
                    FromUser = fromUser,
                    Text = text.Trim(),
                    Timestamp = DateTime.UtcNow
                });

                if (conv.Messages.Count > 500)
                    conv.Messages.RemoveRange(0, conv.Messages.Count - 500);
            }
            return true;
        }

        public MessagesPage GetPrivateMessagesSince(string user1, string user2, int afterId, int max = 100)
        {
            var key = ConvKey(user1, user2);
            if (!_conversations.TryGetValue(key, out var conv))
                return new MessagesPage { Items = new List<ChatMessage>(), LastId = afterId, HasMore = false };

            List<ChatMessage> items;
            int maxId;
            lock (conv.Lock)
            {
                items = conv.Messages.Where(m => m.Id > afterId)
                                     .OrderBy(m => m.Id)
                                     .Take(max)
                                     .ToList();
                maxId = conv.Messages.Count == 0 ? 0 : conv.Messages[conv.Messages.Count - 1].Id;
            }
            var last = items.Count == 0 ? afterId : items[items.Count-1].Id;
            return new MessagesPage { Items = items, LastId = last, HasMore = maxId > last };
        }

        public string[] GetPlayers(string lobbyName)
        {
            var lobby = LobbyManager.Lobbies.FirstOrDefault(l => l.Name.Equals(lobbyName, StringComparison.OrdinalIgnoreCase));
            if (lobby == null)
                return Array.Empty<string>();

            
            return lobby.GetPlayersSnapshot().ToArray();
        }


    }
}
