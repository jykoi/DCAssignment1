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
        bool CreateLobby(string lobbyName, string ownerName, out Lobby lobby);

        [OperationContract]
        void Logout(string username);

        [OperationContract]
        List<Lobby> ListLobbies();

        [OperationContract]
        string[] GetLobbyNames();

        [OperationContract]
        void JoinLobby(string lobbyName, string username);

        [OperationContract]
        Lobby GetLobbyByName(string lobbyName);

        [OperationContract]
        void LeaveLobby(string lobbyName, string username);
    }
}
