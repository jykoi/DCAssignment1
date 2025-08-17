using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public class Lobby
    {
        private List<string> _players;
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<string> Players
        {
            get { return _players; }
        }

        public Lobby(string name)
        {
            _players = new List<string>();
            _name = name;
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
