using InterfaceLibrary;
using LobbyServer;
using ServerDLL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DuplexClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ClientServices : IServerCallback
    {
        public ServerInterfaceDuplex serverChannel;
        private NetTcpBinding tcp;
        private string URL = "net.tcp://localhost:8200/DataService";
        private DuplexChannelFactory<ServerInterfaceDuplex> chanFactory;

        private List<string> _lobbies = new List<string>();
        //These events are fired when the corresponding data is updated. This allows the UI to refresh.
        public Action OnLobbyCreated;
        public Action OnMessageSent;
        public Action OnPlayerJoined;
        public Action OnDMSent;
        public Action OnFileSent;

        // State variables to keep track of current lobby, messages, players, files, etc. Used together
        //with the events to update the UI.
        public string CurrentLobbyName = "";
        public int LastMsgId = 0;
        public MessagesPage CurrentLobbyMessages = null;

        public string Peer = "";
        public int LastDMId = 0;

        public string[] CurrentPlayers = Array.Empty<string>();
        public MessagesPage CurrentDMs = null;

        public int LastFileId = 0;
        public LobbyFileInfo[] CurrentLobbyFiles = null;

        public List<string> Lobbies
        {
            get { return _lobbies; }
        }

        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public ClientServices(string username)
        {
            
            this.username = username;
        }

        public void Connect()
        {
            tcp = new NetTcpBinding();
            chanFactory = new DuplexChannelFactory<ServerInterfaceDuplex>(this, tcp, URL);
            serverChannel = chanFactory.CreateChannel();
            serverChannel.Subscribe();
        }

        public void Disconnect()
        {
            if (serverChannel != null)
            {
                //close the channel and the factory
                serverChannel.Unsubscribe();
                ((ICommunicationObject)serverChannel).Close();
                chanFactory.Close();
            }
        }
        // removes the player from the players list
        public void Logout()
        {
            serverChannel.Logout(Username);
        }
        //checks if the client is still connected to the server
        public bool IsConnected()
        {
            // Check if the channel is still open
            if (serverChannel != null)
            {
                try
                {
                    return ((ICommunicationObject)serverChannel).State == CommunicationState.Opened;
                }
                catch (Exception)
                {
                    return false; // If an exception occurs, we assume the connection is not valid
                }
            }
            return false; // If serverChannel is null, we are not connected
        }
        //get the list of lobbies from the server
        public void FetchLobbies()
        {
            var lobbies = serverChannel.GetLobbyNames().ToList();
            _lobbies = lobbies;
            // Notify that lobbies have been updated
            OnLobbyCreated?.Invoke();
        }
        //fetch the messages from the current lobby
        public void FetchLobbyMessages()
        {
            
            var messages = serverChannel.GetLobbyMessagesSince(CurrentLobbyName, LastMsgId, 100);
            LastMsgId = messages.LastId;
            CurrentLobbyMessages = messages;
            OnMessageSent?.Invoke();
            
        }
        // fetch the players from the current lobby
        public void FetchPlayersList()
        {
            var players = serverChannel.GetPlayers(CurrentLobbyName) ?? Array.Empty<string>();
            CurrentPlayers = players;
            OnPlayerJoined?.Invoke();
            Trace.WriteLine("Lobby name: " + CurrentLobbyName);
        }
        //fetch private messages between the user and the peer
        public void FetchPrivateMessages()
        {
            var messages = serverChannel.GetPrivateMessagesSince(Username, Peer, LastDMId, 100);
            LastDMId = messages.LastId;
            CurrentDMs = messages;
            OnDMSent?.Invoke();
        }
        //fetch files from the current lobby
        public void FetchLobbyFiles()
        {
            var files = serverChannel.GetLobbyFilesSince(CurrentLobbyName, LastFileId, 100);
            CurrentLobbyFiles = files;
            
            OnFileSent?.Invoke();
        }

        //Downloads the file with the given fileId from the current lobby
        public byte[] DownloadLobbyFile(string lobby, int fileId)
            => serverChannel.DownloadLobbyFile(lobby, fileId);

        //Uploads a file to the current lobby
        public bool UploadLobbyFile(string lobby, string fromUser, string fileName, byte[] content, string contentType)
            => serverChannel.UploadLobbyFile(lobby, fromUser, fileName, content, contentType);

        
    }
}
