using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using LobbyServer;

namespace ClassLibrary1
{
    [ServiceContract]
    public interface ServerInterface
    {
        [OperationContract]
        bool AddUser(string username);

        [OperationContract]
        bool CreateLobby(string lobbyName, string ownerName);


    }
}
