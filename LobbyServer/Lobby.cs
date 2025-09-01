using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ServerDLL;
using InterfaceLibrary;


namespace LobbyServer
{
    [DataContract]
    public class Lobby
    {
        [DataMember]
        private readonly List<string> _players = new List<string>();

        [DataMember]
        private readonly object _playersLock = new object();

        [DataMember]
        private string _name;

        [DataMember]
        private readonly object _chatLock = new object();

        [DataMember]
        private readonly List<ChatMessage> _messages = new List<ChatMessage>();

        [DataMember]
        private int _nextMessageId = 1;

        [DataMember]
        public string Name
        {
            get => _name;
            set => _name = (value ?? string.Empty).Trim();
        }

        //File sharing
        [DataMember]
        private readonly object _filesLock = new object();  // lock objects to make file operations threadsafe 

        [DataMember]
        private readonly List<Tuple<InterfaceLibrary.LobbyFileInfo, byte[]>> _files // Store both file metadata (ie. LobbyFileInfo), and raw bytes
            = new List<Tuple<InterfaceLibrary.LobbyFileInfo, byte[]>>();

        [DataMember]
        private int _nextFileId = 1;


        //this property makes the program crash even though it's now used anywhere

        //[DataMember]
        //public List<string> Players
        //{
        //    get
        //    {
        //        return new List<string>(_players);   
        //    }
        //}

        public Lobby(string name)
        {
            //_players = new List<string>();
            //_name = name;
            Name = name;
        }

        private Lobby()
        {

        }

        // used for instantiation during deserialization
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            
        }

        public bool AddPlayer(string username)
        {
            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username)) return false;

            lock (_playersLock)
            {
                if (_players.Contains(username)) return false;
                _players.Add(username);
                return true;
            }
        }

        public bool RemovePlayer(string username)
        {
            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username) || !_players.Contains(username)) return false;

            lock (_playersLock)
            {
                bool result = _players.Remove(username);
                return result;
            }
        }

        public string[] GetPlayersSnapshot()
        {
            
            return _players.ToArray();
            
        }

        public int AddLobbyMessage(string fromUser, string text)
        {
            var msg = new ChatMessage
            {
                Id = _nextMessageId++,
                FromUser = fromUser,
                Text = text,
                Timestamp = System.DateTime.UtcNow
            };

            lock (_chatLock)
            {
                _messages.Add(msg);
                // cap to last 500 messages
                if (_messages.Count > 500)
                    _messages.RemoveRange(0, _messages.Count - 500);
            }
            return msg.Id;
        }

        public List<ChatMessage> GetMessagesSince(int afterId, int max)
        {
            lock (_chatLock)
            {
                return _messages
                    .Where(m => m.Id > afterId)
                    .OrderBy(m => m.Id)
                    .Take(max)
                    .ToList();
            }
        }

        public int CurrentMaxId()
        {
            lock (_chatLock) return _messages.Count == 0 ? 0 : _messages[_messages.Count - 1].Id;
        }


        //File sharing methods
        public int AddLobbyFile(string uploadedBy, string fileName, byte[] content, string contentType)
        {
            if (string.IsNullOrWhiteSpace(uploadedBy) ||
                string.IsNullOrWhiteSpace(fileName) ||
                content == null || content.Length == 0 ||
                string.IsNullOrWhiteSpace(contentType))
                return 0;

            // only allow players in this specific lobby to upload
            lock (_playersLock)
            {
                if (!_players.Contains(uploadedBy))
                    return 0;
            }

            // Only allow images or text as per assignment spec
            if (!(contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                  contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)))
                return 0;

            var meta = new InterfaceLibrary.LobbyFileInfo
            {
                Id = _nextFileId++,
                FileName = fileName,
                ContentType = contentType,
                UploadedBy = uploadedBy,
                UploadedAt = System.DateTime.UtcNow
            };

            lock (_filesLock)
            {
                _files.Add(Tuple.Create(meta, content));
            }

            return meta.Id;
        }
        
        // Return metadata for files uploaded after a given ID
        public List<InterfaceLibrary.LobbyFileInfo> GetFilesSince(int afterId, int max)
        {
            if (max <= 0) max = 100;

            lock (_filesLock)
            {
                return _files
                    .Where(t => t.Item1.Id > afterId)
                    .OrderBy(t => t.Item1.Id)
                    .Take(max)
                    .Select(t => t.Item1)
                    .ToList();
            }
        }

        // Return the raw file bytes so a client can download/open it
        public byte[] DownloadFile(int fileId)
        {
            lock (_filesLock)
            {
                var item = _files.FirstOrDefault(t => t.Item1.Id == fileId);
                return item?.Item2;
            }
        }
        
        // To know the highest file ID in this lobby,
        public int CurrentMaxFileId()
        {
            lock (_filesLock)
            {
                return _files.Count == 0 ? 0 : _files[_files.Count-1].Item1.Id;
            }
        }

    }
}


    

