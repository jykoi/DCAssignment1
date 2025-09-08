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
    internal class ServerImplementation : ServerInterface, ServerInterfaceDuplex
    {
        // maintain a dicitonary of connected duplex clients
        private static Dictionary<string, IServerCallback> _clients = new Dictionary<string, IServerCallback>();
        private static readonly object _duplexLock = new object();
        private bool isDuplexClient = false;
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
                //check that the user has been successfully added
                bool success = UserManager.AddUser(username);
                //logging...
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
            //check that the name is valid
            if (string.IsNullOrWhiteSpace(lobbyName) || LobbyManager.LobbyExists(lobbyName)) {
                return false;
            }

            //create & add the owner to the lobby
            var lobby = new Lobby(lobbyName);
            LobbyManager.AddLobby(lobby);
            JoinLobby(lobbyName, ownerName);
            //notify all duplex clients to refresh their lobby list
            foreach (var client in _clients)
            {
                client.Value.FetchLobbies();
                
            }
            return true;
        }

        public void Logout(string username)
        {
            //normalise the username
            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username)) return;

            lock (UsersLock)
            {
                //remove the user and check that it was successful
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

        public string[] GetLobbyNames()
        {
            return LobbyManager.GetLobbyNames();
        }

        public void JoinLobby(string lobbyName, string username)
        {
            //fetch all the lobbies
            List<Lobby> lobbies = LobbyManager.Lobbies;
            bool lobbyFound = false;
            //find the lobby that matches the given lobbyName
            for (int i = 0; i < lobbies.Count && !lobbyFound; i++)
            {
                if (lobbies[i].Name.Equals(lobbyName, StringComparison.Ordinal))
                {
                    //add self to lobby
                    if (lobbies[i].AddPlayer(username))
                    {
                        //logging...
                        Console.WriteLine($"User '{username}' joined lobby '{lobbyName}'.");
                        foreach (var player in lobbies[i].GetPlayersSnapshot())
                        {
                            Console.WriteLine($"Player in lobby '{lobbyName}': {player}");
                        }
                    }
                    lobbyFound = true;
                }
            }
            //notify all duplex clients to refresh their players list
            foreach (var client in _clients)
            {
                client.Value.FetchPlayersList();
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
            //get the lobby by name
            Lobby lobby = GetLobbyByName(lobbyName);
            // return if lobby is null or username is invalid
            if (lobby == null || string.IsNullOrWhiteSpace(username)) return;
            //try to remove the player from the lobby
            if (lobby.RemovePlayer(username))
            {
                Console.WriteLine($"User '{username}' left lobby '{lobby.Name}'.");
            }
            else
            {
                Console.WriteLine($"User '{username}' could not leave lobby '{lobby.Name}' - an error occured.");
            }
            foreach (var client in _clients)
            {
                client.Value.FetchPlayersList();
            }
        }

        // register the client callback channel
        public void Subscribe()
        {
            try
            {
                //get the session ID of the client
                string clientId = OperationContext.Current.SessionId;
                //get the callback channel
                IServerCallback callback = OperationContext.Current.GetCallbackChannel<IServerCallback>();
                lock (_duplexLock)
                {
                    if (!_clients.ContainsKey(clientId))
                    {
                        //add the callback channel to the dictionary
                        _clients.Add(clientId, callback);
                        isDuplexClient = true;
                    }
                }
            } 
            catch
            {

            }
            
        }

        // delete the client callback channel
        public void Unsubscribe()
        {
            if (isDuplexClient)
            {
                // get the session ID of the client
                string clientId = OperationContext.Current.SessionId;
                lock (_duplexLock)
                {
                    //if the session id is found, remove it from the dictionary
                    if (_clients.ContainsKey(clientId))
                    {
                        _clients.Remove(clientId);
                    }
                }
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

            foreach (var client in _clients)
            {
                client.Value.FetchLobbyMessages();
            }
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

            foreach (var client in _clients)
            {
                client.Value.FetchPrivateMessages();
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

        //get players from a particular lobby
        public string[] GetPlayers(string lobbyName)
        {
            //find the lobby by name
            var lobby = LobbyManager.Lobbies.FirstOrDefault(l => l.Name.Equals(lobbyName, StringComparison.OrdinalIgnoreCase));
            if (lobby == null)
                return Array.Empty<string>();

            //return a snapshot of the players in that lobby
            return lobby.GetPlayersSnapshot().ToArray();
        }


        // File sharing
        public bool UploadLobbyFile(string lobbyName, string fromUser, string fileName, byte[] content, string contentType)
        {
            if (string.IsNullOrWhiteSpace(lobbyName) ||string.IsNullOrWhiteSpace(fromUser) || string.IsNullOrWhiteSpace(fileName) ||
                content == null || content.Length == 0 || string.IsNullOrWhiteSpace(contentType))
                return false;

            var lobby = LobbyManager.GetLobbyByName(lobbyName);
            if (lobby == null) return false;

            var newId = lobby.AddLobbyFile(fromUser, fileName.Trim(), content, contentType.Trim());
            if (newId <= 0) return false;

            // Tell any duplex clients to refresh their view and pull latest files (reuse FetchLobbyMessages(), the existing callback)
            foreach (var client in _clients)
            {
                try { client.Value.FetchLobbyFiles(); } catch { /*ignore if one client's callback channel is broken to prevent server crashing*/ }
            }

            return true;
        }

        // Return file metadata since a given ID (for polling or refresh)
        public InterfaceLibrary.LobbyFileInfo[] GetLobbyFilesSince(string lobbyName, int afterId, int max = 100)
        {
            var lobby = LobbyManager.GetLobbyByName(lobbyName);
            if (lobby == null) return new InterfaceLibrary.LobbyFileInfo[0];

            var items = lobby.GetFilesSince(afterId, max);
            return items.ToArray();
        }

        // Return actual file bytes for a specific file ID
        public byte[] DownloadLobbyFile(string lobbyName, int fileId)
        {
            var lobby = LobbyManager.GetLobbyByName(lobbyName);
            if (lobby == null) return null;

            return lobby.DownloadFile(fileId);
        }


    }
}
