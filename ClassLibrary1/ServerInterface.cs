using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;


namespace ClassLibrary1
{
    [ServiceContract]
    public interface ServerInterface
    {
        [OperationContract]
        bool AddUser(string username);

        [OperationContract]
        void createLobby(string lobbyName, string ownerName);

        [OperationContract]
        void Logout(string username);

        [OperationContract]
        string[] ListLobbies();
    }
}
