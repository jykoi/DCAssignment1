using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
        public string Name
        {
            get => _name;
            set => _name = (value ?? string.Empty).Trim();
        }

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
    }

}
