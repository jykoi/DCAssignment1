using System.ServiceModel;
using InterfaceLibrary;

namespace ServerDLL
{
    [ServiceContract]
    public interface ServerInterface
    {
        // Users
        [OperationContract]
        bool AddUser(string username);

        [OperationContract]
        void Logout(string username);

        // Lobbies 
        [OperationContract]
        bool CreateLobby(string lobbyName, string ownerName);

        [OperationContract]
        string[] GetLobbyNames();

        [OperationContract]
        void JoinLobby(string lobbyName, string username);

        [OperationContract]
        void LeaveLobby(string lobbyName, string username);

        [OperationContract]
        string[] GetPlayers(string lobbyName);

        // Lobby chat
        [OperationContract]
        bool PostLobbyMessage(string lobbyName, string fromUser, string text);

        [OperationContract]
        MessagesPage GetLobbyMessagesSince(string lobbyName, int afterId, int max = 100);

        // Private DM chat
        [OperationContract]
        bool SendPrivateMessage(string fromUser, string toUser, string text);

        [OperationContract]
        MessagesPage GetPrivateMessagesSince(string user1, string user2, int afterId, int max = 100);

        // File sharing
        [OperationContract]
        bool UploadLobbyFile(string lobbyName, string fromUser, string fileName, byte[] content, string contentType);

        [OperationContract]
        LobbyFileInfo[] GetLobbyFilesSince(string lobbyName, int afterId, int max = 100);

        [OperationContract]
        byte[] DownloadLobbyFile(string lobbyName, int fileId);

    }

    // inherits the base interface and adds duplex methods
    [ServiceContract(CallbackContract = typeof(IServerCallback))]
    public interface ServerInterfaceDuplex : ServerInterface
    {
        [OperationContract]
        void Subscribe();
        [OperationContract]
        void Unsubscribe();
    }

    public interface IServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void FetchLobbies();

        [OperationContract(IsOneWay = true)]
        void FetchLobbyMessages();
        [OperationContract(IsOneWay = true)]
        void FetchPlayersList();
    }
}
