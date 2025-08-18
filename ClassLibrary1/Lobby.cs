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
        private List<string> _players;
        
        [DataMember]
        private string _name;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [DataMember]
        public List<string> Players
        {
            get { return _players; }
        }

        public Lobby(string name)
        {
            _players = new List<string>();
            _name = name;
        }

        public Lobby()
        {

        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _players = new List<string>();
        }

        public void AddPlayer(string username)
        {
            if (!_players.Contains(username))
            {
                _players.Add(username);
            }
        }
    }
}
