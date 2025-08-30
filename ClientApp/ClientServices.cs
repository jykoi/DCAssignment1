using ServerDLL;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class ClientServices
    {
        public ServerInterface serverChannel;
        private NetTcpBinding tcp;
        private string URL = "net.tcp://localhost:8100/DataService";
        private ChannelFactory<ServerInterface> chanFactory;

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
            chanFactory = new ChannelFactory<ServerInterface>(tcp, URL);
            serverChannel = chanFactory.CreateChannel();
        }

        public void Disconnect()
        {
            if (serverChannel != null)
            {
                //close the channel and the factory
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
                    return false; 
                }
            }
            return false; 
        }

        // Lobby chat
        public bool PostLobbyMessage(string lobby, string fromUser, string text)
            => serverChannel.PostLobbyMessage(lobby, fromUser, text);

        public MessagesPage GetLobbyMessagesSince(string lobby, int afterId, int max = 100)
            => serverChannel.GetLobbyMessagesSince(lobby, afterId, max);

        // DMs
        public bool SendPrivateMessage(string fromUser, string toUser, string text)
            => serverChannel.SendPrivateMessage(fromUser, toUser, text);

        public MessagesPage GetPrivateMessagesSince(string u1, string u2, int afterId, int max = 100)
            => serverChannel.GetPrivateMessagesSince(u1, u2, afterId, max);
    }
}

