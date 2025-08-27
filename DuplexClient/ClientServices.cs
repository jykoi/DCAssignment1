using ClassLibrary1;
using LobbyServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ClientServices : IServerCallback
    {
        public ServerInterface serverChannel;
        private NetTcpBinding tcp;
        private string URL = "net.tcp://localhost:8200/DataService";
        private DuplexChannelFactory<ServerInterface> chanFactory;

        private List<string> _lobbies = new List<string>();
        //Fired when a new lobby is created
        public Action OnLobbyCreated;

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
            chanFactory = new DuplexChannelFactory<ServerInterface>(this, tcp, URL);
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

        public void Logout()
        {
            serverChannel.Logout(Username);
        }

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

        public void FetchLobbies()
        {
            var lobbies = serverChannel.GetLobbyNames().ToList();
            _lobbies = lobbies;
            // Notify that lobbies have been updated
            OnLobbyCreated?.Invoke();
        }
    }
}
